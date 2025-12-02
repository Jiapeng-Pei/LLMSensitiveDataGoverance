
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LLMSensitiveDataGoverance.Core.Interfaces; 
using LLMSensitiveDataGoverance.Core.Services;
using LLMSensitiveDataGoverance.Core.Repositories;
using LLMSensitiveDataGoverance.CLI.Commands;
using LLMSensitiveDataGoverance.CLI.Handlers;

namespace LLMSensitiveDataGoverance.CLI;

/// <summary>
/// Main entry point for the CLI application
/// </summary>
public class Program
{
    /// <summary>
    /// Main method for the CLI application
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code</returns>
    public static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        try
        {
            var rootCommand = CreateRootCommand(host.Services);
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetService<ILogger<Program>>();
            logger?.LogError(ex, "An unhandled exception occurred");
            Console.WriteLine($"Error: {ex.Message}");
            return -1;
        }
        finally
        {
            await host.StopAsync();
        }
    }

    /// <summary>
    /// Creates the host builder for dependency injection
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Host builder</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register core services
                services.AddScoped<ISensitivityLabelService, SensitivityLabelService>();
                services.AddScoped<ILabelRepository, JsonLabelRepository>();
                services.AddScoped<IEncryptionService, EncryptionService>();
                services.AddScoped<ILabelValidator, LabelValidationService>();

                // Register CLI specific services
                services.AddScoped<CommandHandler>();
                services.AddScoped<OutputFormatter>();

                // Register commands
                services.AddScoped<ClassifyCommand>();
                services.AddScoped<ListLabelsCommand>();
                services.AddScoped<ValidateCommand>();
                services.AddScoped<ConfigureCommand>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

    /// <summary>
    /// Creates the root command with all subcommands
    /// </summary>
    /// <param name="services">Service provider</param>
    /// <returns>Root command</returns>
    private static RootCommand CreateRootCommand(IServiceProvider services)
    {
        var rootCommand = new RootCommand("LLM Sensitive Data Governance CLI - Manage sensitivity labels for grounding data")
        {
            services.GetRequiredService<ClassifyCommand>().GetCommand(),
            services.GetRequiredService<ListLabelsCommand>().GetCommand(),
            services.GetRequiredService<ValidateCommand>().GetCommand(),
            services.GetRequiredService<ConfigureCommand>().GetCommand()
        };

        return rootCommand;
    }
}
