using InterviewAssistant.Core.Models;
using System.Threading.Tasks;

namespace InterviewAssistant.Core.Interfaces
{
    /// <summary>
    /// Service for managing application configuration
    /// </summary>
    public interface IConfigurationManager
    {
        /// <summary>
        /// Load configuration from file
        /// </summary>
        Task<InterviewAssistantConfiguration> LoadConfigurationAsync();

        /// <summary>
        /// Save configuration to file
        /// </summary>
        Task SaveConfigurationAsync(InterviewAssistantConfiguration configuration);

        /// <summary>
        /// Get the current configuration
        /// </summary>
        InterviewAssistantConfiguration GetConfiguration();

        /// <summary>
        /// Get prompt for a specific shortcut
        /// </summary>
        string GetPromptForShortcut(string shortcutId);

        /// <summary>
        /// Get all available prompts
        /// </summary>
        Dictionary<string, string> GetAllPrompts();

        /// <summary>
        /// Add or update a prompt
        /// </summary>
        void AddOrUpdatePrompt(string promptId, string promptText);

        /// <summary>
        /// Remove a prompt
        /// </summary>
        bool RemovePrompt(string promptId);

        /// <summary>
        /// Get keyboard shortcuts configuration
        /// </summary>
        List<KeyboardShortcut> GetKeyboardShortcuts();

        /// <summary>
        /// Add or update a keyboard shortcut
        /// </summary>
        void AddOrUpdateShortcut(KeyboardShortcut shortcut);

        /// <summary>
        /// Remove a keyboard shortcut
        /// </summary>
        bool RemoveShortcut(string shortcutId);

        /// <summary>
        /// Validate configuration
        /// </summary>
        bool ValidateConfiguration(out List<string> validationErrors);

        /// <summary>
        /// Reset configuration to defaults
        /// </summary>
        Task ResetToDefaultsAsync();
    }
}