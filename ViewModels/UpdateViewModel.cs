using System.Threading.Tasks;
using NetSparkleUpdater;
using NetSparkleUpdater.UI.Avalonia;

namespace BigFileHunter.ViewModels;

/// <summary>
/// Manages automatic update checking using NetSparkleUpdater
/// </summary>
public class UpdateViewModel
{
    private readonly SparkleUpdater _sparkle;

    /// <summary>
    /// The appcast URL where update information is hosted
    /// TODO: Replace with your actual appcast URL when hosting the file
    /// </summary>
    private const string AppcastUrl = "https://your-domain.com/updates/appcast.xml";

    /// <summary>
    /// Ed25519 public key for signature verification
    /// TODO: Generate your own key pair and replace this
    /// You can generate keys using NetSparkleUpdater's SignatureGenerator tool
    /// </summary>
    private const string Ed25519PublicKey = "YOUR_ED25519_PUBLIC_KEY_HERE";

    public UpdateViewModel()
    {
        // Initialize Sparkle updater with no signature verification (for development only)
        // TODO: Configure proper signature verification for production
        _sparkle = new SparkleUpdater(AppcastUrl, null)
        {
            UIFactory = new UIFactory(),
            RelaunchAfterUpdate = false
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
