# LLM Sensitive Data Governance - Agent Integration

This module provides integration components for AI agents to work with sensitivity-labeled data while maintaining security and compliance requirements.

## Features

- **AgentLabelProvider**: Manages sensitivity labels for AI agents
- **LLMResponseProcessor**: Processes LLM responses with sensitivity labels
- **GroundingDataProcessor**: Handles grounding data processing and sanitization
- **Configuration**: Comprehensive settings for labels and agent behavior
- **Extensions**: Utility methods for DI and string manipulation

## Quick Start

```csharp
// Configure services
services.AddSensitivityLabelServices(configuration);

// Use in agent
var labelProvider = serviceProvider.GetRequiredService<AgentLabelProvider>();
var responseProcessor = serviceProvider.GetRequiredService<LLMResponseProcessor>();

// Process grounding data
var usableData = await labelProvider.FilterUsableGroundingDataAsync(groundingData);

// Process LLM response
var processedResponse = await responseProcessor.ProcessResponseAsync(response, label);
```
