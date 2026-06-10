using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InterviewAssistant.Service.Services
{
    public class ScreenshotService : IScreenshotService
    {
        private readonly ILogger<ScreenshotService> _logger;
        private bool _isInitialized;

        public ScreenshotService(ILogger<ScreenshotService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task InitializeAsync()
        {
            _isInitialized = true;
            _logger.LogInformation("Screenshot service initialized");
            return Task.CompletedTask;
        }

        public Task StartAsync()
        {
            _logger.LogInformation("Screenshot service started");
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _logger.LogInformation("Screenshot service stopped");
            return Task.CompletedTask;
        }

        public Task<Bitmap> CaptureScreenshotAsync()
        {
            var bounds = GetAllScreenBounds().Aggregate(Rectangle.Empty, Rectangle.Union);
            if (bounds.IsEmpty)
            {
                bounds = GetPrimaryScreenBounds();
            }

            var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            _logger.LogInformation("Captured full screen screenshot: {Width}x{Height}", bounds.Width, bounds.Height);
            return Task.FromResult(bitmap);
        }

        public Task<Bitmap> CaptureRegionAsync(Rectangle region)
        {
            var bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(region.Location, Point.Empty, region.Size, CopyPixelOperation.SourceCopy);
            _logger.LogInformation("Captured screenshot region: {Region}", region);
            return Task.FromResult(bitmap);
        }

        public Task<Bitmap> CaptureActiveWindowAsync()
        {
            var handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
            {
                return CaptureScreenshotAsync();
            }

            if (!GetWindowRect(handle, out var rect))
            {
                return CaptureScreenshotAsync();
            }

            var region = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
            return CaptureRegionAsync(region);
        }

        public Rectangle GetPrimaryScreenBounds()
        {
            return Screen.PrimaryScreen?.Bounds ?? Rectangle.Empty;
        }

        public IEnumerable<Rectangle> GetAllScreenBounds()
        {
            return Screen.AllScreens.Select(s => s.Bounds).ToList();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
