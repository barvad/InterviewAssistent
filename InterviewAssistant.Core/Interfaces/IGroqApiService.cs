using InterviewAssistant.Core.Models;
using System.Drawing;
using System.Threading.Tasks;

namespace InterviewAssistant.Core.Interfaces
{
    /// <summary>
    /// Service for interacting with Groq API
    /// </summary>
    public interface IGroqApiService
    {
        /// <summary>
        /// Initialize the Groq API service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Start the Groq API service
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stop the Groq API service
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Process a screenshot with Groq API
        /// </summary>
        Task<GroqApiResponse> ProcessScreenshotAsync(Bitmap screenshot, string prompt);

        /// <summary>
        /// Process a screenshot with Groq API using a specific model
        /// </summary>
        Task<GroqApiResponse> ProcessScreenshotAsync(Bitmap screenshot, string prompt, string model);

        /// <summary>
        /// Test API connectivity
        /// </summary>
        Task<bool> TestApiConnectionAsync();

        /// <summary>
        /// Get API rate limit information
        /// </summary>
        Task<ApiRateLimit> GetRateLimitAsync();

        /// <summary>
        /// Cancel pending API requests
        /// </summary>
        void CancelPendingRequests();
    }
}