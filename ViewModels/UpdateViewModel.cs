using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetSparkleUpdater;
using NetSparkleUpdater.UI.Avalonia;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.Enums;

namespace BigFileHunter.ViewModels;

/// <summary>
/// Manages automatic update checking using NetSparkleUpdater
/// </summary>
public class UpdateViewModel
{
    /// <summary>
    /// Gets the current application version
    /// </summary>
    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version("1.0.0");

    private readonly SparkleUpdater _sparkle;

    /// <summary>
    /// The appcast URL where update information is hosted
    /// Hosted on GitHub Pages
    /// </summary>
    private const string BaseAppcastUrl = "https://tsanghans626.github.io/BigFileHunter/updates/";

    /// <summary>
    /// Gets the architecture-specific appcast URL for the current process.
    /// Returns architecture-specific URLs (appcast-windows-x64.xml, etc.) for known architectures,
    /// or falls back to the combined appcast.xml for unknown architectures.
    /// </summary>
    private static string GetAppcastUrl()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        string archIdentifier;

        switch (architecture)
        {
            case Architecture.X64:
                archIdentifier = "windows-x64";
                break;
            case Architecture.X86:
                archIdentifier = "windows-x86";
                break;
            case Architecture.Arm64:
                archIdentifier = "windows-arm64";
                break;
            default:
                // Fallback to combined appcast for unknown architectures (backward compatibility)
                return $"{BaseAppcastUrl}appcast.xml";
        }

        return $"{BaseAppcastUrl}appcast-{archIdentifier}.xml";
    }

    /// <summary>
    /// Ed25519 public key for signature verification
    /// IMPORTANT: Replace this with your actual public key!
    /// Generate using: dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator
    ///              netsparkle-generate-appcast --generate-keys
    ///              netsparkle-generate-appcast --export
    /// Then add ED25519_PUBLIC_KEY to GitHub Secrets for the build workflow
    /// </summary>
    private const string Ed25519PublicKey = "Dfe70R+IeIWzVhWA2lvLEBKIG4JAZwkct5HTo1BMKl0=";

    public UpdateViewModel()
    {
        // Initialize Sparkle updater with Ed25519 signature verification
        // SecurityMode.Strict ensures both MSI file and appcast.xml signatures are verified
        var signatureVerifier = new Ed25519Checker(
            SecurityMode.Strict,
            Ed25519PublicKey
        );

        string appcastUrl = GetAppcastUrl();  // Use architecture-specific appcast URL

        _sparkle = new SparkleUpdater(appcastUrl, signatureVerifier)
        {
            UIFactory = new UIFactory(),
            RelaunchAfterUpdate = false,  // MSI installer will relaunch the app
            LogWriter = new LogWriter(LogWriterOutputMode.Console | LogWriterOutputMode.Trace)
        };
    }

    /// <summary>
    /// Check for updates at user request (e.g., when user clicks "Check for Updates" button)
    /// </summary>
    /// <param name="showUI">Whether to show UI if no update is available</param>
    public async Task CheckForUpdatesAsync(bool showUI = true)
    {
        await _sparkle.CheckForUpdatesAtUserRequest(showUI);
    }

    /// <summary>
    /// Check for updates and return status for custom UI handling
    /// Returns tuple: (updateAvailable, latestVersion, statusMessage)
    /// </summary>
    public async Task<(bool UpdateAvailable, string? LatestVersion, string StatusMessage)>
        CheckForUpdatesWithCallbackAsync()
    {
        try
        {
            // Check quietly without showing UI
            var status = await _sparkle.CheckForUpdatesQuietly();

            if (status.Updates != null && status.Updates.Count > 0)
            {
                // Update available - get the version info
                var latestVersion = status.Updates[0].Version;
                return (true, latestVersion, $"发现新版本 {latestVersion}");
            }
            else
            {
                // No updates available
                return (false, null, "当前已是最新版本");
            }
        }
        catch (Exception ex)
        {
            return (false, null, $"检查更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// Start automatically checking for updates in the background
    /// Call this when the application starts
    /// </summary>
    public void StartAutomaticallyCheckingForUpdates()
    {
        // Check for updates every 24 hours (default)
        // You can customize this interval: _sparkle.UpdateCheckInterval = TimeSpan.FromHours(12);
        // The 'true' parameter means do an initial check immediately on startup
        _sparkle.StartLoop(true);
    }

    /// <summary>
    /// Stop the automatic update checking loop
    /// Call this when the application shuts down
    /// </summary>
    public void StopAutomaticallyCheckingForUpdates()
    {
        _sparkle.StopLoop();
    }
}
