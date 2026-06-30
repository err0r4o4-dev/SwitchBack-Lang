using System.Runtime.InteropServices;
using System.Windows;

namespace SwitchBack.SystemServices;

public sealed class ClipboardService
{
    public ClipboardSnapshot Capture()
    {
        var wasCaptured = TryRetry(() => Clipboard.GetDataObject(), out var data);
        return new ClipboardSnapshot(data, wasCaptured);
    }

    public uint GetSequenceNumber() => GetClipboardSequenceNumber();

    public async Task<string?> WaitForChangedTextAsync(
        uint previousSequenceNumber,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (GetSequenceNumber() != previousSequenceNumber)
            {
                return TryReadText();
            }

            await Task.Delay(25, cancellationToken);
        }

        return null;
    }

    public string? TryReadText()
    {
        return Retry(() => Clipboard.ContainsText(TextDataFormat.UnicodeText)
            ? Clipboard.GetText(TextDataFormat.UnicodeText)
            : null);
    }

    public bool TrySetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return Retry(() =>
        {
            Clipboard.SetText(text, TextDataFormat.UnicodeText);
            return true;
        });
    }

    public bool TryRestore(ClipboardSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (!snapshot.WasCaptured)
        {
            // Never clear or overwrite the Clipboard when the original capture failed.
            return true;
        }

        if (snapshot.Data is null)
        {
            return Retry(() =>
            {
                Clipboard.Clear();
                return true;
            });
        }

        return Retry(() =>
        {
            Clipboard.SetDataObject(snapshot.Data, true);
            return true;
        });
    }

    private static T Retry<T>(Func<T> action)
    {
        return TryRetry(action, out var result) ? result : default!;
    }

    private static bool TryRetry<T>(Func<T> action, out T result)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            try
            {
                result = action();
                return true;
            }
            catch (COMException)
            {
                if (attempt == 7)
                {
                    break;
                }

                Thread.Sleep(20 * (attempt + 1));
            }
        }

        result = default!;
        return false;
    }

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();
}
