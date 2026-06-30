using SwitchBack.Config;
using SwitchBack.Core;
using SwitchBack.SystemServices;

namespace SwitchBack.App;

public sealed class ConversionCoordinator
{
    private readonly TextConverter _textConverter;
    private readonly MixedLayoutTextConverter _mixedTextConverter;
    private readonly WindowsInputLanguageService _inputLanguageService;
    private readonly ClipboardService _clipboardService;
    private readonly KeyboardInputService _keyboardInputService;
    private readonly SemaphoreSlim _conversionLock = new(1, 1);

    public ConversionCoordinator(
        TextConverter textConverter,
        MixedLayoutTextConverter mixedTextConverter,
        WindowsInputLanguageService inputLanguageService,
        ClipboardService clipboardService,
        KeyboardInputService keyboardInputService)
    {
        _textConverter = textConverter;
        _mixedTextConverter = mixedTextConverter;
        _inputLanguageService = inputLanguageService;
        _clipboardService = clipboardService;
        _keyboardInputService = keyboardInputService;
    }

    public event EventHandler<string>? Error;

    public event EventHandler<ConversionCompletedEventArgs>? Converted;

    public async Task ConvertSelectionAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (!await _conversionLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        ClipboardSnapshot? snapshot = null;

        try
        {
            var foregroundLayout = _inputLanguageService.GetForegroundLayout();
            var triggerVirtualKey = GlobalHotkeyService.GetVirtualKey(settings.Hotkey.Key);
            await _keyboardInputService.WaitForHotkeyReleaseAsync(triggerVirtualKey, cancellationToken);

            snapshot = _clipboardService.Capture();
            var sequenceNumber = _clipboardService.GetSequenceNumber();

            _keyboardInputService.SendCopy();
            var selectedText = await _clipboardService.WaitForChangedTextAsync(
                sequenceNumber,
                TimeSpan.FromMilliseconds(1_000),
                cancellationToken);

            if (string.IsNullOrEmpty(selectedText))
            {
                return;
            }

            var converted = settings.ConversionMode == ConversionMode.FollowWindowsLanguage
                ? ConvertUsingWindowsLayout(selectedText, foregroundLayout, settings)
                : ConvertUsingBuiltInMapping(selectedText, settings.ConversionMode);

            if (converted.Output == selectedText || !_clipboardService.TrySetText(converted.Output))
            {
                return;
            }

            _keyboardInputService.SendPaste();
            await Task.Delay(settings.ClipboardRestoreDelayMs, cancellationToken);
            Converted?.Invoke(this, new ConversionCompletedEventArgs(converted.Count));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Error?.Invoke(this, exception.Message);
        }
        finally
        {
            if (settings.RestoreClipboard && snapshot is not null && !_clipboardService.TryRestore(snapshot))
            {
                Error?.Invoke(this, "Windows did not allow the previous Clipboard contents to be restored.");
            }

            _conversionLock.Release();
        }
    }

    private (string Output, int Count) ConvertUsingWindowsLayout(
        string input,
        InputLayoutInfo? foregroundLayout,
        AppSettings settings)
    {
        if (foregroundLayout is null)
        {
            throw new InvalidOperationException("Windows did not report the active keyboard language.");
        }

        var behavior = settings.MixedTextPolicy == MixedTextPolicy.SwapBothLayouts
            ? LayoutConversionBehavior.SwapBothLayouts
            : LayoutConversionBehavior.TargetLanguageOnly;

        var candidates = OrderSourceCandidates(
            _inputLanguageService.GetInstalledLayouts(),
            foregroundLayout,
            settings.InputLayouts);
        LayoutConversionResult? bestResult = null;

        foreach (var source in candidates)
        {
            try
            {
                var sourceToTarget = new WindowsLayoutCharacterMapper(source, foregroundLayout);
                var targetToSource = new WindowsLayoutCharacterMapper(foregroundLayout, source);
                var result = _mixedTextConverter.Convert(input, sourceToTarget, targetToSource, behavior);

                if (bestResult is null || result.ConvertedCharacterCount > bestResult.ConvertedCharacterCount)
                {
                    bestResult = result;
                }
            }
            catch (NotSupportedException)
            {
            }
        }

        if (bestResult is null)
        {
            throw new InvalidOperationException("No compatible Windows keyboard layout was found.");
        }

        return (bestResult.Output, bestResult.ConvertedCharacterCount);
    }

    private static IReadOnlyList<InputLayoutInfo> OrderSourceCandidates(
        IReadOnlyList<InputLayoutInfo> installedLayouts,
        InputLayoutInfo target,
        InputLayoutSettings preferredPair)
    {
        var preferredSourceId = string.Equals(target.Id, preferredPair.LayoutAId, StringComparison.OrdinalIgnoreCase)
            ? preferredPair.LayoutBId
            : string.Equals(target.Id, preferredPair.LayoutBId, StringComparison.OrdinalIgnoreCase)
                ? preferredPair.LayoutAId
                : string.Empty;

        return installedLayouts
            .Where(layout => layout.IsSupported &&
                !string.Equals(layout.Id, target.Id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(layout =>
                string.Equals(layout.Id, preferredSourceId, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(layout => layout.LanguageTag.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private (string Output, int Count) ConvertUsingBuiltInMapping(string input, ConversionMode mode)
    {
        var direction = mode switch
        {
            ConversionMode.EnglishToThai => ConversionDirection.EnglishToThai,
            ConversionMode.ThaiToEnglish => ConversionDirection.ThaiToEnglish,
            _ => ConversionDirection.Auto
        };
        var result = _textConverter.Convert(input, direction);
        return (result.Output, result.ConvertedCharacterCount);
    }
}
