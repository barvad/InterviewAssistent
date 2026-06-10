using InterviewAssistant.Core.Models;
using System.Drawing;
using System.Threading.Tasks;

namespace InterviewAssistant.Core.Interfaces
{
    /// <summary>
    /// Service for capturing screenshots
    /// </summary>
    public interface IScreenshotService
    {
        /// <summary>
        /// Initialize the screenshot service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Start the screenshot service
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stop the screenshot service
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Capture the full screen
        /// </summary>
        Task<Bitmap> CaptureScreenshotAsync();

        /// <summary>
        /// Capture a specific screen region
        /// </summary>
        Task<Bitmap> CaptureRegionAsync(Rectangle region);

        /// <summary>
        /// Capture the active window
        /// </summary>
        Task<Bitmap> CaptureActiveWindowAsync();

        /// <summary>
        /// Get the primary screen bounds
        /// </summary>
        Rectangle GetPrimaryScreenBounds();

        /// <summary>
        /// Get all screen bounds
        /// </summary>
        IEnumerable<Rectangle> GetAllScreenBounds();
    }
}