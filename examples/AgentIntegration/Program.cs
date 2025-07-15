using LLMSensitiveDataGoverance.AgentIntegration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LLMSensitiveDataGoverance.Examples.AgentIntegration;

/// <summary>
/// Program entry point for Agent Integration example
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting Agent Integration Example");
        
        try
        {
            var example = new AgentIntegrationExample(host.Services);
            await example.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run agent integration example");
            Environment.ExitCode = 1;
        }
        
        logger.LogInformation("Agent Integration Example finished");
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add core services
                services.AddLLMSensitiveDataGovernance();
                
                // Add logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}