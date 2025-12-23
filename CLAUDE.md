# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BigFileHunter is a .NET 9.0 console application that scans directories to identify and report on disk usage patterns. It builds a tree structure of folders and tracks both cumulative size (includes subdirectories) and direct size (files only within the folder).

## Build and Run

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build -c Release
```

## Architecture

### Core Components

- **Program.cs** - Entry point. Prompts user for folder path, initiates scan, and displays top N results.
- **ScanService.cs** - Main scanning logic using iterative stack-based traversal (not recursive to avoid stack overflow on deep directory trees).
- **FolderNode.cs** - Tree node model representing a directory. Key properties:
  - `Size` - Cumulative size including all descendants
  - `GetDirectSize()` - Size of files directly in this folder (excluding subdirectories)
  - `Children` - Child folders
  - `Parent` - Parent folder reference
- **Exceptions.cs** - Custom exception types implementing `IAppException` interface with error codes and timestamps.

### Key Design Patterns

- **Aggregating size upward**: When a file is found, its size is added to the current node AND propagated up through all ancestors (lines 34-39 in ScanService.cs).
- **Stack-based traversal**: Uses `Stack<FolderNode>` instead of recursion to handle deeply nested directory structures.
- **UnauthorizedAccessException handling**: Silently skips system-protected folders during scan.

### Size Calculation

Each node's `Size` property accumulates:
1. All files directly in that folder
2. All files in all descendant folders (via upward propagation)

To get direct file size only (excluding subdirectories), use `GetDirectSize()`.

## Development Notes

- Target framework: .NET 9.0
- Implicit usings enabled
- Nullable reference types enabled
- Language: C# with collection expressions (`[]` instead of `new List<T>()`)
