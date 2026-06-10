using InterviewAssistant.Core.Models;
using System.Threading.Tasks;

namespace InterviewAssistant.Core.Interfaces
{
    /// <summary>
    /// Service for simulating keyboard input
    /// </summary>
    public interface IKeyboardSimulationService
    {
        /// <summary>
        /// Initialize the keyboard simulation service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Start the keyboard simulation service
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stop the keyboard simulation service
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Process API response and simulate keyboard input
        /// </summary>
        Task ProcessApiResponseAsync(GroqApiResponse apiResponse);

        /// <summary>
        /// Store a text buffer for a given shortcut id for incremental insertion
        /// </summary>
        Task StoreBufferAsync(string shortcutId, string content);

        /// <summary>
        /// Check if there is a pending buffer for the given shortcut id
        /// </summary>
        bool HasPendingBuffer(string shortcutId);

        /// <summary>
        /// Insert the next character from the pending buffer for the given shortcut id
        /// </summary>
        Task InsertNextCharacterAsync(string shortcutId);

        /// <summary>
        /// Simulate text input
        /// </summary>
        Task SimulateTextInputAsync(string text, int delayBetweenChars = 50);

        /// <summary>
        /// Simulate a key press
        /// </summary>
        Task SimulateKeyPressAsync(VirtualKeyCode keyCode, bool shift = false, bool ctrl = false, bool alt = false);

        /// <summary>
        /// Simulate a key combination
        /// </summary>
        Task SimulateKeyCombinationAsync(string combination);

        /// <summary>
        /// Simulate typing text with proper delays
        /// </summary>
        Task SimulateTypingAsync(string text, int wordsPerMinute = 120);

        /// <summary>
        /// Check if keyboard simulation is currently active
        /// </summary>
        bool IsSimulationActive();

        /// <summary>
        /// Cancel any ongoing simulation
        /// </summary>
        void CancelSimulation();
    }
}