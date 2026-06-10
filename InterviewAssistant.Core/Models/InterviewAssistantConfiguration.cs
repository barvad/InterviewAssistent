using System.Text.Json.Serialization;

namespace InterviewAssistant.Core.Models
{
    /// <summary>
    /// Main configuration class for Interview Assistant
    /// </summary>
    public class InterviewAssistantConfiguration
    {
        public const string SectionName = "InterviewAssistant";

        [JsonPropertyName("settings")]
        public Settings Settings { get; set; } = new Settings();

        [JsonPropertyName("prompts")]
        public Prompts Prompts { get; set; } = new Prompts();

        [JsonPropertyName("logging")]
        public LoggingConfiguration Logging { get; set; } = new LoggingConfiguration();

        [JsonPropertyName("api")]
        public ApiConfiguration Api { get; set; } = new ApiConfiguration();

        [JsonPropertyName("keyboard")]
        public KeyboardConfiguration Keyboard { get; set; } = new KeyboardConfiguration();

        [JsonPropertyName("screenshot")]
        public ScreenshotConfiguration Screenshot { get; set; } = new ScreenshotConfiguration();

        [JsonPropertyName("chrome")]
        public ChromeConfiguration Chrome { get; set; } = new ChromeConfiguration();
    }

    /// <summary>
    /// Application settings
    /// </summary>
    public class Settings
    {
        [JsonPropertyName("autoStart")]
        public bool AutoStart { get; set; } = true;

        [JsonPropertyName("startMinimized")]
        public bool StartMinimized { get; set; } = false;

        [JsonPropertyName("enableTrayIcon")]
        public bool EnableTrayIcon { get; set; } = true;

        [JsonPropertyName("checkForUpdates")]
        public bool CheckForUpdates { get; set; } = true;

        [JsonPropertyName("enableNotifications")]
        public bool EnableNotifications { get; set; } = true;

        [JsonPropertyName("logLevel")]
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        [JsonPropertyName("maxLogFiles")]
        public int MaxLogFiles { get; set; } = 7;

        [JsonPropertyName("maxLogFileSize")]
        public int MaxLogFileSize { get; set; } = 10; // MB

        [JsonPropertyName("keyboardShortcuts")]
        public List<KeyboardShortcut> KeyboardShortcuts { get; set; } = new List<KeyboardShortcut>();
    }

    /// <summary>
    /// Prompt configurations
    /// </summary>
    public class Prompts
    {
        [JsonPropertyName("codingInterview")]
        public string CodingInterview { get; set; } = "Analyze this code and provide suggestions for improvement, best practices, and potential bugs. Focus on code quality, performance, and maintainability.";

        [JsonPropertyName("generalInterview")]
        public string GeneralInterview { get; set; } = "Summarize the content and provide key insights, main points, and important takeaways.";

        [JsonPropertyName("technicalAssessment")]
        public string TechnicalAssessment { get; set; } = "Evaluate the technical aspects of this content and provide feedback on architecture, design patterns, and implementation quality.";

        [JsonPropertyName("codeReview")]
        public string CodeReview { get; set; } = "Perform a thorough code review focusing on readability, maintainability, security, and performance. Provide specific recommendations for improvement.";

        [JsonPropertyName("documentation")]
        public string Documentation { get; set; } = "Generate comprehensive documentation for this code including API references, usage examples, and implementation details.";

        [JsonPropertyName("debugging")]
        public string Debugging { get; set; } = "Help debug this code by identifying potential issues, suggesting fixes, and explaining the root causes of problems.";
    }

    /// <summary>
    /// Logging configuration
    /// </summary>
    public class LoggingConfiguration
    {
        [JsonPropertyName("enableConsole")]
        public bool EnableConsole { get; set; } = true;

        [JsonPropertyName("enableFile")]
        public bool EnableFile { get; set; } = true;

        [JsonPropertyName("logFilePath")]
        public string LogFilePath { get; set; } = "logs/interview-assistant-.txt";

        [JsonPropertyName("rollingInterval")]
        public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

        [JsonPropertyName("retainedFileCountLimit")]
        public int RetainedFileCountLimit { get; set; } = 7;

        [JsonPropertyName("maxLogFileSize")]
        public int MaxLogFileSize { get; set; } = 10; // MB

        [JsonPropertyName("includeScopes")]
        public bool IncludeScopes { get; set; } = true;

        [JsonPropertyName("outputTemplate")]
        public string OutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    }

