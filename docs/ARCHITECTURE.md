# SwitchBack architecture and MVP guide

## 1. System overview

SwitchBack stays in the Windows notification area. When the user selects text,
switches Windows to the intended target language, and presses the configured global hotkey, the application waits for the hotkey
keys to be released, snapshots the Clipboard, sends `Ctrl+C`, converts the copied
text by physical keyboard position, sends `Ctrl+V`, and then attempts to restore
the original Clipboard data.

No network request is part of this flow. Text exists only in the foreground
application, the Windows Clipboard, and SwitchBack process memory.

## 2. Project boundaries

```text
SwitchBack.App       WPF settings, tray icon, lifecycle, workflow orchestration
  ├─ SwitchBack.Core      pure conversion and direction detection
  ├─ SwitchBack.Config    settings models and local JSON persistence
  └─ SwitchBack.System    Win32 hotkey/input, Clipboard, Windows startup

SwitchBack.Tests     unit tests for the platform-independent Core
```

`SwitchBack.Core` targets plain `net8.0` and does not reference WPF or Win32.
This keeps the important conversion behavior fast and easy to test. Platform
code is isolated in `SwitchBack.System`; WPF composes those services in
`ConversionCoordinator`.

## 3. Global hotkey

`GlobalHotkeyService` calls the Win32 `RegisterHotKey` API using the hidden WPF
window handle. WPF's `HwndSource` hook receives `WM_HOTKEY` and forwards it to
the conversion coordinator. `MOD_NOREPEAT` prevents key-repeat from triggering
overlapping conversions. A `SemaphoreSlim` adds a second re-entry guard.

Registration can fail when another program owns the same combination. Settings
validate the key and show the Win32 error rather than silently failing.

## 4–5. Copy and paste

Windows has no universal API for "get the selected text" across Chrome, Word,
Electron applications, and native controls. The interoperable approach is to
leave focus in the target application and simulate the standard shortcuts:

1. `KeyboardInputService` waits until the original hotkey is released.
2. `SendInput` emits Ctrl-down, C-down, C-up, Ctrl-up.
3. `ClipboardService` waits for `GetClipboardSequenceNumber` to change.
4. The converted Unicode text is written to the Clipboard.
5. `SendInput` emits Ctrl-down, V-down, V-up, Ctrl-up.

Some secure fields, games, remote-desktop sessions, or applications with custom
shortcut behavior may refuse these operations.

## 6. Clipboard safety

Before copying, `ClipboardService` captures the existing `IDataObject`. Clipboard
calls retry briefly because Windows allows only one process to open the Clipboard
at a time. After paste, SwitchBack waits for a configurable delay (350 ms by
default) and calls `SetDataObject` to restore the snapshot.

Restoration is deliberately described as best-effort. Delayed-rendered formats,
large images, Clipboard managers, and concurrent Clipboard writes can make exact
restoration impossible. A future hardening step is a native OLE snapshot service
with per-format size limits and a policy that does not overwrite a Clipboard that
another application changed after SwitchBack pasted.

## 7. System tray

`TrayIconService` uses `System.Windows.Forms.NotifyIcon`, which works alongside
WPF's dispatcher. Double-click opens settings. The context menu supports Settings,
Enabled/Paused, and Exit. Closing the settings window hides it; Exit disposes the
hotkey and tray icon and then shuts down WPF.

## 8–9. Settings and JSON config

`MainWindow` edits enabled state, conversion mode, modifiers/key, Clipboard
restoration, notifications, and start-with-Windows. `JsonConfigService` writes an
indented JSON file atomically through a temporary file:

```json
{
  "SchemaVersion": 2,
  "Enabled": true,
  "UiLanguage": "System",
  "ConversionMode": "FollowWindowsLanguage",
  "MixedTextPolicy": "TargetLanguageOnly",
  "InputLayouts": {
    "LayoutAId": "<Windows-HKL-A>",
    "LayoutBId": "<Windows-HKL-B>"
  },
  "RestoreClipboard": true,
  "ClipboardRestoreDelayMs": 350,
  "Hotkey": {
    "Control": true,
    "Shift": true,
    "Alt": false,
    "Windows": false,
    "Key": "Space"
  },
  "Preferences": {
    "MinimizeToTray": true,
    "ShowNotifications": true,
    "StartWithWindows": false
  }
}
```

