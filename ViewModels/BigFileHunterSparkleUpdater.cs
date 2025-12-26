using System.Diagnostics;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;
using BigFileHunter.Logging;

namespace BigFileHunter.ViewModels;

/// <summary>
/// Custom SparkleUpdater that properly handles MSI installation with UAC elevation
/// </summary>
public class BigFileHunterSparkleUpdater : SparkleUpdater
{
    private readonly Logging.ILogger _logger;

    public BigFileHunterSparkleUpdater(
        string appcastUrl,
        ISignatureVerifier signatureVerifier,
        Logging.ILogger logger)
        : base(appcastUrl, signatureVerifier)
    {
        _logger = logger;
        RelaunchAfterUpdate = false; // MSI installer will handle restart
    }

    /// <summary>
    /// Intercept the installation process to handle MSI files properly
    /// </summary>
    protected override async Task RunDownloadedInstaller(string downloadedFilePath)
    {
        try
        {
            _logger.LogInfo($"[RunDownloadedInstaller] Preparing to install: {downloadedFilePath}");

            // First, try to use the file directly - it might be the MSI without extension
            if (File.Exists(downloadedFilePath))
            {
                var fileInfo = new FileInfo(downloadedFilePath);
                _logger.LogInfo($"[RunDownloadedInstaller] File exists: {downloadedFilePath}");
                _logger.LogInfo($"[RunDownloadedInstaller] File size: {fileInfo.Length} bytes");

                // Try to launch it directly as an MSI installer
                // Windows can identify MSI files by their content, not just extension
                await RunMsiInstaller(downloadedFilePath);
                return;
            }

            // If the file doesn't exist, search for BigFileHunter MSI files in the same directory
            var directory = Path.GetDirectoryName(downloadedFilePath);
            if (string.IsNullOrEmpty(directory))
            {
                _logger.LogError($"[RunDownloadedInstaller] Could not get directory from path");
                return;
            }

            _logger.LogInfo($"[RunDownloadedInstaller] Searching for BigFileHunter MSI in: {directory}");
            var msiFiles = Directory.GetFiles(directory, "BigFileHunter-*.msi");

            if (msiFiles.Length > 0)
            {
                string msiPath = msiFiles[0];
                _logger.LogInfo($"[RunDownloadedInstaller] Found MSI file: {msiPath}");
                await RunMsiInstaller(msiPath);
                return;
            }

            _logger.LogError($"[RunDownloadedInstaller] No BigFileHunter MSI files found");
            await base.RunDownloadedInstaller(downloadedFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[RunDownloadedInstaller] Exception: {ex.Message}");
            _logger.LogError($"[RunDownloadedInstaller] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Run MSI installer with proper UAC elevation and process isolation
    /// </summary>
    private async Task RunMsiInstaller(string msiPath)
    {
        try
        {
            _logger.LogInfo($"[RunMsiInstaller] Starting MSI installation: {msiPath}");

            _logger.LogInfo($"[RunMsiInstaller] File exists: {File.Exists(msiPath)}");
            _logger.LogInfo($"[RunMsiInstaller] File size: {new FileInfo(msiPath).Length} bytes");

            // Use msiexec.exe to install the MSI file
            // This works even if the file doesn't have .msi extension
            var startInfo = new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                UseShellExecute = true,      // Required for UAC elevation
                Verb = "runas",                // Request administrator privileges
                Arguments = $"/i \"{msiPath}\" /passive /norestart"
            };

            _logger.LogInfo($"[RunMsiInstaller] Starting process: msiexec.exe /i \"{msiPath}\" /passive /norestart");

            var process = Process.Start(startInfo);

            if (process == null)
            {
                _logger.LogError("[RunMsiInstaller] Process.Start returned null");
                return;
            }

            _logger.LogInfo($"[RunMsiInstaller] MSI process started with ID: {process.Id}");

            // Give the process a moment to start
            await Task.Delay(500);

            // Don't wait for exit - just check if it's still running
            // This allows our app to close and not interfere with installation
            bool hasExited = process.HasExited;
            _logger.LogInfo($"[RunMsiInstaller] Process has exited: {hasExited}");

            if (!hasExited)
            {
                _logger.LogInfo("[RunMsiInstaller] MSI installer is running. Application can now close safely.");
            }
            else
            {
                _logger.LogError("[RunMsiInstaller] MSI process exited immediately");
            }

            process.Dispose();
            _logger.LogInfo("[RunMsiInstaller] Process disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[RunMsiInstaller] Exception: {ex.GetType().Name} - {ex.Message}");
            _logger.LogError($"[RunMsiInstaller] Stack trace: {ex.StackTrace}");
        }
    }
}
