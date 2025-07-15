# Agent Integration Guide

## Overview

The LLM Sensitive Data Governance system provides seamless integration with AI Agent applications, enabling automatic sensitivity label management for grounding data and LLM responses.

## Quick Start

### 1. Add Package Reference

```xml
<PackageReference Include="LLMSensitiveDataGoverance.AgentIntegration" Version="1.0.0" />
```

### 2. Register Services

```csharp
using LLMSensitiveDataGoverance.AgentIntegration.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add sensitivity label services
builder.Services.AddSensitivityLabelServices();
builder.Services.AddAgentIntegration();

var app = builder.Build();
```

### 3. Basic Usage

```csharp
public class ChatService
{
    private readonly LLMResponseProcessor _responseProcessor;
    private readonly GroundingDataProcessor _groundingProcessor;

    public ChatService(
        LLMResponseProcessor responseProcessor,
        GroundingDataProcessor groundingProcessor)
    {
        _responseProcessor = responseProcessor;
        _groundingProcessor = groundingProcessor;
    }

    public async Task<string> ProcessChatAsync(string userInput, List<GroundingData> groundingData)
    {
        // Process grounding data
        var processedGrounding = await _groundingProcessor.ProcessGroundingDataAsync(groundingData);
        
        // Filter out restricted content
        var allowedGrounding = processedGrounding.Where(g => g.AllowGrounding).ToList();
        
        // Generate LLM response (your implementation)
        var llmResponse = await GenerateLLMResponseAsync(userInput, allowedGrounding);
        
        // Process response with sensitivity labels
        var processedResponse = await _responseProcessor.ProcessResponseAsync(llmResponse, processedGrounding);
        
        return processedResponse.FormattedResponse;
    }
}
```

## Core Components

### AgentLabelProvider

Provides label information for agent operations.

```csharp
public class AgentLabelProvider
{
    private readonly ISensitivityLabelService _labelService;

    public async Task<SensitivityLabel> GetLabelForContentAsync(string content)
    {
        var groundingData = new GroundingData { Content = content };
        var response = await _labelService.ClassifyAsync(groundingData);
        return response.Label;
    }

    public async Task<SensitivityLabel> GetHighestPriorityLabelAsync(
        IEnumerable<GroundingData> groundingData)
    {
        var labels = groundingData.Select(g => g.Label).Where(l => l != null);
        return await _labelService.GetHighestPriorityLabelAsync(labels);
    }
}
```

### LLMResponseProcessor

Processes LLM responses with sensitivity label formatting.

```csharp
public class LLMResponseProcessor
{
    public async Task<SensitivityLabelResponse> ProcessResponseAsync(
        string response, 
        IEnumerable<GroundingData> groundingData)
    {
        var highestLabel = await GetHighestPriorityLabelAsync(groundingData);
        
        if (highestLabel == null)
        {
            return new SensitivityLabelResponse
            {
                Content = response,
                FormattedResponse = response,
                ShouldDisplay = true,
                AllowCopyPaste = true,
                AllowGrounding = true
            };
        }

        var formattedResponse = await FormatResponseWithLabelAsync(response, highestLabel);
        
        return new SensitivityLabelResponse
        {
            Label = highestLabel,
            Content = response,
            FormattedResponse = formattedResponse,
            ShouldDisplay = !highestLabel.Protection.PreventExtraction,
            AllowCopyPaste = !highestLabel.Protection.PreventCopyPaste,
            AllowGrounding = !highestLabel.Protection.PreventGrounding
        };
    }
}
```

### GroundingDataProcessor

Processes grounding data with sensitivity labels.

```csharp
public class GroundingDataProcessor
{
    public async Task<IEnumerable<GroundingData>> ProcessGroundingDataAsync(
        IEnumerable<GroundingData> groundingData)
    {
        var processedData = new List<GroundingData>();

        foreach (var data in groundingData)
        {
            var response = await _labelService.ClassifyAsync(data);
            data.Label = response.Label;
            
            // Only include if grounding is allowed
            if (response.AllowGrounding)
            {
                processedData.Add(data);
            }
        }

        return processedData;
    }
}
```

## Integration Patterns

### 1. Pre-Processing Pattern

Filter grounding data before LLM processing:

```csharp
public async Task<string> GenerateResponseAsync(string prompt, List<GroundingData> documents)
{
    // Classify and filter documents
    var allowedDocuments = new List<GroundingData>();
    
    foreach (var doc in documents)
    {
        var response = await _labelService.ClassifyAsync(doc);
        if (response.AllowGrounding)
        {
            allowedDocuments.Add(doc);
        }
    }
    
    // Use only allowed documents for grounding
    var llmResponse = await _llmService.GenerateAsync(prompt, allowedDocuments);
    
    // Process response with labels
    return await _responseProcessor.ProcessResponseAsync(llmResponse, allowedDocuments);
}
```

