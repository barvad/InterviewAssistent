using System;

namespace InterviewAssistant.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when screenshot operations fail
    /// </summary>
    public class ScreenshotException : InterviewAssistantException
    {
        public ScreenshotException() : base() { }

        public ScreenshotException(string message) : base(message) { }

        public ScreenshotException(string message, Exception innerException) : base(message, innerException) { }
    }
}