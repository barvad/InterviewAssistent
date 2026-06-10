using InterviewAssistant.Core.Models;
using System.Threading.Tasks;

namespace InterviewAssistant.Core.Interfaces
{
    /// <summary>
    /// Service for handling global keyboard hooks
    /// </summary>
    public interface IKeyboardHookService
    {
        /// <summary>
        /// Initialize the keyboard hook service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Start the keyboard hook service
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stop the keyboard hook service
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Raised when a registered keyboard shortcut is triggered
        /// </summary>
        event Func<KeyboardShortcutEvent, Task>? ShortcutPressed;

        /// <summary>
        /// Register a keyboard shortcut
        /// </summary>
        void RegisterShortcut(KeyboardShortcut shortcut);

        /// <summary>
        /// Unregister a keyboard shortcut
        /// </summary>
        void UnregisterShortcut(string shortcutId);

        /// <summary>
        /// Get all registered shortcuts
        /// </summary>
        IEnumerable<KeyboardShortcut> GetRegisteredShortcuts();
    }
}