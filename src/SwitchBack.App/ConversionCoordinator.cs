using SwitchBack.Config;
using SwitchBack.Core;
using SwitchBack.SystemServices;

namespace SwitchBack.App;

public sealed class ConversionCoordinator
{
    private readonly TextConverter _textConverter;
    private readonly ClipboardService _clipboardService;
    private readonly KeyboardInputService _keyboardInputService;
    private readonly SemaphoreSlim _conversionLock = new(1, 1);

    public ConversionCoordinator(
        TextConverter textConverter,
        ClipboardService clipboardService,
        KeyboardInputService keyboardInputService)
    {
        _textConverter = textConverter;
        _clipboardService = clipboardService;
        _keyboardInputService = keyboardInputService;
    }

    public event EventHandler<string>? Error;

    public event EventHandler<ConversionResult>? Converted;

    public async Task ConvertSelectionAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (!await _conversionLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        ClipboardSnapshot? snapshot = null;

        try
        {
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

            var result = _textConverter.Convert(selectedText, MapDirection(settings.ConversionMode));
            if (!result.Changed || !_clipboardService.TrySetText(result.Output))
            {
                return;
            }

            _keyboardInputService.SendPaste();
            await Task.Delay(settings.ClipboardRestoreDelayMs, cancellationToken);
            Converted?.Invoke(this, result);
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

    private static ConversionDirection MapDirection(ConversionMode mode) => mode switch
    {
        ConversionMode.EnglishToThai => ConversionDirection.EnglishToThai,
        ConversionMode.ThaiToEnglish => ConversionDirection.ThaiToEnglish,
        _ => ConversionDirection.Auto
    };
}