Invalid or unreadable JSON falls back to safe defaults. Config contains no
selected text. Windows startup uses the current-user `Run` registry key and does
not require administrator rights.

## 10. Thai Kedmanee mapping

`KeyboardMapper` contains aligned unshifted and shifted key tables. Each English
key maps to the Thai character printed on the same physical Kedmanee key. Reverse
tables are generated from the same source, so mappings remain maintainable and
consistent. `TextConverter` walks Unicode characters, maps known characters, and
preserves spaces, emoji, and unsupported characters.

Example: `l ; y l f u 8 i y [` maps by position to `ส ว ั ส ด ี ค ร ั บ`.

## 11. Automatic direction

The default mode reads the foreground thread's active Windows keyboard layout.
Within the configured pair, the active layout becomes the target and the other
layout becomes the source. `WindowsLayoutCharacterMapper` uses Windows' installed
layout tables for generic direct-keyboard conversion. IME profiles are marked as
limited and unsupported composition sequences are preserved rather than guessed.

The legacy text detector counts Thai Unicode characters and English letters. More Thai
selects Thai→English; otherwise English→Thai. This deterministic heuristic works
for the common wrong-layout case and avoids sending data anywhere. Mixed-language
or punctuation-only selections can be ambiguous, so Settings also offers fixed
direction modes.

## 12–13. MVP code and tests

The end-to-end workflow is in `src/SwitchBack.App/ConversionCoordinator.cs`.
Mapping and conversion are in `src/SwitchBack.Core`. Tests verify the sample,
reverse conversion, unsupported characters, mapping keys, and direction detection:

```powershell
dotnet test SwitchBack.sln -c Release
```

System-level Copy/Paste behavior should later be covered by Windows UI automation
tests because unit tests should not take control of a developer's real Clipboard.

## 14–16. EXE, portable zip, and installer

A normal framework-dependent build is:

```powershell
dotnet build SwitchBack.sln -c Release
```

`scripts/Publish-Portable.ps1` performs a self-contained publish, so end users
do not need to install .NET. `scripts/Build-Installer.ps1` builds both
`win-x86` and `win-x64` portable packages.

The same script invokes Inno Setup using `installer/SwitchBack.iss` and creates
one installer that selects x86/x64 files automatically.
The per-user installer writes to `%LOCALAPPDATA%\Programs\SwitchBack`, creates an
uninstaller, and does not request elevation.

## 17. GitHub Release

The workflow `.github/workflows/release.yml` runs tests, publishes the portable
build, compiles the installer, and creates a release when a `v*` tag is pushed:

```powershell
git tag v0.3.0
git push origin v0.3.0
```

Before a public release, add a custom icon, code-sign the executable/installer,
test on a clean Windows VM, and publish SHA-256 checksums.

## 18. Privacy, permissions, antivirus, and conflicts

- Do not log selections, Clipboard contents, passwords, or conversion history by
  default. Keep future history local, opt-in, clearable, and easy to disable.
- Clipboard managers may observe intermediate copied/converted text even though
  SwitchBack itself is offline. The privacy notice states this limitation.
- A normal process cannot inject input into an administrator-elevated target due
  to Windows UIPI. Avoid requesting admin globally; run both at the same integrity
  level when conversion in an elevated app is genuinely needed.
- Global hotkeys and `SendInput` can resemble automation tools to antivirus
  heuristics. Use `RegisterHotKey` rather than a keylogger-style global hook, keep
  builds reproducible, sign releases, publish source/checksums, and submit false
  positives to vendors instead of adding exclusions.
- Hotkey conflicts are expected. Registration errors are surfaced and the user
  can choose another combination.
- Restore timing differs by application. Very slow or remote applications may
  need a longer Clipboard restore delay.
- Test Chrome, Edge, Firefox, Office, LINE, Discord, VS Code, Windows Search,
  Notepad, elevated apps, multiple monitors, RDP, and Clipboard-history enabled.

## MVP boundary and next phases

The MVP includes auto direction because it is small and testable, but does not
store history, auto-update, preview, or blacklist applications. Recommended next
order: Undo/preview, app allow/deny list, hardened Clipboard ownership checks,
signed releases, optional local history, then auto-update. Dark mode is independent
and can be added without changing the conversion pipeline.
