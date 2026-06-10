using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace InterviewAssistant.Service.Services
{
    /// <summary>
    /// Chrome integration service implementation
    /// </summary>
    public class ChromeIntegrationService : IChromeIntegrationService
    {
        private readonly ILogger<ChromeIntegrationService> _logger;
        private readonly object _lockObject = new object();
        private bool _isInitialized = false;
        private ProcessInfo? _chromeProcessInfo;
        private IntPtr _chromeWindowHandle = IntPtr.Zero;
        private readonly IConfigurationManager _configurationManager;

        public ChromeIntegrationService(ILogger<ChromeIntegrationService> logger, IConfigurationManager configurationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        }

        /// <summary>
        /// Initialize the Chrome integration service
        /// </summary>
        public async Task InitializeAsync()
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                    return;
                
                _isInitialized = true;
            }

            try
            {
                _logger.LogInformation("Initializing Chrome integration service");
                
                // Load configuration
                var config = _configurationManager.GetConfiguration();
                
                if (!config.Chrome.EnableIntegration)
                {
                    _logger.LogWarning("Chrome integration is disabled in configuration");
                    return;
                }
                
                _logger.LogInformation("Chrome integration service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Chrome integration service");
                throw;
            }
        }

        /// <summary>
        /// Start the Chrome integration service
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                _logger.LogInformation("Starting Chrome integration service");
                
                // Start monitoring Chrome processes
                await MonitorChromeProcessesAsync();
                
                _logger.LogInformation("Chrome integration service started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Chrome integration service");
                throw;
            }
        }

        /// <summary>
        /// Stop the Chrome integration service
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                _logger.LogInformation("Stopping Chrome integration service");
                
                // Clear Chrome process info
                lock (_lockObject)
                {
                    _chromeProcessInfo = null;
                    _chromeWindowHandle = IntPtr.Zero;
                }
                
                _logger.LogInformation("Chrome integration service stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Chrome integration service");
                throw;
            }
        }

        /// <summary>
        /// Check if Chrome is currently active
        /// </summary>
        public async Task<bool> IsChromeActiveAsync()
        {
            try
            {
                var isRunning = IsChromeRunning();
                var isActive = await IsChromeWindowActiveAsync();
                
                return isRunning && isActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Chrome is active");
                return false;
            }
        }

        /// <summary>
        /// Check if Chrome is running (process exists)
        /// </summary>
        public bool IsChromeRunning()
        {
            try
            {
                var config = _configurationManager.GetConfiguration();
                var chromeProcesses = Process.GetProcessesByName(config.Chrome.ProcessName);
                
                if (chromeProcesses.Length == 0)
                {
                    lock (_lockObject)
                    {
                        _chromeProcessInfo = null;
                        _chromeWindowHandle = IntPtr.Zero;
                    }
                    return false;
                }
                
                // Get the main Chrome process
                var mainProcess = chromeProcesses.FirstOrDefault(p => !string.IsNullOrEmpty(p.MainWindowTitle)) ?? chromeProcesses[0];
                
                lock (_lockObject)
                {
                    _chromeProcessInfo = new ProcessInfo
                    {
                        ProcessId = mainProcess.Id,
                        ProcessName = mainProcess.ProcessName,
                        MainWindowTitle = mainProcess.MainWindowTitle ?? string.Empty,
                        MainWindowHandle = mainProcess.MainWindowHandle,
                        StartTime = mainProcess.StartTime,
                        Responding = mainProcess.Responding
                    };
                    _chromeWindowHandle = mainProcess.MainWindowHandle;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Chrome is running");
                return false;
            }
        }

        /// <summary>
        /// Get the main Chrome window handle
        /// </summary>
        public IntPtr GetChromeWindowHandle()
        {
            lock (_lockObject)
            {
                return _chromeWindowHandle;
            }
        }

        /// <summary>
        /// Activate the Chrome window
        /// </summary>
        public async Task<bool> ActivateChromeWindowAsync()
        {
            try
            {
                if (!IsChromeRunning())
                {
                    _logger.LogWarning("Chrome is not running");
                    return false;
                }
                
                var config = _configurationManager.GetConfiguration();
                var attempts = 0;
                var success = false;
                
                while (attempts < config.Chrome.MaxActivationAttempts && !success)
                {
                    attempts++;
                    
                    try
                    {
                        var handle = GetChromeWindowHandle();
                        if (handle != IntPtr.Zero)
                        {
                            // Bring window to foreground
                            var foregroundWindow = GetForegroundWindow();
                            if (foregroundWindow != handle)
                            {
                                // Set window to foreground
                                SetForegroundWindow(handle);
                                Thread.Sleep(config.Chrome.ActivationDelay);
                                
                                // Verify window is active
                                var newForegroundWindow = GetForegroundWindow();
                                if (newForegroundWindow == handle)
                                {
                                    success = true;
                                    _logger.LogInformation("Chrome window activated successfully");
                                }
                            }
                            else
                            {
                                success = true;
                                _logger.LogInformation("Chrome window is already active");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Attempt {Attempt} failed to activate Chrome window", attempts);
                    }
                    
                    if (!success && attempts < config.Chrome.MaxActivationAttempts)
                    {
                        await Task.Delay(config.Chrome.ActivationDelay);
                    }
                }
                
                if (!success)
                {
                    _logger.LogWarning("Failed to activate Chrome window after {Attempts} attempts", attempts);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating Chrome window");
                return false;
            }
        }

        /// <summary>
        /// Bring Chrome window to foreground
        /// </summary>
        public async Task<bool> BringChromeToFrontAsync()
        {
            return await ActivateChromeWindowAsync();
        }

        /// <summary>
        /// Get Chrome window title
        /// </summary>
        public string GetChromeWindowTitle()
        {
            lock (_lockObject)
            {
                return _chromeProcessInfo?.MainWindowTitle ?? string.Empty;
            }
        }

        /// <summary>
        /// Get Chrome process information
        /// </summary>
        public ProcessInfo GetChromeProcessInfo()
        {
            lock (_lockObject)
            {
                return _chromeProcessInfo ?? new ProcessInfo();
            }
        }

        /// <summary>
        /// Wait for Chrome to become active
        /// </summary>
        public async Task<bool> WaitForChromeActiveAsync(int timeoutMs = 5000)
        {
            var startTime = DateTime.UtcNow;
            var config = _configurationManager.GetConfiguration();
            
            while (DateTime.UtcNow - startTime < TimeSpan.FromMilliseconds(timeoutMs))
            {
                if (await IsChromeActiveAsync())
                {
                    return true;
                }
                
                await Task.Delay(config.Chrome.ActivationDelay);
            }
            
            return false;
        }

        /// <summary>
        /// Check if current foreground window is Chrome
        /// </summary>
        public bool IsChromeForegroundWindow()
        {
            try
            {
                var foregroundWindow = GetForegroundWindow();
                var config = _configurationManager.GetConfiguration();
                
                if (foregroundWindow == IntPtr.Zero)
                    return false;
                
                // Get window title
                var title = GetWindowTitle(foregroundWindow);
                if (string.IsNullOrEmpty(title))
                    return false;
                
                // Check if title matches Chrome pattern
                var pattern = config.Chrome.WindowTitlePattern;
                return title.Contains(pattern, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Chrome is foreground window");
                return false;
            }
        }

        /// <summary>
        /// Get Chrome version
        /// </summary>
        public string GetChromeVersion()
        {
            try
            {
                var chromePath = GetChromeInstallationPath();
                if (string.IsNullOrEmpty(chromePath))
                    return string.Empty;
                
                var versionFile = Path.Combine(chromePath, "chrome.dll");
                if (!File.Exists(versionFile))
                    return string.Empty;
                
                var versionInfo = FileVersionInfo.GetVersionInfo(versionFile);
                return versionInfo.FileVersion ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Chrome version");
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if Chrome supports required features
        /// </summary>
        public bool SupportsRequiredFeatures()
        {
            try
            {
                var config = _configurationManager.GetConfiguration();
                if (!config.Chrome.EnableVersionCheck)
                    return true;
                
                var version = GetChromeVersion();
                if (string.IsNullOrEmpty(version))
                    return false;
                
                // Parse version
                var versionParts = version.Split('.');
                if (versionParts.Length < 4)
                    return false;
                
                if (!int.TryParse(versionParts[0], out var majorVersion))
                    return false;
                
                if (!int.TryParse(versionParts[1], out var minorVersion))
                    return false;
                
                var requiredVersion = config.Chrome.MinVersion.Split('.');
                if (requiredVersion.Length < 2)
                    return false;
                
                if (!int.TryParse(requiredVersion[0], out var requiredMajor))
                    return false;
                
                if (!int.TryParse(requiredVersion[1], out var requiredMinor))
                    return false;
                
                // Check if version meets minimum requirements
                if (majorVersion > requiredMajor)
                    return true;
                
                if (majorVersion == requiredMajor && minorVersion >= requiredMinor)
                    return true;
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Chrome version requirements");
                return false;
            }
        }

        /// <summary>
        /// Monitor Chrome processes
        /// </summary>
        private async Task MonitorChromeProcessesAsync()
        {
            try
            {
                // Start background monitoring
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            IsChromeRunning();
                            await Task.Delay(1000); // Check every second
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error monitoring Chrome processes");
                            await Task.Delay(5000); // Wait longer on error
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Chrome process monitoring");
                throw;
            }
        }

        /// <summary>
        /// Check if Chrome window is active
        /// </summary>
        private async Task<bool> IsChromeWindowActiveAsync()
        {
            try
            {
                var handle = GetChromeWindowHandle();
                if (handle == IntPtr.Zero)
                    return false;
                
                var foregroundWindow = GetForegroundWindow();
                return foregroundWindow == handle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Chrome window is active");
                return false;
            }
        }

        /// <summary>
        /// Get Chrome installation path
        /// </summary>
        private string GetChromeInstallationPath()
        {
            try
            {
                // Check common installation paths
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                var chromePath = Path.Combine(programFiles, "Google", "Chrome", "Application");
                
                if (Directory.Exists(chromePath))
                    return chromePath;
                
                // Check 64-bit path
                programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                chromePath = Path.Combine(programFiles, "Google", "Chrome", "Application");
                
                if (Directory.Exists(chromePath))
                    return chromePath;
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Chrome installation path");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get window title
        /// </summary>
        private string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            var buffer = new StringBuilder(nChars);
            
            if (GetWindowText(handle, buffer, nChars) > 0)
            {
                return buffer.ToString();
            }
            
            return string.Empty;
        }

        // P/Invoke declarations
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }
}