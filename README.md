# SwitchBack-Lang

SwitchBack is a Windows background utility that fixes text typed with the wrong
Thai/English keyboard layout.

Select `l;ylfu8iy[` in any application and press `Ctrl + Shift + Space`.
SwitchBack replaces it with `สวัสดีครับ` (and can convert in the other direction).

## MVP features

- English QWERTY ↔ Thai Kedmanee conversion
- Automatic direction detection or a fixed direction
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

Create a self-contained portable zip (the user does not need .NET installed):

```powershell
./scripts/Publish-Portable.ps1
```

Create the portable zip and Inno Setup installer:

```powershell
./scripts/Build-Installer.ps1 -Version 0.1.0
```

Artifacts are written to `artifacts/`. Pushing a tag such as `v0.1.0` runs the
GitHub Release workflow.

## Documentation

- [Architecture and implementation guide](docs/ARCHITECTURE.md)
- [Privacy notice](PRIVACY.md)

## Branches

- `main`: stable, releasable code
- `develop`: integration branch for ongoing work
