using LLMSensitiveDataGoverance.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LLMSensitiveDataGoverance.Examples.PerformanceTesting;

/// <summary>
/// Program entry point for performance testing
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting Performance Testing Suite");
        
        try
        {
            var testRunner = new PerformanceTestRunner(host.Services);
            await testRunner.RunAllTestsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Performance testing failed");
            Environment.ExitCode = 1;
        }
        
        logger.LogInformation("Performance Testing Suite completed");
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLLMSensitiveDataGovernanceCore();
                
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}