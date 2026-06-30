# Multilingual conversion flow

SwitchBack separates three independent language concepts:

1. Installer language
2. Application UI language
3. Keyboard-layout conversion pair

The installer offers Thai and English, defaults from the Windows display/UI
language, and passes the selection to the first application launch. The app UI
supports System, English, and Thai; unsupported display languages fall back to
English.

## Install and first-run flow

```text
Start universal installer
  -> detect Windows UI language
  -> user confirms Thai or English installer language
  -> detect x86/x64-compatible operating system
  -> install the matching self-contained binary
  -> launch SwitchBack with the selected UI language
  -> enumerate installed Windows input profiles
  -> prefer English + Thai when both exist
  -> otherwise choose the first two direct keyboard layouts
  -> if fewer than two selectable layouts exist, start paused
```

SwitchBack never installs Windows language packs or input methods. That can
require network access and changes system-wide user preferences. Users add or
remove input languages through Windows Settings.

## Runtime conversion flow

```text
User selects text in the foreground application
  -> user switches Windows to the language they intended to type
  -> user presses the single global hotkey
  -> capture the foreground thread's active keyboard layout
  -> resolve it against configured layout A/B
  -> active layout becomes target; the other layout becomes source
  -> copy selected text
  -> map source characters by physical key position into target layout
  -> apply mixed-text policy
  -> paste the result
  -> restore the previous Clipboard on a best-effort basis
```

Example with active Thai layout:

```text
Target language only: l;ylfu สวัสดี -> สวัสดี สวัสดี
Swap both layouts:    l;ylfu สวัสดี -> สวัสดี l;ylfu
```

## Capability levels

- Verified: US QWERTY and Thai Kedmanee behavior is covered by explicit unit
  tests and a Windows-layout integration test.
- Generic: direct Windows keyboard layouts are mapped with `VkKeyScanEx`,
  `MapVirtualKeyEx`, and `ToUnicodeEx` using the installed HKL handles.
- Limited: profiles reported by Windows as IMEs remain selectable because modern
  Windows can report ordinary layouts through the IME subsystem. Only characters
  that `VkKeyScanEx` and `ToUnicodeEx` can translate directly are converted;
  composition sequences are preserved.

Dead keys, AltGr, ligatures, and multi-character mappings are accepted only when
Windows can translate them without changing keyboard state. Generic support is
not marketed as verified until a layout-specific test pack exists.

## Missing or additional languages

- English and Thai are not mandatory.
- With two supported direct layouts, those layouts can form the primary pair.
- With one supported layout, SwitchBack installs but starts paused.
- With more than two layouts, the user chooses the primary pair in Settings.
- If the active foreground layout is outside the configured pair, conversion is
  cancelled with a notification rather than guessing and corrupting text.
- UI languages not bundled with SwitchBack fall back to English; conversion
  language support does not depend on the UI language.

## Packaging

- `SwitchBack-win-x86-portable.zip`: legacy 32-bit Windows build
- `SwitchBack-win-x64-portable.zip`: standard 64-bit Windows build
- `SwitchBack-Setup-<version>.exe`: one installer containing both builds and
  selecting the correct one automatically

ARM64 currently receives the x64 build through Windows x64 compatibility. A
native `win-arm64` package should be released only after testing on real ARM64
hardware.
