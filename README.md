# BigFileHunter

A disk usage analyzer for Windows that scans directories to identify and report on disk usage patterns. Built with .NET 9.0 and Avalonia UI.

## Features

- Visual tree view of folder structures with size information
- Cumulative size tracking (includes all subdirectories)
- Direct file size analysis (files only within the folder)
- Multi-architecture support (x64, x86, ARM64)
- Built-in auto-update support using NetSparkle
- Windows MSI installer packages

## Requirements

- .NET 9.0 SDK
- Windows 10/11
- WiX Toolset v5 (for building MSI installers only)

## Building

### Build the Application

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build -c Release
```

### Build MSI Installers

To build Windows MSI installers, you need to install WiX Toolset v5:

```bash
# Install WiX Toolset v5
dotnet tool install --global wix --version 5.0.2

# Install WiX UI extension
wix extension add WixToolset.UI.wixext/5.0.2
```

#### Step 1: Publish the Application

Publish for each target architecture:

```bash
# x64
dotnet publish BigFileHunter.csproj -c Release -r win-x64 --self-contained -o ./publish/win-x64

# x86
dotnet publish BigFileHunter.csproj -c Release -r win-x86 --self-contained -o ./publish/win-x86

# ARM64
dotnet publish BigFileHunter.csproj -c Release -r win-arm64 --self-contained -o ./publish/win-arm64
```

#### Step 2: Build MSI Installers

```bash
# Build x64 MSI
dotnet build BigFileHunter.Setup/BigFileHunter.Setup.csproj -c Release -p:Platform=x64

# Build x86 MSI
dotnet build BigFileHunter.Setup/BigFileHunter.Setup.csproj -c Release -p:Platform=x86

# Build ARM64 MSI
dotnet build BigFileHunter.Setup/BigFileHunter.Setup.csproj -c Release -p:Platform=ARM64
```

The output MSI files will be named with version and architecture:
- `BigFileHunter-{version}-x64.msi`
- `BigFileHunter-{version}-x86.msi`
- `BigFileHunter-{version}-ARM64.msi`

#### Custom Build Options

You can override the publish path when building:

```bash
dotnet build BigFileHunter.Setup/BigFileHunter.Setup.csproj -c Release -p:Platform=x64 -p:PublishPath=../publish/win-x64/
```

## Releasing

### Automated Release (GitHub Actions)

Releases are automated via GitHub Actions. To create a new release:

#### Option 1: Tag-based Release

```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0
```

The workflow will:
1. Build the application for all architectures
2. Create MSI installers (x64, x86, ARM64)
3. Create ZIP archives
4. Sign all packages with Ed25519
5. Generate appcast.xml for auto-updates
6. Create a GitHub release with all artifacts
7. Deploy appcast files to GitHub Pages

#### Option 2: Manual Trigger

1. Go to **Actions** → **Build Windows MSI Installers**
2. Click **Run workflow**
3. Enter the version number (e.g., `1.0.0`)
4. Click **Run workflow**

### Version Management

The version is managed in `Directory.Build.props`:

```xml
<PropertyGroup>
  <Version>1.0.1</Version>
</PropertyGroup>
```

Update this file before creating a release.

### Secrets Required

The workflow requires the following GitHub secret:

- `ED25519_PRIVATE_KEY`: Private key for signing appcast.xml (NetSparkle auto-updates)

### Artifacts

Each release produces:

| Artifact | Description |
|----------|-------------|
| `BigFileHunter-{version}-x64.msi` | Windows x64 installer |
| `BigFileHunter-{version}-x86.msi` | Windows x86 installer |
| `BigFileHunter-{version}-ARM64.msi` | Windows ARM64 installer |
| `BigFileHunter-{version}-win-x64.zip` | Portable x64 archive |
| `BigFileHunter-{version}-win-x86.zip` | Portable x86 archive |
| `BigFileHunter-{version}-win-arm64.zip` | Portable ARM64 archive |
| `appcast.xml` | Auto-update feed |
| `appcast.xml.signature` | Signed appcast for verification |

## Architecture

- **Program.cs** - Entry point with user prompts for folder path and scan initiation
- **ScanService.cs** - Stack-based directory traversal to avoid recursion limits
- **FolderNode.cs** - Tree node model with cumulative and direct size tracking
- **Exceptions.cs** - Custom exception types with error codes and timestamps

## Size Calculation

Each node's `Size` property accumulates:
1. All files directly in that folder
2. All files in all descendant folders (via upward propagation)

To get direct file size only (excluding subdirectories), use `GetDirectSize()`.

## License

Copyright © 2024
