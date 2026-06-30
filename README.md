# SwitchBack-Lang

SwitchBack is a Windows background utility that fixes text typed with the wrong
Thai/English keyboard layout.

Select `l;ylfu8iy[` in any application, switch Windows to Thai, and press `Ctrl + Shift + Space`.
SwitchBack replaces it with `สวัสดีครับ` (and can convert in the other direction).

## MVP features

- Verified English QWERTY ↔ Thai Kedmanee conversion
- Generic position mapping for installed, non-IME Windows keyboard layouts
- Direction follows the active Windows input language by default
- Mixed text can convert toward the active language or swap both layouts
- Thai/English UI with Windows-language default and English fallback
- Global, configurable hotkey
- System tray controls and settings window
- Best-effort Clipboard restoration
- Local JSON configuration
- Optional start with Windows
- 100% offline processing; no history or telemetry

## Requirements

- Windows 10/11 x64
- .NET 8 SDK for development
- Inno Setup 6 only when building the installer

## Build and test

```powershell
dotnet restore SwitchBack.sln
dotnet test SwitchBack.sln -c Release
dotnet build SwitchBack.sln -c Release
```

Run the development build:

```powershell
dotnet run --project src/SwitchBack.App/SwitchBack.App.csproj
```

## Package

Create a self-contained x64 portable zip (the user does not need .NET installed):

```powershell
./scripts/Publish-Portable.ps1
```

Create x86/x64 portable zips and the universal Inno Setup installer:

```powershell
./scripts/Build-Installer.ps1 -Version 0.3.0
```

Artifacts are written to `artifacts/`. Pushing a tag such as `v0.3.0` runs the
GitHub Release workflow.

The installer automatically selects x86 or x64 binaries. Its language dialog
defaults to the Windows display language. Complex IME-based input methods (for
example Chinese/Japanese composition) are marked as limited and only characters
that Windows can map directly are converted.

## Documentation

- [Architecture and implementation guide](docs/ARCHITECTURE.md)
- [Multilingual conversion flow](docs/MULTI_LANGUAGE_FLOW.md)
- [Privacy notice](PRIVACY.md)

## Branches

- `main`: stable, releasable code
- `develop`: integration branch for ongoing work
