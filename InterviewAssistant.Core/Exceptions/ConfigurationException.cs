using System;
using System.Collections.Generic;

namespace InterviewAssistant.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when configuration operations fail
    /// </summary>
    public class ConfigurationException : InterviewAssistantException
    {
        public ConfigurationException() : base() { }

        public ConfigurationException(string message) : base(message) { }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Gets or sets the validation errors if available
        /// </summary>
        public List<string>? ValidationErrors { get; set; }
    }
}