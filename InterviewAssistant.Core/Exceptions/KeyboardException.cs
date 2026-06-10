using System;

namespace InterviewAssistant.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when keyboard operations fail
    /// </summary>
    public class KeyboardException : InterviewAssistantException
    {
        public KeyboardException() : base() { }

        public KeyboardException(string message) : base(message) { }

        public KeyboardException(string message, Exception innerException) : base(message, innerException) { }
    }
}