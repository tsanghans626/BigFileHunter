# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BigFileHunter is a .NET 9.0 Avalonia desktop application for Windows that scans directories to identify and report on disk usage patterns. It builds a tree structure of folders with visual representation and tracks both cumulative size (includes subdirectories) and direct size (files only within the folder). Includes auto-update support via NetSparkle.

## Build and Run

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build -c Release

# Publish for specific architecture (self-contained)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish/win-x64
dotnet publish -c Release -r win-x86 --self-contained -o ./publish/win-x86
dotnet publish -c Release -r win-arm64 --self-contained -o ./publish/win-arm64

# Build MSI installers (requires WiX Toolset v5)
dotnet build BigFileHunter.Setup/BigFileHunter.Setup.csproj -c Release -p:Platform=x64
dotnet build BigFileHunter.Setup/BigFileHunter.Setup.csproj -c Release -p:Platform=x86
dotnet build BigFileHunter.Setup/BigFileHunter.Setup.csproj -c Release -p:Platform=ARM64
```

## Architecture

### UI Framework: Avalonia MVVM Pattern

The application follows the Model-View-ViewModel (MVVM) pattern using Avalonia:

- **Views/MainWindow.axaml** - UI markup (Avalonia XAML)
- **Views/MainWindow.axaml.cs** - Code-behind that wires up the ViewModel
- **ViewModels/MainWindowViewModel.cs** - Main business logic and UI state management
- **ViewModels/UpdateViewModel.cs** - Auto-update logic using NetSparkle
- **App.axaml.cs** - Application entry point and Avalonia initialization

### Core Components

- **Program.cs** - Avalonia app builder entry point
- **ScanService.cs** - Main scanning logic using iterative stack-based traversal (not recursive to avoid stack overflow on deep directory trees)
- **FolderNode.cs** - Tree node model representing a directory. Key properties:
  - `Size` - Cumulative size including all descendants
  - `GetDirectSize()` - Size of files directly in this folder (excluding subdirectories)
  - `Children` - Child folders (can be sorted via `SortChildrenBySize()`)
  - `Parent` - Parent folder reference
- **Exceptions.cs** - Custom exception types implementing `IAppException` interface with error codes and timestamps

### Key Design Patterns

- **MVVM with INotifyPropertyChanged**: MainWindowViewModel exposes observable properties and commands for UI binding
- **Aggregating size upward**: When a file is found, its size is added to the current node AND propagated up through all ancestors
- **Stack-based traversal**: Uses `Stack<FolderNode>` instead of recursion to handle deeply nested directory structures
- **Background scanning**: Directory scanning runs on `Task.Run()` with UI updates dispatched via `Dispatcher.UIThread.Post()`
- **UnauthorizedAccessException handling**: Silently skips system-protected folders during scan

### Size Calculation

Each node's `Size` property accumulates:
1. All files directly in that folder
2. All files in all descendant folders (via upward propagation)

To get direct file size only (excluding subdirectories), use `GetDirectSize()`.

## Version Management

Version is centralized in `Directory.Build.props`:
```xml
<PropertyGroup>
  <Version>1.0.1</Version>
</PropertyGroup>
```

Update this file before creating a release. It is inherited by both the main project and the WiX installer project.

## Auto-Update System

Uses NetSparkleUpdater with Ed25519 signature verification:
- Appcast URL: `https://tsanghans626.github.io/BigFileHunter/updates/appcast.xml`
- Public key is stored in `UpdateViewModel.cs` (line 30) - currently a placeholder
- GitHub Actions workflow signs both appcast.xml and MSI files during release
- Update checks run automatically on startup (24-hour interval) and via manual "Check for Updates" button

## Development Notes

- Target framework: .NET 9.0
- UI Framework: Avalonia 11.3.3 with Fluent theme
- Implicit usings enabled
- Nullable reference types enabled
- Language: C# with collection expressions (`[]` instead of `new List<T>()`)
- Multi-architecture support: x64, x86, ARM64
- Custom `RelayCommand` implementation for MVVM command binding (defined in MainWindowViewModel.cs:188-209)
