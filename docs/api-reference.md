# API Reference

## Core Interfaces

### ISensitivityLabelService

Main service interface for sensitivity label operations.

#### Methods

##### `ClassifyAsync(GroundingData data)`
Classifies grounding data and returns appropriate sensitivity label.

**Parameters:**
- `data`: GroundingData object containing content and metadata

**Returns:** `Task<SensitivityLabelResponse>`

**Example:**
```csharp
var response = await labelService.ClassifyAsync(groundingData);
```

##### `ProcessLLMResponseAsync(string response, SensitivityLabel label)`
Processes LLM response with sensitivity label formatting.

**Parameters:**
- `response`: Raw LLM response text
- `label`: Associated sensitivity label

**Returns:** `Task<SensitivityLabelResponse>`

##### `GetHighestPriorityLabelAsync(IEnumerable<SensitivityLabel> labels)`
Returns the label with highest priority from a collection.

**Parameters:**
- `labels`: Collection of sensitivity labels

**Returns:** `Task<SensitivityLabel>`

### ILabelRepository

Repository interface for label persistence operations.

#### Methods

##### `GetByIdAsync(string id)`
Retrieves label by unique identifier.

##### `GetByNameAsync(string name)`
Retrieves label by name.

##### `GetAllAsync()`
Retrieves all available labels.

##### `CreateAsync(SensitivityLabel label)`
Creates new sensitivity label.

##### `UpdateAsync(SensitivityLabel label)`
Updates existing sensitivity label.

##### `DeleteAsync(string id)`
Deletes label by identifier.

### IEncryptionService

Service interface for encryption operations.

#### Methods

##### `EncryptAsync(string content, SensitivityLabel label)`
Encrypts content based on label protection settings.

##### `DecryptAsync(string encryptedContent, SensitivityLabel label)`
Decrypts content using label-specific decryption.

##### `CanDecryptAsync(string encryptedContent, SensitivityLabel label)`
Checks if content can be decrypted with given label.

##### `ShouldEncryptAsync(SensitivityLabel label)`
Determines if content should be encrypted based on label.

## Core Models

### SensitivityLabel

Represents a sensitivity label with protection settings.

#### Properties

- `Id`: Unique identifier
- `Name`: Display name
- `Description`: Label description
- `Priority`: Label priority level (LabelPriority enum)
- `Protection`: Protection settings (ProtectionSettings)
- `CustomProperties`: Custom key-value properties
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last update timestamp
- `IsActive`: Whether label is active

### GroundingData

Represents data used for LLM grounding.

#### Properties

- `Id`: Unique identifier
- `Content`: Data content
- `Source`: Data source identifier
- `DataType`: Type of data
- `Label`: Associated sensitivity label
- `Metadata`: Additional metadata
- `LastModified`: Last modification timestamp

### ProtectionSettings

Defines protection behavior for sensitivity labels.

#### Properties

- `RequireEncryption`: Whether encryption is required
- `PreventExtraction`: Whether to prevent data extraction
- `PreventCopyPaste`: Whether to prevent copy/paste operations
- `PreventGrounding`: Whether to prevent use in grounding
- `AllowedUsers`: List of allowed users
- `AllowedGroups`: List of allowed groups

### SensitivityLabelResponse

Response object containing processed label information.

#### Properties

- `Label`: Associated sensitivity label
- `Content`: Processed content
- `FormattedResponse`: Formatted response with label indicators
- `ShouldDisplay`: Whether response should be displayed
- `AllowCopyPaste`: Whether copy/paste is allowed
- `AllowGrounding`: Whether content can be used for grounding

## Enums

### LabelPriority

Defines priority levels for sensitivity labels.

- `Public = 0`: Public information
- `Internal = 1`: Internal use only
- `Confidential = 2`: Confidential information
- `HighlyConfidential = 3`: Highly confidential information
- `Restricted = 4`: Restricted access information

## Exceptions

### SensitivityLabelException

Base exception for sensitivity label operations.

#### Properties
- `LabelId`: Associated label identifier

### InvalidLabelException

Exception for invalid label operations.

#### Properties
- `ValidationResult`: Validation result details

### EncryptionException

Exception for encryption/decryption operations.

#### Properties
- `Operation`: Failed operation name

## Extension Methods

### ServiceCollectionExtensions

#### `AddSensitivityLabelServices()`
Registers all sensitivity label services with dependency injection.

#### `AddAgentIntegration()`
Registers agent integration services.

### StringExtensions

#### `ToSensitivityLabel()`
Converts string to sensitivity label format.

#### `HasSensitivityLabel()`
Checks if string contains sensitivity label markers.

## Configuration

### LabelConfiguration

Configuration options for label system.

#### Properties
- `DefaultProvider`: Default label provider type
- `ConfigurationPath`: Path to label configuration file
- `EnableEncryption`: Whether encryption is enabled
- `ValidateOnStartup`: Whether to validate on startup

### AgentSettings

Configuration for agent integration.

#### Properties
- `DefaultResponseFormat`: Default response format
- `IncludeLabelInResponse`: Whether to include labels in responses
- `PreventGroundingOnRestricted`: Whether to prevent grounding on restricted labels
- `ShowVisualIndicators`: Whether to show visual indicators