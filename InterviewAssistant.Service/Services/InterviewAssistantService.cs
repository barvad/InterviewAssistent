using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace InterviewAssistant.Service.Services
{
    /// <summary>
    /// Main Windows Service class that orchestrates all services
    /// </summary>
    public class InterviewAssistantService : IDisposable
    {
        private readonly ILogger<InterviewAssistantService> _logger;
        private readonly IKeyboardHookService _keyboardHookService;
        private readonly IScreenshotService _screenshotService;
        private readonly IGroqApiService _groqApiService;
        private readonly IKeyboardSimulationService _keyboardSimulationService;
        private readonly IConfigurationManager _configurationManager;
        private readonly IChromeIntegrationService _chromeIntegrationService;
        private readonly ILoggingService _loggingService;
        
        private readonly object _lockObject = new object();
        private bool _isRunning = false;
        private bool _disposed = false;
        private CancellationTokenSource? _cancellationTokenSource;

        public InterviewAssistantService(
            ILogger<InterviewAssistantService> logger,
            IKeyboardHookService keyboardHookService,
            IScreenshotService screenshotService,
            IGroqApiService groqApiService,
            IKeyboardSimulationService keyboardSimulationService,
            IConfigurationManager configurationManager,
            IChromeIntegrationService chromeIntegrationService,
            ILoggingService loggingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyboardHookService = keyboardHookService ?? throw new ArgumentNullException(nameof(keyboardHookService));
            _screenshotService = screenshotService ?? throw new ArgumentNullException(nameof(screenshotService));
            _groqApiService = groqApiService ?? throw new ArgumentNullException(nameof(groqApiService));
            _keyboardSimulationService = keyboardSimulationService ?? throw new ArgumentNullException(nameof(keyboardSimulationService));
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _chromeIntegrationService = chromeIntegrationService ?? throw new ArgumentNullException(nameof(chromeIntegrationService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Start the service in interactive mode (for development/testing)
        /// </summary>
        public async Task RunInteractiveAsync()
        {
            try
            {
                _logger.LogInformation("Starting Interview Assistant in interactive mode");
                
                // Load configuration
                await LoadConfigurationAsync();
                
                // Initialize services
                await InitializeServicesAsync();
                
                // Start services
                await StartServicesAsync();
                
                _logger.LogInformation("Interview Assistant started successfully");
                
                // Keep the service running
                await Task.Delay(Timeout.Infinite, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Interview Assistant in interactive mode");
                throw;
            }
        }

        /// <summary>
        /// Start the service
        /// </summary>
        public async Task StartAsync()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;
                
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                _logger.LogInformation("Starting Interview Assistant service");
                
                // Load configuration
                await LoadConfigurationAsync();
                
                // Initialize services
                await InitializeServicesAsync();
                
                // Start services
                await StartServicesAsync();
                
                _logger.LogInformation("Interview Assistant service started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Interview Assistant service");
                await StopAsync();
                throw;
            }
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        public async Task StopAsync()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;
                
                _isRunning = false;
                _cancellationTokenSource?.Cancel();
            }

            try
            {
                _logger.LogInformation("Stopping Interview Assistant service");
                
                // Stop services in reverse order
                await StopServicesAsync();
                
                _logger.LogInformation("Interview Assistant service stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Interview Assistant service");
                throw;
            }
        }

        /// <summary>
        /// Load configuration
        /// </summary>
        private async Task LoadConfigurationAsync()
        {
            try
            {
                _logger.LogInformation("Loading configuration");
                
                var config = await _configurationManager.LoadConfigurationAsync();
                
                // Validate configuration
                if (config == null)
                {
                    throw new InvalidOperationException("Configuration is null");
                }
                
                if (string.IsNullOrWhiteSpace(config.Api?.Groq?.ApiKey))
                {
                    throw new InvalidOperationException("Groq API key is not configured");
                }
                
                _logger.LogInformation("Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration");
                throw;
            }
        }

        /// <summary>
        /// Initialize all services
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            try
            {
                _logger.LogInformation("Initializing services");
                
                // Initialize keyboard hook service
                await _keyboardHookService.InitializeAsync();
                _keyboardHookService.ShortcutPressed += HandleKeyboardShortcutAsync;
                
                // Register configured shortcuts
                var configuredShortcuts = _configurationManager.GetKeyboardShortcuts()
                    .Where(s => s.Enabled)
                    .ToList();

                foreach (var shortcut in configuredShortcuts)
                {
                    _keyboardHookService.RegisterShortcut(shortcut);
                }
                
                // Initialize screenshot service
                await _screenshotService.InitializeAsync();
                
                // Initialize Groq API service
                await _groqApiService.InitializeAsync();
                
                // Initialize keyboard simulation service
                await _keyboardSimulationService.InitializeAsync();
                
                // Initialize Chrome integration service
                await _chromeIntegrationService.InitializeAsync();
                
                _logger.LogInformation("All services initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing services");
                throw;
            }
        }

        /// <summary>
        /// Start all services
        /// </summary>
        private async Task StartServicesAsync()
        {
            try
            {
                _logger.LogInformation("Starting services");
                
                // Start keyboard hook service
                await _keyboardHookService.StartAsync();
                
                // Start screenshot service
                await _screenshotService.StartAsync();
                
                // Start Groq API service
                await _groqApiService.StartAsync();
                
                // Start keyboard simulation service
                await _keyboardSimulationService.StartAsync();
                
                // Start Chrome integration service
                await _chromeIntegrationService.StartAsync();
                
                _logger.LogInformation("All services started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting services");
                throw;
            }
        }

        /// <summary>
        /// Stop all services
        /// </summary>
        private async Task StopServicesAsync()
        {
            try
            {
                _logger.LogInformation("Stopping services");
                
                // Stop services in reverse order
                if (_keyboardSimulationService != null)
                    await _keyboardSimulationService.StopAsync();
                
                if (_groqApiService != null)
                    await _groqApiService.StopAsync();
                
                if (_screenshotService != null)
                    await _screenshotService.StopAsync();
                
                if (_keyboardHookService != null)
                    await _keyboardHookService.StopAsync();
                
                if (_chromeIntegrationService != null)
                    await _chromeIntegrationService.StopAsync();
                
                _logger.LogInformation("All services stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping services");
                throw;
            }
        }

        /// <summary>
        /// Handle keyboard shortcut event
        /// </summary>
        public async Task HandleKeyboardShortcutAsync(KeyboardShortcutEvent shortcutEvent)
        {
        

            try
            {
                _logger.LogInformation("Handling keyboard shortcut: {Shortcut}", shortcutEvent.Shortcut);

                // Check if Chrome is active
                if (!await _chromeIntegrationService.IsChromeActiveAsync())
                {
                    _logger.LogWarning("Chrome is not active, ignoring shortcut");
                    return;
                }

                // Determine shortcut id and handle pending buffer insertion first
                var shortcutId = shortcutEvent.ShortcutDefinition?.Id ?? shortcutEvent.Shortcut;
                if (_keyboardSimulationService.HasPendingBuffer(shortcutId))
                {
                    await _keyboardSimulationService.InsertNextCharacterAsync(shortcutId);
                    _logger.LogInformation("Inserted next character for shortcut {ShortcutId}", shortcutId);
                    return;
                }

                // Take screenshot
                var screenshot = await _screenshotService.CaptureScreenshotAsync();
                if (screenshot == null)
                {
                    _logger.LogError("Failed to capture screenshot");
                    return;
                }

                // Get prompt based on shortcut (may be empty for insert-only shortcuts)
                var prompt = _configurationManager.GetPromptForShortcut(shortcutEvent.Shortcut);

                // Send to Groq API
                var apiResponse = await _groqApiService.ProcessScreenshotAsync(screenshot, prompt);
                if (apiResponse == null)
                {
                    _logger.LogError("Failed to process screenshot with Groq API");
                    return;
                }

                // If API returned commands, let existing processor handle them
                var commands = apiResponse.ExtractCommands();
                if (commands.Any())
                {
                    await _keyboardSimulationService.ProcessApiResponseAsync(apiResponse);
                }
                else
                {
                    var content = apiResponse.GetContent();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        // Store buffer for incremental insertion and insert first character
                        await _keyboardSimulationService.StoreBufferAsync(shortcutId, content.Trim());
                        await _keyboardSimulationService.InsertNextCharacterAsync(shortcutId);
                    }
                }

                _logger.LogInformation("Keyboard shortcut processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling keyboard shortcut: {Shortcut}", shortcutEvent.Shortcut);
                await _loggingService.LogErrorAsync(ex, $"KeyboardShortcutProcessing: Error processing shortcut {shortcutEvent.Shortcut}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                }
                
                _disposed = true;
            }
        }

        ~InterviewAssistantService()
        {
            Dispose(false);
        }
    }
}