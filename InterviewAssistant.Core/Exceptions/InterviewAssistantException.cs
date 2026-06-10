using System;

namespace InterviewAssistant.Core.Exceptions
{
    /// <summary>
    /// Base exception for Interview Assistant application
    /// </summary>
    public class InterviewAssistantException : Exception
    {
        public InterviewAssistantException() : base() { }

        public InterviewAssistantException(string message) : base(message) { }

        public InterviewAssistantException(string message, Exception innerException) : base(message, innerException) { }
    }
}