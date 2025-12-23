////////////////////////////////////////////////////////////////////
// BigFileHunter Build Script
// Uses Cake (https://cakebuild.net/) to automate building and packaging
////////////////////////////////////////////////////////////////////

#tool "nuget:?package=NuGet.CommandLine&version=6.8.0"
#addin "nuget:?package=Cake.FileHelpers&version=6.1.3"

////////////////////////////////////////////////////////////////////
// ARGUMENTS
////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("version", "1.0.0");

////////////////////////////////////////////////////////////////////
// PREPARATION
////////////////////////////////////////////////////////////////////

var artifactsDir = Directory("./artifacts");
var publishDir = Directory("./publish");

////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    Information("Starting BigFileHunter build v{0}...", version);
    Information("Configuration: {0}", configuration);
    Information("Target Framework: .NET 9.0");
});

Teardown(ctx =>
{
    Information("Build finished!");
});

////////////////////////////////////////////////////////////////////
// TASKS
////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans build output directories")
    .Does(() =>
{
    CleanDirectories("./bin/" + configuration);
    CleanDirectories("./obj/" + configuration);
    CleanDirectories("./**/bin/" + configuration);
    CleanDirectories("./**/obj/" + configuration);
    CleanDirectories(publishDir);
    CleanDirectories(artifactsDir);

    Information("Cleaned build directories");
});

Task("Restore")
    .Description("Restores NuGet packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetRestore("./BigFileHunter.sln");
    Information("Restored NuGet packages");
});

Task("Build")
    .Description("Builds the solution")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var settings = new DotNetBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = new DotNetMSBuildSettings()
            .SetVersion(version)
            .SetAssemblyVersion(version)
            .SetFileVersion(version)
    };

    DotNetBuild("./BigFileHunter.csproj", settings);
    Information("Built solution");
});

Task("Publish-Windows-x64")
    .Description("Publishes for Windows x64")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        Runtime = "win-x64",
        SelfContained = true,
        OutputDirectory = publishDir + Directory("win-x64")
    };

    DotNetPublish("./BigFileHunter.csproj", settings);
    Information("Published for Windows x64");
});

Task("Publish-Windows-x86")
    .Description("Publishes for Windows x86 (32-bit)")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        Runtime = "win-x86",
        SelfContained = true,
        OutputDirectory = publishDir + Directory("win-x86")
    };

    DotNetPublish("./BigFileHunter.csproj", settings);
    Information("Published for Windows x86");
});

Task("Publish-Windows-ARM64")
    .Description("Publishes for Windows ARM64")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        Runtime = "win-arm64",
        SelfContained = true,
        OutputDirectory = publishDir + Directory("win-arm64")
    };

    DotNetPublish("./BigFileHunter.csproj", settings);
    Information("Published for Windows ARM64");
});

Task("Publish-All-Windows")
    .Description("Publishes for all Windows architectures")
    .IsDependentOn("Publish-Windows-x64")
    .IsDependentOn("Publish-Windows-x86")
    .IsDependentOn("Publish-Windows-ARM64");

Task("Create-MSI")
    .Description("Creates MSI installers using WiX (Windows only)")
    .IsDependentOn("Publish-All-Windows")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Information("Creating MSI installers...");

    // Build WiX project for each architecture
    var architectures = new[] { "x64", "x86", "ARM64" };

    foreach (var arch in architectures)
    {
        Information("Building MSI for {0}...", arch);

        // Note: This requires WiX Toolset to be installed
        // Install with: dotnet tool install --global wix
        var msbuildSettings = new MSBuildSettings
        {
            Configuration = configuration,
            MSBuildPlatform = MSBuildPlatform.x64,
            ToolVersion = MSBuildToolVersion.Default,
            ArgumentCustomization = args => args
                .Append("/p:Platform=" + arch)
                .Append("/p:OutputPath=./publish/" + arch)
        };

        MSBuild("./BigFileHunter.Setup/BigFileHunter.Setup.csproj", msbuildSettings);
    }

    Information("MSI installers created");
});

Task("Zip-Artifacts")
    .Description("Creates ZIP archives for portable distribution")
    .IsDependentOn("Publish-All-Windows")
    .Does(() =>
{
    EnsureDirectoryExists(artifactsDir);

    var platforms = new[] { "win-x64", "win-x86", "win-arm64" };

    foreach (var platform in platforms)
    {
        var sourceDir = publishDir + Directory(platform);
        var zipFile = artifactsDir + File($"BigFileHunter-{version}-{platform}.zip");

        Information("Creating ZIP for {0}...", platform);
        Zip(sourceDir, zipFile);
    }

    Information("Created ZIP archives");
});

Task("Default")
    .Description("Default target - publishes Windows binaries")
    .IsDependentOn("Publish-All-Windows");

Task("Rebuild")
    .Description("Rebuilds everything")
    .IsDependentOn("Clean")
    .IsDependentOn("Default");

////////////////////////////////////////////////////////////////////
// EXECUTION
////////////////////////////////////////////////////////////////////

RunTarget(target);