    /// <summary>
    /// API configuration
    /// </summary>
    public class ApiConfiguration
    {
        [JsonPropertyName("groq")]
        public GroqApiConfiguration Groq { get; set; } = new GroqApiConfiguration();

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; } = 30000; // 30 seconds

        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; } = 3;

        [JsonPropertyName("retryDelay")]
        public int RetryDelay { get; set; } = 1000; // 1 second

        [JsonPropertyName("rateLimit")]
        public RateLimitConfiguration RateLimit { get; set; } = new RateLimitConfiguration();
    }

    /// <summary>
    /// Groq API configuration
    /// </summary>
    public class GroqApiConfiguration
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; } = "https://api.groq.com/v1";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "llama-3.1-70b-versatile";

        [JsonPropertyName("maxTokens")]
        public int MaxTokens { get; set; } = 4096;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("topP")]
        public double TopP { get; set; } = 1.0;

        [JsonPropertyName("topK")]
        public int TopK { get; set; } = 40;
    }

    /// <summary>
    /// Rate limit configuration
    /// </summary>
    public class RateLimitConfiguration
    {
        [JsonPropertyName("requestsPerMinute")]
        public int RequestsPerMinute { get; set; } = 60;

        [JsonPropertyName("requestsPerHour")]
        public int RequestsPerHour { get; set; } = 1000;

        [JsonPropertyName("requestsPerDay")]
        public int RequestsPerDay { get; set; } = 10000;

        [JsonPropertyName("enableThrottling")]
        public bool EnableThrottling { get; set; } = true;

        [JsonPropertyName("throttleDelay")]
        public int ThrottleDelay { get; set; } = 100; // milliseconds
    }

    /// <summary>
    /// Keyboard configuration
    /// </summary>
    public class KeyboardConfiguration
    {
        [JsonPropertyName("hookType")]
        public HookType HookType { get; set; } = HookType.LowLevel;

        [JsonPropertyName("enableGlobalHook")]
        public bool EnableGlobalHook { get; set; } = true;

        [JsonPropertyName("hookPriority")]
        public HookPriority HookPriority { get; set; } = HookPriority.Normal;

        [JsonPropertyName("enableEventFiltering")]
        public bool EnableEventFiltering { get; set; } = true;

        [JsonPropertyName("eventQueueSize")]
        public int EventQueueSize { get; set; } = 1000;

        [JsonPropertyName("enableAsyncProcessing")]
        public bool EnableAsyncProcessing { get; set; } = true;

        [JsonPropertyName("processingDelay")]
        public int ProcessingDelay { get; set; } = 10; // milliseconds
    }

    /// <summary>
    /// Screenshot configuration
    /// </summary>
    public class ScreenshotConfiguration
    {
        [JsonPropertyName("format")]
        public ImageFormat Format { get; set; } = ImageFormat.Png;

        [JsonPropertyName("quality")]
        public int Quality { get; set; } = 90;

        [JsonPropertyName("region")]
        public CaptureRegion Region { get; set; } = CaptureRegion.FullScreen;

        [JsonPropertyName("enableRegionSelection")]
        public bool EnableRegionSelection { get; set; } = false;

        [JsonPropertyName("captureCursor")]
        public bool CaptureCursor { get; set; } = true;

        [JsonPropertyName("includeTaskbar")]
        public bool IncludeTaskbar { get; set; } = true;

        [JsonPropertyName("maxFileSize")]
        public int MaxFileSize { get; set; } = 5; // MB

        [JsonPropertyName("enableCompression")]
        public bool EnableCompression { get; set; } = true;

        [JsonPropertyName("compressionLevel")]
        public int CompressionLevel { get; set; } = 6; // 1-9
    }

    /// <summary>
    /// Chrome configuration
    /// </summary>
    public class ChromeConfiguration
    {
        [JsonPropertyName("enableIntegration")]
        public bool EnableIntegration { get; set; } = true;

        [JsonPropertyName("windowTitlePattern")]
        public string WindowTitlePattern { get; set; } = "Google Chrome";

        [JsonPropertyName("processName")]
        public string ProcessName { get; set; } = "chrome";

        [JsonPropertyName("activationDelay")]
        public int ActivationDelay { get; set; } = 500; // milliseconds

        [JsonPropertyName("maxActivationAttempts")]
        public int MaxActivationAttempts { get; set; } = 3;

        [JsonPropertyName("enableVersionCheck")]
        public bool EnableVersionCheck { get; set; } = true;

        [JsonPropertyName("minVersion")]
        public string MinVersion { get; set; } = "120.0.0.0";

        [JsonPropertyName("enableFocusManagement")]
        public bool EnableFocusManagement { get; set; } = true;

        [JsonPropertyName("focusTimeout")]
        public int FocusTimeout { get; set; } = 5000; // milliseconds
    }

    /// <summary>
    /// Log levels
    /// </summary>
    public enum LogLevel
    {
        Verbose,
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// Rolling intervals
    /// </summary>
    public enum RollingInterval
    {
        Day,
        Hour,
        Minute,
        Month,
        Year,
        Infinite
    }

    /// <summary>
    /// Hook types
    /// </summary>
    public enum HookType
    {
        LowLevel,
        Journal,
        Legacy
    }

    /// <summary>
    /// Hook priorities
    /// </summary>
    public enum HookPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// Image formats
    /// </summary>
    public enum ImageFormat
    {
        Png,
        Jpeg,
        Bmp,
        Tiff
    }

    /// <summary>
    /// Capture regions
    /// </summary>
    public enum CaptureRegion
    {
        FullScreen,
        ActiveWindow,
        PrimaryScreen,
        Custom
    }
}