### 2. Post-Processing Pattern

Apply labels after LLM generation:

```csharp
public async Task<string> GenerateResponseAsync(string prompt, List<GroundingData> documents)
{
    // Generate response first
    var llmResponse = await _llmService.GenerateAsync(prompt, documents);
    
    // Classify all used documents
    var labeledDocuments = await _groundingProcessor.ProcessGroundingDataAsync(documents);
    
    // Apply highest priority label to response
    var processedResponse = await _responseProcessor.ProcessResponseAsync(
        llmResponse, labeledDocuments);
    
    return processedResponse.FormattedResponse;
}
```

### 3. Streaming Pattern

Handle streaming responses with labels:

```csharp
public async IAsyncEnumerable<string> GenerateStreamingResponseAsync(
    string prompt, 
    List<GroundingData> documents)
{
    // Process grounding data first
    var processedGrounding = await _groundingProcessor.ProcessGroundingDataAsync(documents);
    var highestLabel = await _labelProvider.GetHighestPriorityLabelAsync(processedGrounding);
    
    // Add label header if needed
    if (highestLabel != null && _settings.IncludeLabelInResponse)
    {
        yield return await FormatLabelHeaderAsync(highestLabel);
    }
    
    // Stream LLM response
    await foreach (var chunk in _llmService.GenerateStreamingAsync(prompt, processedGrounding))
    {
        yield return chunk;
    }
    
    // Add label footer if needed
    if (highestLabel != null && _settings.ShowVisualIndicators)
    {
        yield return await FormatLabelFooterAsync(highestLabel);
    }
}
```

## Configuration

### AgentSettings

Configure agent integration behavior:

```json
{
  "AgentIntegration": {
    "DefaultResponseFormat": "Html",
    "IncludeLabelInResponse": true,
    "PreventGroundingOnRestricted": true,
    "ShowVisualIndicators": true,
    "CopyPasteWarnings": true,
    "ResponseFormats": {
      "Html": {
        "LabelTemplate": "<div class='sensitivity-label {priority}'>{displayText}</div>",
        "ContentTemplate": "<div class='labeled-content'>{content}</div>",
        "CopyPasteWarning": "<div class='copy-warning'>This content contains sensitive information</div>"
      },
      "Markdown": {
        "LabelTemplate": "⚠️ **{displayText}**",
        "ContentTemplate": "{content}",
        "CopyPasteWarning": "> ⚠️ This content contains sensitive information"
      }
    }
  }
}
```

### Custom Response Formatting

Implement custom response formatting:

```csharp
public class CustomResponseFormatter : IResponseFormatter
{
    public async Task<string> FormatResponseAsync(string content, SensitivityLabel label)
    {
        if (label == null) return content;
        
        return label.Priority switch
        {
            LabelPriority.Public => content,
            LabelPriority.Internal => $"[INTERNAL] {content}",
            LabelPriority.Confidential => $"[CONFIDENTIAL] {content} [CONFIDENTIAL]",
            LabelPriority.HighlyConfidential => $"🔒 [HIGHLY CONFIDENTIAL] {content} [HIGHLY CONFIDENTIAL] 🔒",
            LabelPriority.Restricted => $"🚫 [RESTRICTED ACCESS] {content} [RESTRICTED ACCESS] 🚫",
            _ => content
        };
    }
}

// Register custom formatter
services.AddSingleton<IResponseFormatter, CustomResponseFormatter>();
```

## Advanced Scenarios

### Multi-Document Processing

Handle multiple documents with different labels:

```csharp
public async Task<string> ProcessMultiDocumentQueryAsync(
    string query, 
    Dictionary<string, GroundingData> documents)
{
    var labeledDocuments = new Dictionary<string, (GroundingData Data, SensitivityLabel Label)>();
    
    // Classify all documents
    foreach (var (key, doc) in documents)
    {
        var response = await _labelService.ClassifyAsync(doc);
        labeledDocuments[key] = (doc, response.Label);
    }
    
    // Group by label priority
    var documentsByPriority = labeledDocuments
        .GroupBy(kvp => kvp.Value.Label?.Priority ?? LabelPriority.Public)
        .OrderBy(g => g.Key)
        .ToDictionary(g => g.Key, g => g.ToList());
    
    // Process each priority level
    var responses = new List<string>();
    foreach (var (priority, docs) in documentsByPriority)
    {
        var allowedDocs = docs.Where(d => d.Value.Label?.Protection.PreventGrounding != true);
        if (allowedDocs.Any())
        {
            var response = await _llmService.GenerateAsync(
                query, 
                allowedDocs.Select(d => d.Value.Data).ToList());
            
            var processedResponse = await _responseProcessor.ProcessResponseAsync(
                response, 
                allowedDocs.Select(d => d.Value.Data));
            
            responses.Add(processedResponse.FormattedResponse);
        }
    }
    
    return string.Join("\n\n", responses);
}
```

