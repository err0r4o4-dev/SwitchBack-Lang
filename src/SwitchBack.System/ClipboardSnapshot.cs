using System.Windows;

namespace SwitchBack.SystemServices;

public sealed record ClipboardSnapshot(IDataObject? Data, bool WasCaptured);
