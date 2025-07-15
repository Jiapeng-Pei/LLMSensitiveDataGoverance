using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.AgentIntegration;
using LLMSensitiveDataGoverance.AgentIntegration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLMSensitiveDataGoverance.Examples.AgentIntegration;

/// <summary>
/// Example demonstrating integration with AI Agent systems
/// </summary>
public class AgentIntegrationExample
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgentIntegrationExample> _logger;
    private readonly AgentLabelProvider _labelProvider;
    private readonly LLMResponseProcessor _responseProcessor;
    private readonly GroundingDataProcessor _groundingProcessor;
    
    public AgentIntegrationExample(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<AgentIntegrationExample>>();
        _labelProvider = serviceProvider.GetRequiredService<AgentLabelProvider>();
        _responseProcessor = serviceProvider.GetRequiredService<LLMResponseProcessor>();
        _groundingProcessor = serviceProvider.GetRequiredService<GroundingDataProcessor>();
    }
    
    /// <summary>
    /// Run comprehensive agent integration example
    /// </summary>
    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Agent Integration Example");
        
        try
        {
            // Example 1: Process grounding data for AI agent
            await ProcessGroundingDataExample();
            
            // Example 2: Handle LLM response with sensitivity labels
            await ProcessLLMResponseExample();
            
            // Example 3: Batch processing of multiple documents
            await BatchProcessingExample();
            
            // Example 4: Real-time label validation during conversation
            await RealTimeValidationExample();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during agent integration example");
            throw;
        }
        
        _logger.LogInformation("Agent Integration Example completed successfully");
    }
    
    /// <summary>
    /// Demonstrate processing grounding data for AI agent consumption
    /// </summary>
    private async Task ProcessGroundingDataExample()
    {
        _logger.LogInformation("=== Grounding Data Processing Example ===");
        
        var groundingDataItems = new List<GroundingData>
        {
            new GroundingData
            {
                Id = "hr-001",
                Content = "Employee performance review for Q4 2024",
                Source = "HR System",
                DataType = "document",
                Metadata = new Dictionary<string, object>
                {
                    { "department", "Human Resources" },
                    { "classification", "internal" }
                }
            },
            new GroundingData
            {
                Id = "finance-001",
                Content = "Quarterly financial projections and budget allocation",
                Source = "Finance Database",
                DataType = "spreadsheet",
                Metadata = new Dictionary<string, object>
                {
                    { "department", "Finance" },
                    { "classification", "confidential" }
                }
            }
        };
        
        foreach (var item in groundingDataItems)
        {
            var processedData = await _groundingProcessor.ProcessAsync(item);
            
            _logger.LogInformation($"Processed: {item.Id}");
            _logger.LogInformation($"  Label: {processedData.Label?.Name ?? "None"}");
            _logger.LogInformation($"  Can be used for grounding: {processedData.Label?.Protection?.PreventGrounding == false}");
            _logger.LogInformation($"  Requires encryption: {processedData.Label?.Protection?.RequireEncryption == true}");
        }
    }
    
    /// <summary>
    /// Demonstrate LLM response processing with different sensitivity levels
    /// </summary>
    private async Task ProcessLLMResponseExample()
    {
        _logger.LogInformation("\n=== LLM Response Processing Example ===");
        
        var responses = new List<(string content, string expectedLabel)>
        {
            ("Here's the company's public product roadmap for 2025.", "Public"),
            ("The internal team structure shows 15 developers across 3 teams.", "Internal"),
            ("The financial projections indicate 25% growth in Q2.", "Confidential"),
            ("Access to the classified research requires special authorization.", "Restricted")
        };
        
        foreach (var (content, expectedLabel) in responses)
        {
            var labelInfo = await _labelProvider.GetLabelForContentAsync(content);
            var processedResponse = await _responseProcessor.ProcessResponseAsync(content, labelInfo);
            
            _logger.LogInformation($"Original: {content}");
            _logger.LogInformation($"Expected Label: {expectedLabel}");
            _logger.LogInformation($"Detected Label: {labelInfo?.Name ?? "None"}");
            _logger.LogInformation($"Processed Response: {processedResponse.FormattedResponse}");
            _logger.LogInformation($"Display Label: {processedResponse.ShouldDisplay}");
            _logger.LogInformation("---");
        }
    }
    
    /// <summary>
    /// Demonstrate batch processing of multiple documents
    /// </summary>
    private async Task BatchProcessingExample()
    {
        _logger.LogInformation("\n=== Batch Processing Example ===");
        
        var documents = GenerateSampleDocuments();
        var processingTasks = documents.Select(async doc =>
        {
            var result = await _groundingProcessor.ProcessAsync(doc);
            return new { Document = doc, Result = result };
        });
        
        var results = await Task.WhenAll(processingTasks);
        
        // Group by sensitivity level
        var groupedResults = results.GroupBy(r => r.Result.Label?.Priority ?? LabelPriority.Public);
        
        foreach (var group in groupedResults.OrderBy(g => g.Key))
        {
            _logger.LogInformation($"Sensitivity Level: {group.Key}");
            foreach (var item in group)
            {
                _logger.LogInformation($"  - {item.Document.Id}: {item.Document.Source}");
            }
        }
        
        _logger.LogInformation($"Total documents processed: {results.Length}");
        _logger.LogInformation($"Documents requiring encryption: {results.Count(r => r.Result.Label?.Protection?.RequireEncryption == true)}");
        _logger.LogInformation($"Documents restricted from grounding: {results.Count(r => r.Result.Label?.Protection?.PreventGrounding == true)}");
    }
    
    /// <summary>
    /// Demonstrate real-time label validation during conversation
    /// </summary>
    private async Task RealTimeValidationExample()
    {
        _logger.LogInformation("\n=== Real-time Validation Example ===");
        
        var conversationTurns = new List<string>
        {
            "What's our current employee count?",
            "Can you show me the latest financial reports?",
            "Tell me about our new product launch strategy",
            "What are the details of the classified project Alpha?",
            "How are we performing compared to competitors?"
        };
        
        foreach (var turn in conversationTurns)
        {
            _logger.LogInformation($"User Query: {turn}");
            
            // Simulate AI agent processing
            var suggestedLabel = await _labelProvider.GetLabelForContentAsync(turn);
            var canProcess = await ValidateConversationTurn(turn, suggestedLabel);
            
            if (canProcess)
            {
                var mockResponse = await GenerateMockResponse(turn);
                var processedResponse = await _responseProcessor.ProcessResponseAsync(mockResponse, suggestedLabel);
                
                _logger.LogInformation($"  Response: {processedResponse.FormattedResponse}");
                _logger.LogInformation($"  Label: {suggestedLabel?.Name ?? "None"}");
            }
            else
            {
                _logger.LogInformation($"  Response: Access denied due to sensitivity restrictions");
            }
            
            _logger.LogInformation("---");
        }
    }
    
    /// <summary>
    /// Generate sample documents for testing
    /// </summary>
    private List<GroundingData> GenerateSampleDocuments()
    {
        return new List<GroundingData>
        {
            new GroundingData
            {
                Id = "doc-001",
                Content = "Company newsletter for all employees",
                Source = "Internal Communications",
                DataType = "text/html"
            },
            new GroundingData
            {
                Id = "doc-002",
                Content = "Quarterly sales targets and performance metrics",
                Source = "Sales Database",
                DataType = "application/json"
            },
            new GroundingData
            {
                Id = "doc-003",
                Content = "Customer contract negotiations and terms",
                Source = "Legal Department",
                DataType = "application/pdf"
            },
            new GroundingData
            {
                Id = "doc-004",
                Content = "Research and development roadmap - confidential",
                Source = "R&D Labs",
                DataType = "text/markdown"
            },
            new GroundingData
            {
                Id = "doc-005",
                Content = "Executive compensation and board decisions",
                Source = "Board of Directors",
                DataType = "application/vnd.ms-excel"
            }
        };
    }
    
    /// <summary>
    /// Validate if a conversation turn can be processed
    /// </summary>
    private async Task<bool> ValidateConversationTurn(string query, SensitivityLabel suggestedLabel)
    {
        if (suggestedLabel == null) return true;
        
        // Simulate user permission check
        var hasPermission = suggestedLabel.Priority switch
        {
            LabelPriority.Public => true,
            LabelPriority.Internal => true,
            LabelPriority.Confidential => true, // Assume user has access
            LabelPriority.HighlyConfidential => false, // Restricted access
            LabelPriority.Restricted => false, // No access
            _ => true
        };
        
        return hasPermission && !suggestedLabel.Protection.PreventGrounding;
    }
    
    /// <summary>
    /// Generate mock response for demonstration
    /// </summary>
    private async Task<string> GenerateMockResponse(string query)
    {
        // Simulate AI response generation
        await Task.Delay(100); // Simulate processing time
        
        return query.ToLower() switch
        {
            var q when q.Contains("employee count") => "The company currently has 1,250 employees across all departments.",
            var q when q.Contains("financial reports") => "The Q4 financial report shows revenue growth of 15% year-over-year.",
            var q when q.Contains("product launch") => "The new product launch is scheduled for Q2 2025 with marketing campaign starting in March.",
            var q when q.Contains("project alpha") => "Project Alpha details are classified and require special authorization.",
            var q when q.Contains("competitors") => "Our market performance shows competitive advantage in customer satisfaction metrics.",
            _ => "I can help you with that information."
        };
    }
}