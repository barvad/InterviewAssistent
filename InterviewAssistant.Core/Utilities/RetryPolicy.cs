using System;
using System.Threading;
using System.Threading.Tasks;

namespace InterviewAssistant.Core.Utilities
{
    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public class RetryPolicy
    {
        public int MaxAttempts { get; set; } = 3;
        public int InitialDelayMs { get; set; } = 1000;
        public int MaxDelayMs { get; set; } = 30000;
        public double BackoffMultiplier { get; set; } = 2.0;
        public Type[] RetryableExceptionTypes { get; set; } = Array.Empty<Type>();

        public static RetryPolicy Default => new RetryPolicy();
        public static RetryPolicy Aggressive => new RetryPolicy { MaxAttempts = 5, InitialDelayMs = 500 };
        public static RetryPolicy Conservative => new RetryPolicy { MaxAttempts = 2, InitialDelayMs = 2000 };
    }

    /// <summary>
    /// Retry policy execution utilities
    /// </summary>
    public static class RetryExecutor
    {
        /// <summary>
        /// Execute an operation with retry policy
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation,
            RetryPolicy? policy = null,
            CancellationToken cancellationToken = default)
        {
            policy ??= RetryPolicy.Default;
            
            var lastException = default(Exception);
            
            for (var attempt = 1; attempt <= policy.MaxAttempts; attempt++)
            {
                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (Exception ex) when (IsRetryableException(ex, policy.RetryableExceptionTypes))
                {
                    lastException = ex;
                    
                    if (attempt == policy.MaxAttempts)
                        break;
                    
                    var delay = CalculateDelay(attempt, policy);
                    
                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                }
            }
            
            throw new InterviewAssistant.Core.Exceptions.InterviewAssistantException(
                $"Operation failed after {policy.MaxAttempts} attempts", lastException);
        }

        /// <summary>
        /// Execute an operation with retry policy (non-generic)
        /// </summary>
        public static async Task ExecuteAsync(
            Func<Task> operation,
            RetryPolicy? policy = null,
            CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(async () =>
            {
                await operation().ConfigureAwait(false);
                return true;
            }, policy, cancellationToken).ConfigureAwait(false);
        }

        private static bool IsRetryableException(Exception ex, Type[] retryableExceptionTypes)
        {
            if (retryableExceptionTypes.Length == 0)
                return true; // Retry all exceptions by default
            
            return retryableExceptionTypes.Any(t => t.IsInstanceOfType(ex));
        }

        private static int CalculateDelay(int attempt, RetryPolicy policy)
        {
            var delay = policy.InitialDelayMs * Math.Pow(policy.BackoffMultiplier, attempt - 1);
            return (int)Math.Min(delay, policy.MaxDelayMs);
        }
    }
}