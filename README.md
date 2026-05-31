# Pomidoras

A minimal desktop Pomodoro timer built with Avalonia and .NET 9.

The purpose of the project was to learn some basic C#.

## Features

- **Pomodoro timer** - cycles through work, short & long breaks
- **Playback controls** - start, stop, and switch between timer modes
- **Always on top** - keep the timer visible over other windows
- **Continuous mode** - automatically advance through work and break cycles
- **Compact UI** - small window (160×90) with only essential information & controls

## Build

Requires .NET 9.0 SDK.

```bash
dotnet build Pomidoras.sln
```

Run tests:

```bash
dotnet test Pomidoras.sln
```

Run the app:

```bash
dotnet run --project Pomidoras/Pomidoras.csproj
```

## Configuration

Settings are stored in:

- **Windows:** `%APPDATA%\Pomidoras\config.json`
- **Linux:** `~/.config/Pomidoras/config.json`
- **macOS:** `~/Library/Application Support/Pomidoras/config.json`

The file is created automatically on first launch with sensible defaults.

**NOTE**: The configuration file is not updated through app controls, so consider it "startup defaults".
