using System.Text.Json.Serialization;

namespace InterviewAssistant.Core.Models
{
    /// <summary>
    /// Groq API request model
    /// </summary>
    public class GroqApiRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "llama-3.1-70b-versatile";

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 4096;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 1.0;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("stop")]
        public List<string>? Stop { get; set; }
    }

    /// <summary>
    /// Message model for Groq API
    /// </summary>
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public object Content { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    /// <summary>
    /// Groq API response model
    /// </summary>
    public class GroqApiResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = "chat.completion";

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new List<Choice>();

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; } = new Usage();

        [JsonPropertyName("system_fingerprint")]
        public string? SystemFingerprint { get; set; }

        [JsonPropertyName("error")]
        public ApiError? Error { get; set; }

        /// <summary>
        /// Get the main response content
        /// </summary>
        public string GetContent()
        {
            if (Choices.Count == 0)
                return string.Empty;
                
            var choice = Choices[0];
            if (choice?.Message?.Content == null)
                return string.Empty;
                
            var content = choice.Message.Content;
            
            // Handle object type (could be string or array)
            if (content is string strContent)
            {
                return strContent;
            }
            else if (content is List<object> contentArray)
            {
                // For multimodal responses, extract text content
                foreach (var item in contentArray)
                {
                    if (item is Dictionary<string, object> dict && dict.TryGetValue("text", out var text))
                    {
                        return text?.ToString() ?? string.Empty;
                    }
                }
            }
            
            return content?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Check if the response contains an error
        /// </summary>
        public bool HasError()
        {
            return Error != null;
        }

        /// <summary>
        /// Get the error message
        /// </summary>
        public string GetErrorMessage()
        {
            return Error?.Message ?? "Unknown error";
        }

        /// <summary>
        /// Get the response type
        /// </summary>
        public ResponseType GetResponseType()
        {
            var content = GetContent().ToLowerInvariant();
            
            if (content.Contains("type:"))
            {
                var typeMatch = System.Text.RegularExpressions.Regex.Match(content, @"type:\s*(\w+)");
                if (typeMatch.Success)
                {
                    return typeMatch.Groups[1].Value switch
                    {
                        "text" => ResponseType.Text,
                        "command" => ResponseType.Command,
                        "shortcut" => ResponseType.Shortcut,
                        "code" => ResponseType.Code,
                        "error" => ResponseType.Error,
                        _ => ResponseType.Text
                    };
                }
            }
            
            return ResponseType.Text;
        }

        /// <summary>
        /// Extract commands from the response
        /// </summary>
        public List<Command> ExtractCommands()
        {
            var commands = new List<Command>();
            var content = GetContent();
            
            // Look for command patterns
            var commandPattern = @"(?:type:\s*command\s*\n)?(?:action:\s*(\w+)\s*\n)?(?:text:\s*""([^""]+)""\s*\n)?(?:shortcut:\s*""([^""]+)""\s*\n)?";
            var matches = System.Text.RegularExpressions.Regex.Matches(content, commandPattern, System.Text.RegularExpressions.RegexOptions.Multiline);
            
            foreach (var match in matches)
            {
                if (match is System.Text.RegularExpressions.Match m && m.Success)
                {
                    var command = new Command
                    {
                        Action = m.Groups[1].Value.ToLowerInvariant(),
                        Text = m.Groups[2].Value,
                        Shortcut = m.Groups[3].Value
                    };
                    
                    if (!string.IsNullOrEmpty(command.Action) || !string.IsNullOrEmpty(command.Text) || !string.IsNullOrEmpty(command.Shortcut))
                    {
                        commands.Add(command);
                    }
                }
            }
            
            return commands;
        }
    }

    /// <summary>
    /// Choice model for Groq API response
    /// </summary>
    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = "stop";
    }

    /// <summary>
    /// Usage model for Groq API response
    /// </summary>
    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// API error model
    /// </summary>
    public class ApiError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("param")]
        public string? Param { get; set; }

        [JsonPropertyName("line")]
        public int? Line { get; set; }
    }

    /// <summary>
    /// Command extracted from API response
    /// </summary>
    public class Command
    {
        public string Action { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Shortcut { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response types
    /// </summary>
    public enum ResponseType
    {
        Text,
        Command,
        Shortcut,
        Code,
        Error,
        Unknown
    }

    /// <summary>
    /// API rate limit information
    /// </summary>
    public class ApiRateLimit
    {
        [JsonPropertyName("requests_per_minute")]
        public int RequestsPerMinute { get; set; }

        [JsonPropertyName("requests_per_hour")]
        public int RequestsPerHour { get; set; }

        [JsonPropertyName("requests_per_day")]
        public int RequestsPerDay { get; set; }

        [JsonPropertyName("tokens_per_minute")]
        public int TokensPerMinute { get; set; }

        [JsonPropertyName("tokens_per_hour")]
        public int TokensPerHour { get; set; }

        [JsonPropertyName("tokens_per_day")]
        public int TokensPerDay { get; set; }

        [JsonPropertyName("reset_time")]
        public DateTime ResetTime { get; set; }

        [JsonPropertyName("remaining_requests_minute")]
        public int RemainingRequestsMinute { get; set; }

        [JsonPropertyName("remaining_requests_hour")]
        public int RemainingRequestsHour { get; set; }

        [JsonPropertyName("remaining_requests_day")]
        public int RemainingRequestsDay { get; set; }

        [JsonPropertyName("remaining_tokens_minute")]
        public int RemainingTokensMinute { get; set; }

        [JsonPropertyName("remaining_tokens_hour")]
        public int RemainingTokensHour { get; set; }

        [JsonPropertyName("remaining_tokens_day")]
        public int RemainingTokensDay { get; set; }

        /// <summary>
        /// Check if rate limit exceeded
        /// </summary>
        public bool IsRateLimitExceeded()
        {
            return RemainingRequestsMinute <= 0 || RemainingRequestsHour <= 0 || RemainingRequestsDay <= 0;
        }

        /// <summary>
        /// Get rate limit status
        /// </summary>
        public string GetRateLimitStatus()
        {
            if (IsRateLimitExceeded())
            {
                return "Rate limit exceeded";
            }
            
            return $"Requests: {RemainingRequestsMinute}/{RequestsPerMinute} (min), {RemainingRequestsHour}/{RequestsPerHour} (hr), {RemainingRequestsDay}/{RequestsPerDay} (day)";
        }
    }
}