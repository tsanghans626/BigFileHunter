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
    private readonly SparkleUpdater _sparkle;

    /// <summary>
    /// The appcast URL where update information is hosted
    /// Hosted on GitHub Pages
    /// </summary>
    private const string AppcastUrl = "https://tsanghans626.github.io/BigFileHunter/updates/appcast.xml";

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

        _sparkle = new SparkleUpdater(AppcastUrl, signatureVerifier)
        {
            UIFactory = new UIFactory(),
            RelaunchAfterUpdate = false  // MSI installer will relaunch the app
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
