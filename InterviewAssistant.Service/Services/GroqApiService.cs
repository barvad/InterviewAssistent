using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using Microsoft.Extensions.Logging;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace InterviewAssistant.Service.Services;

public class GroqApiService : IGroqApiService
{
    private readonly IConfigurationManager _configurationManager;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<GroqApiService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private HttpClient? _httpClient;

    public GroqApiService(ILogger<GroqApiService> logger, IConfigurationManager configurationManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }

    public Task InitializeAsync()
    {
        var config = _configurationManager.GetConfiguration();
        var baseAddress = new Uri(config.Api.Groq.BaseUrl.TrimEnd('/') + "/");

        _httpClient = new HttpClient
        {
            BaseAddress = baseAddress,
            Timeout = TimeSpan.FromMilliseconds(config.Api.Timeout)
        };

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(config.Api.Groq.ApiKey))
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.Api.Groq.ApiKey);

        _logger.LogInformation("Groq API service initialized with base address {BaseAddress}", baseAddress);
        return Task.CompletedTask;
    }

    public Task StartAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _logger.LogInformation("Groq API service started");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _httpClient?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping Groq API service");
        }

        _logger.LogInformation("Groq API service stopped");
        return Task.CompletedTask;
    }

    public async Task<GroqApiResponse> ProcessScreenshotAsync(Bitmap screenshot, string prompt)
    {
        var config = _configurationManager.GetConfiguration();
        return await ProcessScreenshotAsync(screenshot, prompt, config.Api.Groq.Model);
    }

    public async Task<GroqApiResponse> ProcessScreenshotAsync(Bitmap screenshot, string prompt, string model)
    {
        if (_httpClient == null)
            throw new InvalidOperationException("Groq API service was not initialized.");

        // 1. Для Vision-моделей Groq (например, "llama-3.2-11b-vision-preview")
        // сжатие до 1024px идеальное — текст C# будет четким и уйдет за миллисекунды.
        using var resizedBitmap = ResizeScreenshot(screenshot, maxDimension: 1024);
        var imageBase64 = ConvertToBase64Jpg(resizedBitmap, 75L);

        // 2. СТРОИМ ОБЪЕКТ НАПРЯМУЮ. Поле content ОБЯЗАНО быть массивом объектов (матрицей контента).
        // Передаем строго актуальную мультимодальную модель.
        var requestBody = new
        {
            model, 
            max_tokens = _configurationManager.GetConfiguration().Api.Groq.MaxTokens,
            temperature = _configurationManager.GetConfiguration().Api.Groq.Temperature,
            top_p = _configurationManager.GetConfiguration().Api.Groq.TopP,
            stream = false,
            messages = new object[]
            {
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = prompt },
                    new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{imageBase64}" } }
                }
            }
            }
        };

        try
        {
            // Сериализуем анонимный тип напрямую (имена полей уже в snake_case)
            var json = JsonSerializer.Serialize(requestBody);

            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync("chat/completions", httpContent, _cancellationTokenSource?.Token ?? CancellationToken.None);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Groq API returned status {StatusCode}: {Response}", response.StatusCode, responseContent);
                return new GroqApiResponse { Error = new ApiError { Message = $"API request failed: {response.StatusCode}. Details: {responseContent}" } };
            }

            // Ответ десериализуем через ваши настройки маппинга
            return JsonSerializer.Deserialize<GroqApiResponse>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq API");
            return new GroqApiResponse { Error = new ApiError { Message = ex.Message } };
        }
    }




    public async Task<bool> TestApiConnectionAsync()
    {
        if (_httpClient == null)
            return false;

        try
        {
            using var response = await _httpClient.GetAsync("v1/models");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Groq API connectivity test failed");
            return false;
        }
    }

    public Task<ApiRateLimit> GetRateLimitAsync()
    {
        var config = _configurationManager.GetConfiguration();
        return Task.FromResult(new ApiRateLimit
        {
            RequestsPerMinute = config.Api.RateLimit.RequestsPerMinute,
            RequestsPerHour = config.Api.RateLimit.RequestsPerHour,
            RequestsPerDay = config.Api.RateLimit.RequestsPerDay,
            RemainingRequestsMinute = config.Api.RateLimit.RequestsPerMinute,
            RemainingRequestsHour = config.Api.RateLimit.RequestsPerHour,
            RemainingRequestsDay = config.Api.RateLimit.RequestsPerDay,
            ResetTime = DateTime.UtcNow.AddMinutes(1)
        });
    }

    public void CancelPendingRequests()
    {
        _cancellationTokenSource?.Cancel();
    }

    private static Bitmap ResizeScreenshot(Bitmap original, int maxDimension)
    {
        if (original.Width <= maxDimension && original.Height <= maxDimension)
            return new Bitmap(original);

        int newWidth = original.Width;
        int newHeight = original.Height;

        if (original.Width > original.Height)
        {
            newWidth = maxDimension;
            newHeight = (int)(original.Height * ((float)maxDimension / original.Width));
        }
        else
        {
            newHeight = maxDimension;
            newWidth = (int)(original.Width * ((float)maxDimension / original.Height));
        }

        var resized = new Bitmap(newWidth, newHeight);
        using (var g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(original, 0, 0, newWidth, newHeight);
        }
        return resized;
    }

    private static string ConvertToBase64Jpg(Bitmap screenshot, long quality)
    {
        using (var ms = new MemoryStream())
        {
            var jpegEncoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param = new[] { new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality) };

            screenshot.Save(ms, jpegEncoder, encoderParameters);
            return Convert.ToBase64String(ms.ToArray());
        }
    }



}