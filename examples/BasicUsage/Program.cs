using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Services;
using LLMSensitiveDataGoverance.Core.Repositories;
using LLMSensitiveDataGoverance.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LLMSensitiveDataGoverance.Examples.BasicUsage;

/// <summary>
/// Basic usage example demonstrating core functionality of the LLM Sensitive Data Governance system
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Starting LLM Sensitive Data Governance Basic Usage Example");
        
        try
        {
            // Example 1: Basic label classification
            await RunBasicClassificationExample(serviceProvider, logger);
            
            // Example 2: LLM response processing
            await RunLLMResponseProcessingExample(serviceProvider, logger);
            
            // Example 3: Grounding data validation
            await RunGroundingDataValidationExample(serviceProvider, logger);
            
            // Example 4: Multiple labels handling
            await RunMultipleLabelsExample(serviceProvider, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during example execution");
        }
        finally
        {
            logger.LogInformation("Basic usage example completed");
        }
    }
    
    /// <summary>
    /// Configure dependency injection services
    /// </summary>
    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        services.AddSingleton<ILabelRepository, InMemoryLabelRepository>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<ILabelValidator, LabelValidationService>();
        services.AddSingleton<ISensitivityLabelService, SensitivityLabelService>();
        services.AddSingleton<LabelClassificationService>();
    }
    
    /// <summary>
    /// Demonstrate basic label classification
    /// </summary>
    private static async Task RunBasicClassificationExample(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("=== Basic Classification Example ===");
        
        var labelService = serviceProvider.GetRequiredService<ISensitivityLabelService>();
        
        // Create sample grounding data
        var groundingData = new GroundingData
        {
            Id = "doc-001",
            Content = "Employee salary information: John Doe - $75,000 annually",
            Source = "HR Database",
            DataType = "text/plain",
            Metadata = new Dictionary<string, object>
            {
                { "department", "HR" },
                { "confidential", true }
            },
            LastModified = DateTime.UtcNow
        };
        
        // Classify the data
        var result = await labelService.ClassifyAsync(groundingData);
        
        logger.LogInformation($"Classification Result:");
        logger.LogInformation($"  Label: {result.Label.Name}");
        logger.LogInformation($"  Priority: {result.Label.Priority}");
        logger.LogInformation($"  Allow Grounding: {result.AllowGrounding}");
        logger.LogInformation($"  Allow Copy/Paste: {result.AllowCopyPaste}");
        logger.LogInformation($"  Formatted Response: {result.FormattedResponse}");
    }
    
    /// <summary>
    /// Demonstrate LLM response processing with sensitivity labels
    /// </summary>
    private static async Task RunLLMResponseProcessingExample(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("\n=== LLM Response Processing Example ===");
        
        var labelService = serviceProvider.GetRequiredService<ISensitivityLabelService>();
        var labelRepository = serviceProvider.GetRequiredService<ILabelRepository>();
        
        // Get a confidential label
        var confidentialLabel = await labelRepository.GetByNameAsync("Confidential");
        
        // Sample LLM response
        var llmResponse = "Based on the HR data, the employee's compensation is competitive within the industry standards.";
        
        // Process the response with sensitivity label
        var processedResponse = await labelService.ProcessLLMResponseAsync(llmResponse, confidentialLabel);
        
        logger.LogInformation($"Original Response: {llmResponse}");
        logger.LogInformation($"Processed Response: {processedResponse.FormattedResponse}");
        logger.LogInformation($"Protection Applied: {!processedResponse.AllowCopyPaste}");
    }
    
    /// <summary>
    /// Demonstrate grounding data validation
    /// </summary>
    private static async Task RunGroundingDataValidationExample(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("\n=== Grounding Data Validation Example ===");
        
        var labelValidator = serviceProvider.GetRequiredService<ILabelValidator>();
        var labelRepository = serviceProvider.GetRequiredService<ILabelRepository>();
        
        // Create test data with different sensitivity levels
        var publicData = new GroundingData
        {
            Id = "public-001",
            Content = "Company's public announcement about new product launch",
            Source = "Press Release",
            DataType = "text/plain"
        };
        
        var restrictedData = new GroundingData
        {
            Id = "restricted-001",
            Content = "Classified research data - Project Alpha specifications",
            Source = "R&D Database",
            DataType = "text/plain"
        };
        
        var publicLabel = await labelRepository.GetByNameAsync("Public");
        var restrictedLabel = await labelRepository.GetByNameAsync("Restricted");
        
        // Validate label applications
        var canApplyPublic = await labelValidator.CanApplyLabelAsync(publicData, publicLabel);
        var canApplyRestricted = await labelValidator.CanApplyLabelAsync(restrictedData, restrictedLabel);
        
        logger.LogInformation($"Can apply Public label to public data: {canApplyPublic}");
        logger.LogInformation($"Can apply Restricted label to restricted data: {canApplyRestricted}");
        
        // Check label compatibility
        var areCompatible = await labelValidator.IsLabelCompatibleAsync(publicLabel, restrictedLabel);
        logger.LogInformation($"Public and Restricted labels compatible: {areCompatible}");
    }
    
    /// <summary>
    /// Demonstrate handling multiple labels and priority resolution
    /// </summary>
    private static async Task RunMultipleLabelsExample(IServiceProvider serviceProvider, ILogger logger)
    {
        logger.LogInformation("\n=== Multiple Labels Example ===");
        
        var labelService = serviceProvider.GetRequiredService<ISensitivityLabelService>();
        var labelRepository = serviceProvider.GetRequiredService<ILabelRepository>();
        
        // Get multiple labels
        var allLabels = await labelRepository.GetAllAsync();
        var multipleLabelsList = allLabels.Take(3).ToList();
        
        logger.LogInformation($"Available labels: {string.Join(", ", multipleLabelsList.Select(l => l.Name))}");
        
        // Get highest priority label
        var highestPriorityLabel = await labelService.GetHighestPriorityLabelAsync(multipleLabelsList);
        
        logger.LogInformation($"Highest priority label: {highestPriorityLabel.Name} (Priority: {highestPriorityLabel.Priority})");
        
        // Demonstrate label formatting
        var sampleContent = "This content requires the highest level of protection.";
        var formattedContent = await labelService.FormatResponseWithLabelAsync(sampleContent, highestPriorityLabel);
        
        logger.LogInformation($"Original content: {sampleContent}");
        logger.LogInformation($"Formatted content: {formattedContent}");
    }
}