using InterviewAssistant.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace InterviewAssistant.Service.Services
{
    /// <summary>
    /// Logging service implementation
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly object _lockObject = new object();

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Log an error
        /// </summary>
        public void LogError(Exception ex, string context)
        {
            lock (_lockObject)
            {
                _logger.LogError(ex, "Error in {Context}: {Message}", context, ex.Message);
            }
        }

        /// <summary>
        /// Log an error asynchronously
        /// </summary>
        public Task LogErrorAsync(Exception ex, string context)
        {
            LogError(ex, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        public void LogWarning(string message, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogWarning("Warning in {Context}: {Message}", context, message);
            }
        }

        /// <summary>
        /// Log a warning asynchronously
        /// </summary>
        public Task LogWarningAsync(string message, string context = "")
        {
            LogWarning(message, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log information
        /// </summary>
        public void LogInformation(string message, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("Info in {Context}: {Message}", context, message);
            }
        }

        /// <summary>
        /// Log information asynchronously
        /// </summary>
        public Task LogInformationAsync(string message, string context = "")
        {
            LogInformation(message, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log debug information
        /// </summary>
        public void LogDebug(string message, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogDebug("Debug in {Context}: {Message}", context, message);
            }
        }

        /// <summary>
        /// Log debug information asynchronously
        /// </summary>
        public Task LogDebugAsync(string message, string context = "")
        {
            LogDebug(message, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log API response
        /// </summary>
        public void LogApiResponse(object response, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("API Response in {Context}: {Response}", context, response?.ToString() ?? "null");
            }
        }

        /// <summary>
        /// Log API response asynchronously
        /// </summary>
        public Task LogApiResponseAsync(object response, string context = "")
        {
            LogApiResponse(response, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log keyboard shortcut event
        /// </summary>
        public void LogKeyboardShortcutEvent(string shortcut, string action, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("Keyboard Shortcut Event - Shortcut: {Shortcut}, Action: {Action}, Context: {Context}", shortcut, action, context);
            }
        }

        /// <summary>
        /// Log keyboard shortcut event asynchronously
        /// </summary>
        public Task LogKeyboardShortcutEventAsync(string shortcut, string action, string context = "")
        {
            LogKeyboardShortcutEvent(shortcut, action, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log screenshot capture event
        /// </summary>
        public void LogScreenshotEvent(string action, long fileSize, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("Screenshot Event - Action: {Action}, FileSize: {FileSize} bytes, Context: {Context}", action, fileSize, context);
            }
        }

        /// <summary>
        /// Log screenshot capture event asynchronously
        /// </summary>
        public Task LogScreenshotEventAsync(string action, long fileSize, string context = "")
        {
            LogScreenshotEvent(action, fileSize, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log keyboard simulation event
        /// </summary>
        public void LogKeyboardSimulationEvent(string action, string text, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("Keyboard Simulation Event - Action: {Action}, Text: {Text}, Context: {Context}", action, text, context);
            }
        }

        /// <summary>
        /// Log keyboard simulation event asynchronously
        /// </summary>
        public Task LogKeyboardSimulationEventAsync(string action, string text, string context = "")
        {
            LogKeyboardSimulationEvent(action, text, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log performance metrics
        /// </summary>
        public void LogPerformanceMetric(string operation, long duration, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("Performance Metric - Operation: {Operation}, Duration: {Duration}ms, Context: {Context}", operation, duration, context);
            }
        }

        /// <summary>
        /// Log performance metrics asynchronously
        /// </summary>
        public Task LogPerformanceMetricAsync(string operation, long duration, string context = "")
        {
            LogPerformanceMetric(operation, duration, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log method entry
        /// </summary>
        public void LogMethodEntry(string methodName, string className, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogDebug("Method Entry - Method: {Method}, Class: {Class}, Context: {Context}", methodName, className, context);
            }
        }

        /// <summary>
        /// Log method exit
        /// </summary>
        public void LogMethodExit(string methodName, string className, long duration, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogDebug("Method Exit - Method: {Method}, Class: {Class}, Duration: {Duration}ms, Context: {Context}", methodName, className, duration, context);
            }
        }

        /// <summary>
        /// Log method exception
        /// </summary>
        public void LogMethodException(string methodName, string className, Exception ex, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogError(ex, "Method Exception - Method: {Method}, Class: {Class}, Context: {Context}", methodName, className, context);
            }
        }

        /// <summary>
        /// Log configuration change
        /// </summary>
        public void LogConfigurationChange(string setting, string oldValue, string newValue, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("Configuration Change - Setting: {Setting}, OldValue: {OldValue}, NewValue: {NewValue}, Context: {Context}", setting, oldValue, newValue, context);
            }
        }

        /// <summary>
        /// Log service state change
        /// </summary>
        public void LogServiceStateChange(string serviceName, string oldState, string newState, string context = "")
        {
            lock (_lockObject)
            {
                _logger.LogInformation("Service State Change - Service: {Service}, OldState: {OldState}, NewState: {NewState}, Context: {Context}", serviceName, oldState, newState, context);
            }
        }

        /// <summary>
        /// Log memory usage
        /// </summary>
        public void LogMemoryUsage(string context = "")
        {
            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
            
            lock (_lockObject)
            {
                _logger.LogInformation("Memory Usage - {MemoryUsage} MB, Context: {Context}", memoryUsage, context);
            }
        }

        /// <summary>
        /// Log CPU usage
        /// </summary>
        public void LogCpuUsage(string context = "")
        {
            var process = Process.GetCurrentProcess();
            var cpuUsage = process.TotalProcessorTime.TotalMilliseconds;
            
            lock (_lockObject)
            {
                _logger.LogInformation("CPU Usage - {CpuUsage} ms, Context: {Context}", cpuUsage, context);
            }
        }
    }
}