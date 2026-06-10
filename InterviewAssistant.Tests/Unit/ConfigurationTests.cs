using InterviewAssistant.Core.Models;
using ConfigurationManagerImpl = InterviewAssistant.Configuration.Implementations.ConfigurationManager;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Xunit;

namespace InterviewAssistant.Tests.Unit
{
    /// <summary>
    /// Unit tests for configuration management
    /// </summary>
    public class ConfigurationTests
    {
        private readonly IConfiguration _configuration;
        private readonly ConfigurationManagerImpl _configManager;

        public ConfigurationTests()
        {
            // Create in-memory configuration for testing
            var inConfig = new Dictionary<string, string?>
            {
                ["InterviewAssistant:settings:autoStart"] = "true",
                ["InterviewAssistant:settings:startMinimized"] = "false",
                ["InterviewAssistant:settings:enableTrayIcon"] = "true",
                ["InterviewAssistant:prompts:codingInterview"] = "Test coding prompt",
                ["InterviewAssistant:prompts:generalInterview"] = "Test general prompt",
                ["InterviewAssistant:api:groq:apiKey"] = "test-api-key",
                ["InterviewAssistant:api:groq:model"] = "llama-3.1-70b-versatile",
                ["InterviewAssistant:screenshot:format"] = "Png",
                ["InterviewAssistant:screenshot:quality"] = "90"
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inConfig)
                .Build();

            _configManager = new ConfigurationManagerImpl(_configuration);
        }

        [Fact]
        public async Task LoadConfigurationAsync_ShouldLoadValidConfiguration()
        {
            // Act
            var config = await _configManager.LoadConfigurationAsync();

            // Assert
            config.Should().NotBeNull();
            config.Settings.AutoStart.Should().BeTrue();
            config.Settings.StartMinimized.Should().BeFalse();
            config.Settings.EnableTrayIcon.Should().BeTrue();
            config.Prompts.CodingInterview.Should().Be("Test coding prompt");
            config.Prompts.GeneralInterview.Should().Be("Test general prompt");
            config.Api.Groq.ApiKey.Should().Be("test-api-key");
            config.Api.Groq.Model.Should().Be("llama-3.1-70b-versatile");
            config.Screenshot.Format.Should().Be(InterviewAssistant.Core.Models.ImageFormat.Png);
            config.Screenshot.Quality.Should().Be(90);
        }

        [Fact]
        public async Task GetPromptForShortcut_ShouldReturnCorrectPrompt()
        {
            // Arrange
            await _configManager.LoadConfigurationAsync();
            
            // Act
            var prompt = _configManager.GetPromptForShortcut("test-shortcut");

            // Assert
            prompt.Should().NotBeNullOrEmpty();
            prompt.Should().Be("Test coding prompt"); // Default prompt
        }

        [Fact]
        public async Task AddOrUpdateShortcut_ShouldAddNewShortcut()
        {
            // Arrange
            await _configManager.LoadConfigurationAsync();
            
            var newShortcut = new KeyboardShortcut
            {
                Id = "new-shortcut",
                Combination = "Ctrl+Alt+T",
                Description = "Test shortcut",
                PromptId = "generalInterview"
            };

            // Act
            _configManager.AddOrUpdateShortcut(newShortcut);
            var shortcuts = _configManager.GetKeyboardShortcuts();

            // Assert
            shortcuts.Should().NotBeEmpty();
            shortcuts.Should().Contain(s => s.Id == "new-shortcut");
            shortcuts.Should().Contain(s => s.Combination == "Ctrl+Alt+T");
        }

        [Fact]
        public async Task ValidateConfiguration_WithValidConfig_ShouldReturnTrue()
        {
            // Arrange
            await _configManager.LoadConfigurationAsync();

            // Act
            var isValid = _configManager.ValidateConfiguration(out var validationErrors);

            // Assert
            isValid.Should().BeTrue();
            validationErrors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateConfiguration_WithInvalidConfig_ShouldReturnFalse()
        {
            // Arrange
            await _configManager.LoadConfigurationAsync();
            
            // Remove API key to make config invalid
            var config = _configManager.GetConfiguration();
            config.Api.Groq.ApiKey = string.Empty;
            await _configManager.SaveConfigurationAsync(config);

            // Act
            var isValid = _configManager.ValidateConfiguration(out var validationErrors);

            // Assert
            isValid.Should().BeFalse();
            validationErrors.Should().NotBeEmpty();
            validationErrors.Should().Contain(e => e.Contains("API key"));
        }

        [Fact]
        public async Task GetAllPrompts_ShouldReturnAllPrompts()
        {
            // Arrange
            await _configManager.LoadConfigurationAsync();

            // Act
            var prompts = _configManager.GetAllPrompts();

            // Assert
            prompts.Should().NotBeEmpty();
            prompts.Should().ContainKey("CodingInterview");
            prompts.Should().ContainKey("GeneralInterview");
            prompts["CodingInterview"].Should().Be("Test coding prompt");
            prompts["GeneralInterview"].Should().Be("Test general prompt");
        }

        [Fact]
        public async Task AddOrUpdatePrompt_ShouldUpdateExistingPrompt()
        {
            // Arrange
            await _configManager.LoadConfigurationAsync();

            // Act
            _configManager.AddOrUpdatePrompt("CodingInterview", "Updated coding prompt");
            var prompts = _configManager.GetAllPrompts();

            // Assert
            prompts.Should().ContainKey("CodingInterview");
            prompts["CodingInterview"].Should().Be("Updated coding prompt");
        }

        [Fact]
        public async Task RemoveShortcut_ShouldRemoveExistingShortcut()
        {
            // Arrange
            await _configManager.LoadConfigurationAsync();
            
            var newShortcut = new KeyboardShortcut
            {
                Id = "remove-shortcut",
                Combination = "Ctrl+Alt+R",
                Description = "Remove test shortcut"
            };
            
            _configManager.AddOrUpdateShortcut(newShortcut);
            var shortcutsBefore = _configManager.GetKeyboardShortcuts();

            // Act
            var removed = _configManager.RemoveShortcut("remove-shortcut");
            var shortcutsAfter = _configManager.GetKeyboardShortcuts();

            // Assert
            removed.Should().BeTrue();
            shortcutsBefore.Should().HaveCountGreaterThan(shortcutsAfter.Count);
            shortcutsAfter.Should().NotContain(s => s.Id == "remove-shortcut");
        }
    }
}