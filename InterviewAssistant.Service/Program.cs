using InterviewAssistant.Service;
using InterviewAssistant.Service.Services;
using InterviewAssistant.Configuration.Implementations;
using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.ServiceProcess;

// Create host builder
var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/interview-assistant-.txt", 
            rollingInterval: Serilog.RollingInterval.Day,
            retainedFileCountLimit: 7))
    .ConfigureServices((context, services) =>
    {
        
        // Register configuration
        services.Configure<InterviewAssistantConfiguration>(
            context.Configuration.GetSection(InterviewAssistantConfiguration.SectionName));
        
        // Register services
        services.AddSingleton<IKeyboardHookService, KeyboardHookService>();
        services.AddSingleton<IScreenshotService, ScreenshotService>();
        services.AddSingleton<IGroqApiService, GroqApiService>();
        services.AddSingleton<IKeyboardSimulationService, KeyboardSimulationService>();
        services.AddSingleton<IConfigurationManager, ConfigurationManager>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IChromeIntegrationService, ChromeIntegrationService>();
        
        // Register the main service
        services.AddSingleton<InterviewAssistantService>();
    })
    .Build();

// Check if running as a Windows Service
if (Environment.UserInteractive)
{
    // Run as console app for development/testing
    Log.Information("Running in interactive mode");
    
    var service = host.Services.GetRequiredService<InterviewAssistantService>();
    await service.RunInteractiveAsync();
}
else
{
    // Run as Windows Service
    Log.Information("Running as Windows Service");
    
    using var service = new WindowsServiceWrapper(host);
    ServiceBase.Run(service);
}

// Windows Service Wrapper
public class WindowsServiceWrapper : ServiceBase
{
    private readonly IHost _host;
    private InterviewAssistantService? _service;

    public WindowsServiceWrapper(IHost host)
    {
        _host = host;
        ServiceName = "InterviewAssistant";
        CanStop = true;
        CanPauseAndContinue = false;
        AutoLog = true;
    }

    protected override async void OnStart(string[] args)
    {
        try
        {
            await _host.StartAsync();
            _service = _host.Services.GetRequiredService<InterviewAssistantService>();
            await _service.StartAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to start service");
            throw;
        }
    }

    protected override async void OnStop()
    {
        try
        {
            if (_service != null)
            {
                await _service.StopAsync();
            }
            
            await _host.StopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error stopping service");
        }
    }
}