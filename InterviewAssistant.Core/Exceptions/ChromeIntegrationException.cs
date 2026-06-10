using System;

namespace InterviewAssistant.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when Chrome integration operations fail
    /// </summary>
    public class ChromeIntegrationException : InterviewAssistantException
    {
        public ChromeIntegrationException() : base() { }

        public ChromeIntegrationException(string message) : base(message) { }

        public ChromeIntegrationException(string message, Exception innerException) : base(message, innerException) { }
    }
}