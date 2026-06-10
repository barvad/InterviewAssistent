using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using MicrosoftConfig = Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace InterviewAssistant.Configuration.Implementations
{
    /// <summary>
    /// Configuration manager implementation
    /// </summary>
    public class ConfigurationManager : Core.Interfaces.IConfigurationManager
    {
        private readonly MicrosoftConfig.IConfiguration _configuration;
        private readonly string _configFilePath;
        private InterviewAssistantConfiguration _currentConfiguration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly object _lockObject = new object();

        public ConfigurationManager(MicrosoftConfig.IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _configFilePath = GetConfigFilePath();
            _currentConfiguration = new InterviewAssistantConfiguration();
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Load configuration from file
        /// </summary>
        public async Task<InterviewAssistantConfiguration> LoadConfigurationAsync()
        {
            InterviewAssistantConfiguration configuration;

            if (File.Exists(_configFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_configFilePath);
                    configuration = JsonSerializer.Deserialize<InterviewAssistantConfiguration>(json, _jsonOptions) ?? new InterviewAssistantConfiguration();
                    
                    // Merge with appsettings.json
                    MergeWithAppSettings(configuration);

                    configuration = ValidateAndFixConfiguration(configuration);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
                    configuration = new InterviewAssistantConfiguration();
                }
            }
            else
            {
                configuration = new InterviewAssistantConfiguration();
                await SaveConfigurationAsync(configuration);
            }

            lock (_lockObject)
            {
                _currentConfiguration = configuration;
            }

            return _currentConfiguration;
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        public async Task SaveConfigurationAsync(InterviewAssistantConfiguration configuration)
        {
            try
            {
                var json = JsonSerializer.Serialize(configuration, _jsonOptions);
                Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath) ?? string.Empty);
                await File.WriteAllTextAsync(_configFilePath, json);

                lock (_lockObject)
                {
                    _currentConfiguration = configuration;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the current configuration
        /// </summary>
        public InterviewAssistantConfiguration GetConfiguration()
        {
            lock (_lockObject)
            {
                return _currentConfiguration;
            }
        }

        /// <summary>
        /// Get prompt for a specific shortcut
        /// </summary>
        public string GetPromptForShortcut(string shortcutId)
        {
            lock (_lockObject)
            {
                var shortcut = _currentConfiguration.Settings.KeyboardShortcuts.FirstOrDefault(s => s.Id == shortcutId);
                if (shortcut != null && !string.IsNullOrEmpty(shortcut.PromptId))
                {
                    return _currentConfiguration.Prompts.GetType()
                        .GetProperty(shortcut.PromptId)?
                        .GetValue(_currentConfiguration.Prompts) as string ?? string.Empty;
                }
                
                // Return default prompt if no specific prompt found
                return _currentConfiguration.Prompts.CodingInterview;
            }
        }

        /// <summary>
        /// Get all available prompts
        /// </summary>
        public Dictionary<string, string> GetAllPrompts()
        {
            lock (_lockObject)
            {
                var prompts = new Dictionary<string, string>();
                var promptsType = typeof(Prompts);
                
                foreach (var property in promptsType.GetProperties())
                {
                    if (property.PropertyType == typeof(string))
                    {
                        var value = property.GetValue(_currentConfiguration.Prompts) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            prompts[property.Name] = value;
                        }
                    }
                }
                
                return prompts;
            }
        }

        /// <summary>
        /// Add or update a prompt
        /// </summary>
        public void AddOrUpdatePrompt(string promptId, string promptText)
        {
            lock (_lockObject)
            {
                var promptsType = typeof(Prompts);
                var property = promptsType.GetProperty(promptId);
                
                if (property != null && property.PropertyType == typeof(string))
                {
                    property.SetValue(_currentConfiguration.Prompts, promptText);
                    SaveConfigurationAsync(_currentConfiguration).Wait();
                }
            }
        }

        /// <summary>
        /// Remove a prompt
        /// </summary>
        public bool RemovePrompt(string promptId)
        {
            lock (_lockObject)
            {
                var promptsType = typeof(Prompts);
                var property = promptsType.GetProperty(promptId);
                
                if (property != null && property.PropertyType == typeof(string))
                {
                    property.SetValue(_currentConfiguration.Prompts, string.Empty);
                    SaveConfigurationAsync(_currentConfiguration).Wait();
                    return true;
                }
                
                return false;
            }
        }

        /// <summary>
        /// Get keyboard shortcuts configuration
        /// </summary>
        public List<KeyboardShortcut> GetKeyboardShortcuts()
        {
            lock (_lockObject)
            {
                return _currentConfiguration.Settings.KeyboardShortcuts.ToList();
            }
        }

        /// <summary>
        /// Add or update a keyboard shortcut
        /// </summary>
        public void AddOrUpdateShortcut(KeyboardShortcut shortcut)
        {
            lock (_lockObject)
            {
                var existingIndex = _currentConfiguration.Settings.KeyboardShortcuts
                    .FindIndex(s => s.Id == shortcut.Id);
                
                if (existingIndex >= 0)
                {
                    _currentConfiguration.Settings.KeyboardShortcuts[existingIndex] = shortcut;
                }
                else
                {
                    _currentConfiguration.Settings.KeyboardShortcuts.Add(shortcut);
                }
                
                SaveConfigurationAsync(_currentConfiguration).Wait();
            }
        }

        /// <summary>
        /// Remove a keyboard shortcut
        /// </summary>
        public bool RemoveShortcut(string shortcutId)
        {
            lock (_lockObject)
            {
                var removed = _currentConfiguration.Settings.KeyboardShortcuts
                    .RemoveAll(s => s.Id == shortcutId);
                
                if (removed > 0)
                {
                    SaveConfigurationAsync(_currentConfiguration).Wait();
                    return true;
                }
                
                return false;
            }
        }

        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool ValidateConfiguration(out List<string> validationErrors)
        {
            validationErrors = new List<string>();
            
            lock (_lockObject)
            {
                // Validate API configuration
                if (string.IsNullOrWhiteSpace(_currentConfiguration.Api.Groq.ApiKey))
                {
                    validationErrors.Add("Groq API key is required");
                }
                
                if (string.IsNullOrWhiteSpace(_currentConfiguration.Api.Groq.Model))
                {
                    validationErrors.Add("Groq API model is required");
                }
                
                // Validate keyboard shortcuts
                foreach (var shortcut in _currentConfiguration.Settings.KeyboardShortcuts)
                {
                    if (string.IsNullOrWhiteSpace(shortcut.Combination))
                    {
                        validationErrors.Add($"Shortcut '{shortcut.Id}' has no combination defined");
                    }
                    
                    if (string.IsNullOrWhiteSpace(shortcut.Description))
                    {
                        validationErrors.Add($"Shortcut '{shortcut.Id}' has no description");
                    }
                }
                
                // Validate screenshot configuration
                if (_currentConfiguration.Screenshot.Quality < 1 || _currentConfiguration.Screenshot.Quality > 100)
                {
                    validationErrors.Add("Screenshot quality must be between 1 and 100");
                }
                
                // Validate logging configuration
                if (_currentConfiguration.Logging.MaxLogFileSize < 1 || _currentConfiguration.Logging.MaxLogFileSize > 100)
                {
                    validationErrors.Add("Max log file size must be between 1 and 100 MB");
                }
                
                return validationErrors.Count == 0;
            }
        }

        /// <summary>
        /// Reset configuration to defaults
        /// </summary>
        public async Task ResetToDefaultsAsync()
        {
            var configuration = new InterviewAssistantConfiguration();
            await SaveConfigurationAsync(configuration);
        }

        /// <summary>
        /// Get configuration file path
        /// </summary>
        private string GetConfigFilePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appPath = Path.Combine(appDataPath, "InterviewAssistant");
            return Path.Combine(appPath, "config.json");
        }

        /// <summary>
        /// Merge with appsettings.json
        /// </summary>
        private void MergeWithAppSettings(InterviewAssistantConfiguration config)
        {
            try
            {
                var section = _configuration.GetSection(InterviewAssistantConfiguration.SectionName);
                if (section.Exists())
                {
                    // Bind appsettings values into the existing configuration instance.
                    // The binder will only overwrite properties that are present in appsettings.
                    section.Bind(config);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error merging appsettings: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate and fix configuration
        /// </summary>
        private InterviewAssistantConfiguration ValidateAndFixConfiguration(InterviewAssistantConfiguration config)
        {
            var fixedConfig = config ?? new InterviewAssistantConfiguration();
            
            // Ensure collections are not null
            fixedConfig.Settings.KeyboardShortcuts ??= new List<KeyboardShortcut>();
            fixedConfig.Prompts ??= new Prompts();
            fixedConfig.Logging ??= new LoggingConfiguration();
            fixedConfig.Api ??= new ApiConfiguration();
            fixedConfig.Keyboard ??= new KeyboardConfiguration();
            fixedConfig.Screenshot ??= new ScreenshotConfiguration();
            fixedConfig.Chrome ??= new ChromeConfiguration();
            
            // Ensure API configuration is not null
            fixedConfig.Api.Groq ??= new GroqApiConfiguration();
            
            return fixedConfig;
        }
    }
}