# LLM Sensitive Data Governance

A C# system for managing sensitivity labels for LLM grounding data, designed to provide comprehensive data protection and governance for AI Agent development.

## Features

- **Privacy-compliant sensitivity labels** with encryption and access controls
- **Visual label indicators** in AI responses
- **Copy/paste protection** with label preservation
- **Grounding data protection** based on sensitivity levels
- **Configurable label hierarchy** (Public → Internal → Confidential → Highly Confidential → Restricted)
- **Local standalone operation** with no network dependencies
- **Agent integration** for seamless AI development workflow 

## Quick Start

### Installation

```bash
git clone https://github.com/your-org/LLMSensitiveDataGoverance.git
cd LLMSensitiveDataGoverance
dotnet build
```

### Basic Usage

```csharp
// Initialize the service
var services = new ServiceCollection();
services.AddSensitivityLabelServices();
var provider = services.BuildServiceProvider();

var labelService = provider.GetRequiredService<ISensitivityLabelService>();

// Classify grounding data
var groundingData = new GroundingData
{
    Content = "Company financial data",
    Source = "quarterly-report.pdf"
};

var response = await labelService.ClassifyAsync(groundingData);
Console.WriteLine($"Label: {response.Label.Name}");
Console.WriteLine($"Can use for grounding: {response.AllowGrounding}");
```

### CLI Usage

```bash
# Classify content
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- classify --content "sensitive data"

# List available labels
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- list-labels

# Validate configuration
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- validate
```

## Architecture

The system consists of four main components:

1. **Core Library** (`LLMSensitiveDataGoverance.Core`) - Core models, services, and interfaces
2. **CLI Application** (`LLMSensitiveDataGoverance.CLI`) - Command-line interface
3. **Agent Integration** (`LLMSensitiveDataGoverance.AgentIntegration`) - AI Agent integration utilities
4. **Configuration** - JSON-based label and system configuration

## Configuration

### Labels Configuration (`config/labels.json`)

Define custom sensitivity labels with protection settings:

```json
{
  "labels": [
    {
      "id": "confidential",
      "name": "Confidential",
      "priority": 2,
      "protection": {
        "requireEncryption": true,
        "preventExtraction": true,
        "preventGrounding": false
      }
    }
  ]
}
```

### Application Settings (`config/appsettings.json`)

Configure system behavior:

```json
{
  "SensitivityLabels": {
    "DefaultProvider": "InMemory",
    "EnableEncryption": true,
    "ValidateOnStartup": true
  }
}
```

## AI Agent Integration

Integrate with your AI Agent projects:

```csharp
services.AddSensitivityLabelServices();
services.AddAgentIntegration();

// Use in your agent
var processor = serviceProvider.GetRequiredService<LLMResponseProcessor>();
var processedResponse = await processor.ProcessResponseAsync(llmResponse, groundingData);
```

## Documentation

- [API Reference](docs/api-reference.md)
- [Configuration Guide](docs/configuration.md)
- [Agent Integration Guide](docs/agent-integration.md)
- [CLI Reference](docs/cli-reference.md)

## Examples

See the [examples](examples/) directory for:
- Basic usage examples
- Agent integration samples
- Custom label configurations
- Performance optimization examples

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues and questions:
- Create an issue in the GitHub repository
- Check the [documentation](docs/)
- Review the [examples](examples/)

## Requirements

- .NET 8.0 or later
- No external dependencies required for core functionality
- Optional: JSON configuration files for custom labels
