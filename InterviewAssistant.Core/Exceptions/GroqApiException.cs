using System;

namespace InterviewAssistant.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when Groq API operations fail
    /// </summary>
    public class GroqApiException : InterviewAssistantException
    {
        public GroqApiException() : base() { }

        public GroqApiException(string message) : base(message) { }

        public GroqApiException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Gets or sets the HTTP status code if the exception is related to HTTP requests
        /// </summary>
        public System.Net.HttpStatusCode? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the API error response if available
        /// </summary>
        public InterviewAssistant.Core.Models.ApiError? ApiError { get; set; }
    }
}