using System;

namespace InterviewAssistant.Core.Interfaces
{
    /// <summary>
    /// Service for logging application events
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Log an error
        /// </summary>
        void LogError(Exception ex, string context);

        /// <summary>
        /// Log an error asynchronously
        /// </summary>
        Task LogErrorAsync(Exception ex, string context);

        /// <summary>
        /// Log a warning
        /// </summary>
        void LogWarning(string message, string context = "");

        /// <summary>
        /// Log a warning asynchronously
        /// </summary>
        Task LogWarningAsync(string message, string context = "");

        /// <summary>
        /// Log information
        /// </summary>
        void LogInformation(string message, string context = "");

        /// <summary>
        /// Log information asynchronously
        /// </summary>
        Task LogInformationAsync(string message, string context = "");

        /// <summary>
        /// Log debug information
        /// </summary>
        void LogDebug(string message, string context = "");

        /// <summary>
        /// Log debug information asynchronously
        /// </summary>
        Task LogDebugAsync(string message, string context = "");

        /// <summary>
        /// Log API response
        /// </summary>
        void LogApiResponse(object response, string context = "");

        /// <summary>
        /// Log API response asynchronously
        /// </summary>
        Task LogApiResponseAsync(object response, string context = "");

        /// <summary>
        /// Log keyboard shortcut event
        /// </summary>
        void LogKeyboardShortcutEvent(string shortcut, string action, string context = "");

        /// <summary>
        /// Log keyboard shortcut event asynchronously
        /// </summary>
        Task LogKeyboardShortcutEventAsync(string shortcut, string action, string context = "");

        /// <summary>
        /// Log screenshot capture event
        /// </summary>
        void LogScreenshotEvent(string action, long fileSize, string context = "");

        /// <summary>
        /// Log screenshot capture event asynchronously
        /// </summary>
        Task LogScreenshotEventAsync(string action, long fileSize, string context = "");

        /// <summary>
        /// Log keyboard simulation event
        /// </summary>
        void LogKeyboardSimulationEvent(string action, string text, string context = "");

        /// <summary>
        /// Log keyboard simulation event asynchronously
        /// </summary>
        Task LogKeyboardSimulationEventAsync(string action, string text, string context = "");

        /// <summary>
        /// Log performance metrics
        /// </summary>
        void LogPerformanceMetric(string operation, long duration, string context = "");

        /// <summary>
        /// Log performance metrics asynchronously
        /// </summary>
        Task LogPerformanceMetricAsync(string operation, long duration, string context = "");
    }
}