### Conditional Grounding

Implement conditional grounding based on user permissions:

```csharp
public class ConditionalGroundingService
{
    private readonly IUserPermissionService _permissionService;
    private readonly ISensitivityLabelService _labelService;

    public async Task<List<GroundingData>> FilterGroundingDataAsync(
        List<GroundingData> documents, 
        string userId)
    {
        var allowedDocuments = new List<GroundingData>();
        
        foreach (var doc in documents)
        {
            var response = await _labelService.ClassifyAsync(doc);
            
            if (await CanUserAccessAsync(userId, response.Label))
            {
                allowedDocuments.Add(doc);
            }
        }
        
        return allowedDocuments;
    }

    private async Task<bool> CanUserAccessAsync(string userId, SensitivityLabel label)
    {
        if (label == null) return true;
        
        // Check user permissions
        var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);
        
        return label.Protection.AllowedUsers.Contains(userId) ||
               label.Protection.AllowedGroups.Any(group => userPermissions.Groups.Contains(group));
    }
}
```

## Testing

### Unit Testing

```csharp
[Test]
public async Task ProcessResponseAsync_WithConfidentialLabel_AddsLabelIndicator()
{
    // Arrange
    var processor = new LLMResponseProcessor(_labelService, _formatter, _settings);
    var groundingData = new List<GroundingData>
    {
        new GroundingData
        {
            Content = "Confidential information",
            Label = new SensitivityLabel
            {
                Priority = LabelPriority.Confidential,
                Name = "Confidential"
            }
        }
    };

    // Act
    var result = await processor.ProcessResponseAsync("Test response", groundingData);

    // Assert
    Assert.That(result.FormattedResponse, Does.Contain("Confidential"));
    Assert.That(result.Label.Priority, Is.EqualTo(LabelPriority.Confidential));
}
```

### Integration Testing

```csharp
[Test]
public async Task EndToEndProcessing_WithMixedLabels_FiltersCorrectly()
{
    // Arrange
    var documents = new List<GroundingData>
    {
        CreateGroundingData("Public info", LabelPriority.Public),
        CreateGroundingData("Confidential info", LabelPriority.Confidential),
        CreateGroundingData("Restricted info", LabelPriority.Restricted)
    };

    // Act
    var result = await _chatService.ProcessChatAsync("Test query", documents);

    // Assert
    Assert.That(result, Does.Not.Contain("Restricted info"));
    Assert.That(result, Does.Contain("Confidential"));
}
```

## Performance Considerations

### Caching

Implement label caching for better performance:

```csharp
public class CachedLabelService : ISensitivityLabelService
{
    private readonly ISensitivityLabelService _innerService;
    private readonly IMemoryCache _cache;

    public async Task<SensitivityLabelResponse> ClassifyAsync(GroundingData data)
    {
        var cacheKey = $"label:{data.Content.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out SensitivityLabelResponse cachedResult))
        {
            return cachedResult;
        }

        var result = await _innerService.ClassifyAsync(data);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        
        return result;
    }
}
```

### Batch Processing

Process multiple documents in batches:

```csharp
public async Task<List<SensitivityLabelResponse>> ClassifyBatchAsync(
    IEnumerable<GroundingData> documents)
{
    var semaphore = new SemaphoreSlim(10); // Limit concurrent operations
    var tasks = documents.Select(async doc =>
    {
        await semaphore.WaitAsync();
        try
        {
            return await _labelService.ClassifyAsync(doc);
        }
        finally
        {
            semaphore.Release();
        }
    });

    return (await Task.WhenAll(tasks)).ToList();
}
```

## Best Practices

1. **Always classify grounding data** before using it in LLM prompts
2. **Respect label restrictions** and never override protection settings
3. **Use appropriate response formatting** based on label priority
4. **Implement proper error handling** for label classification failures
5. **Cache label results** for frequently accessed content
6. **Log label usage** for audit and compliance purposes
7. **Test with various label combinations** to ensure proper behavior
8. **Monitor performance** and optimize batch processing when needed