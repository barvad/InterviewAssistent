# Interview Assistant

A Windows background application that captures global keyboard shortcuts, takes screenshots, sends them to Groq API for processing, and simulates keyboard input based on API responses.

## Architecture Overview

The application follows a modular architecture with the following components:

### Core Projects

1. **InterviewAssistant.Core** - Core interfaces and models
2. **InterviewAssistant.Configuration** - Configuration management system
3. **InterviewAssistant.Service** - Main Windows Service implementation
4. **InterviewAssistant.Tests** - Unit and integration tests

### Key Services

- **Keyboard Hook Service** - Global keyboard monitoring
- **Screenshot Service** - Screen capture functionality
- **Groq API Service** - AI integration
- **Keyboard Simulation Service** - Input simulation
- **Chrome Integration Service** - Browser-specific functionality
- **Configuration Manager** - Settings and prompts management
- **Logging Service** - Comprehensive logging system

## Prerequisites

- .NET 8.0 SDK
- Windows 10 or later
- Administrator privileges for service installation

## Setup Instructions

### 1. Build the Solution

```bash
dotnet build
```

### 2. Install the Windows Service

```bash
# Run as Administrator
cd InterviewAssistant.Service/bin/Debug/net8.0-windows
InterviewAssistant.Service.exe install
```

### 3. Start the Service

```bash
# Run as Administrator
net start InterviewAssistant
```

### 4. Configure the Application

Edit `appsettings.json` to configure:
- Groq API key
- Keyboard shortcuts
- Prompts
- Logging settings

## Configuration

### API Configuration

```json
"InterviewAssistant": {
  "api": {
    "groq": {
      "apiKey": "your-api-key-here",
      "model": "llama-3.1-70b-versatile",
      "maxTokens": 4096,
      "temperature": 0.7
    }
  }
}
```

### Keyboard Shortcuts

```json
"InterviewAssistant": {
  "settings": {
    "keyboardShortcuts": [
      {
        "id": "capture-interview",
        "combination": "Ctrl+Shift+I",
        "description": "Capture screenshot and send to AI",
        "enabled": true,
        "promptId": "codingInterview"
      }
    ]
  }
}
```

### Prompts

```json
"InterviewAssistant": {
  "prompts": {
    "codingInterview": "Analyze this code and provide suggestions for improvement...",
    "generalInterview": "Summarize the content and provide key insights..."
  }
}
```

## Service Management

### Install the Service

```bash
InterviewAssistant.Service.exe install
```

### Uninstall the Service

```bash
InterviewAssistant.Service.exe uninstall
```

### Start the Service

```bash
net start InterviewAssistant
```

### Stop the Service

```bash
net stop InterviewAssistant
```

### Check Service Status

```bash
sc query InterviewAssistant
```

## Development

### Running in Development Mode

For development and testing, you can run the application in console mode:

```bash
cd InterviewAssistant.Service
dotnet run
```

### Testing

Run the unit tests:

```bash
cd InterviewAssistant.Tests
dotnet test
```

### Project Structure

```
InterviewAssistant/
├── InterviewAssistant.Core/          # Core interfaces and models
│   ├── Interfaces/                   # Service interfaces
│   └── Models/                       # Data models
├── InterviewAssistant.Configuration/  # Configuration management
│   └── Implementations/              # Configuration implementations
├── InterviewAssistant.Service/       # Main service implementation
│   ├── Services/                     # Service implementations
│   ├── Utils/                        # Utility classes
│   └── appsettings.json             # Configuration file
└── InterviewAssistant.Tests/         # Test projects
    └── Unit/                         # Unit tests
```

## Key Features

### Global Keyboard Hooking
- Captures system-wide keyboard shortcuts
- Configurable shortcut combinations
- Chrome-specific integration

### Screenshot Capture
- Full screen and region capture
- Multiple image formats (PNG, JPEG, BMP, TIFF)
- Optimized for API transmission

### Groq API Integration
- AI-powered content analysis
- Configurable prompts
- Rate limiting and retry mechanisms

### Keyboard Simulation
- Reliable input simulation
- Unicode support
- Timing and sequencing control

### Chrome Integration
- Browser detection and activation
- Version compatibility checking
- Focus management

### Logging and Monitoring
- Comprehensive logging system
- Performance metrics
- Error tracking and reporting

## Troubleshooting

### Common Issues

1. **Service Installation Fails**
   - Run as Administrator
   - Check .NET Framework installation
   - Verify service executable path

2. **Keyboard Hook Not Working**
   - Check if another application is using global hooks
   - Verify shortcut combinations are not in use
   - Check Windows security settings

3. **Chrome Integration Issues**
   - Ensure Chrome is installed and running
   - Check Chrome version compatibility
   - Verify window title patterns

4. **API Connection Problems**
   - Verify API key is correct
   - Check network connectivity
   - Review API rate limits

### Logs

Application logs are located in:
- `%APPDATA%\InterviewAssistant\logs\`
- Console output when running in development mode

## Security Considerations

- API keys are stored in configuration files
- Service runs with minimal privileges
- Input simulation requires careful handling
- File access is restricted to application directories

## Performance Optimization

- Memory usage target: < 100MB RAM
- CPU usage target: < 5% average
- Response time target: < 2 seconds for screenshot capture
- API latency target: < 5 seconds for Groq API response

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For support and questions:
- Create an issue in the GitHub repository
- Check the troubleshooting section
- Review the logs for error details