using System.Threading.Tasks;

namespace InterviewAssistant.Core.Interfaces
{
    /// <summary>
    /// Service for Chrome browser integration
    /// </summary>
    public interface IChromeIntegrationService
    {
        /// <summary>
        /// Initialize the Chrome integration service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Start the Chrome integration service
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stop the Chrome integration service
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Check if Chrome is currently active
        /// </summary>
        Task<bool> IsChromeActiveAsync();

        /// <summary>
        /// Check if Chrome is running (process exists)
        /// </summary>
        bool IsChromeRunning();

        /// <summary>
        /// Get the main Chrome window handle
        /// </summary>
        IntPtr GetChromeWindowHandle();

        /// <summary>
        /// Activate the Chrome window
        /// </summary>
        Task<bool> ActivateChromeWindowAsync();

        /// <summary>
        /// Bring Chrome window to foreground
        /// </summary>
        Task<bool> BringChromeToFrontAsync();

        /// <summary>
        /// Get Chrome window title
        /// </summary>
        string GetChromeWindowTitle();

        /// <summary>
        /// Get Chrome process information
        /// </summary>
        ProcessInfo GetChromeProcessInfo();

        /// <summary>
        /// Wait for Chrome to become active
        /// </summary>
        Task<bool> WaitForChromeActiveAsync(int timeoutMs = 5000);

        /// <summary>
        /// Check if current foreground window is Chrome
        /// </summary>
        bool IsChromeForegroundWindow();

        /// <summary>
        /// Get Chrome version
        /// </summary>
        string GetChromeVersion();

        /// <summary>
        /// Check if Chrome supports required features
        /// </summary>
        bool SupportsRequiredFeatures();
    }

    /// <summary>
    /// Process information
    /// </summary>
    public class ProcessInfo
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string MainWindowTitle { get; set; } = string.Empty;
        public IntPtr MainWindowHandle { get; set; }
        public DateTime StartTime { get; set; }
        public bool Responding { get; set; }
    }
}