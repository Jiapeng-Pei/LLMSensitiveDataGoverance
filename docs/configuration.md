# Configuration Guide

## Overview

The LLM Sensitive Data Governance system uses JSON-based configuration to customize label behavior, security settings, and integration options.

## Configuration Files

### 1. Application Settings (`config/appsettings.json`)

Main system configuration file.

#### SensitivityLabels Section

```json
{
  "SensitivityLabels": {
    "DefaultProvider": "InMemory",
    "ConfigurationPath": "config/labels.json",
    "EnableEncryption": true,
    "EncryptionKey": "your-encryption-key-here",
    "CacheEnabled": true,
    "CacheExpirationMinutes": 30,
    "MaxLabelPriority": 4,
    "AllowCustomLabels": true,
    "ValidateOnStartup": true
  }
}
```

**Properties:**
- `DefaultProvider`: Label storage provider (`InMemory`, `Json`, `Configurable`)
- `ConfigurationPath`: Path to labels configuration file
- `EnableEncryption`: Enable/disable encryption features
- `EncryptionKey`: Encryption key for protected content
- `CacheEnabled`: Enable label caching for performance
- `CacheExpirationMinutes`: Cache expiration time
- `MaxLabelPriority`: Maximum allowed label priority
- `AllowCustomLabels`: Allow creation of custom labels
- `ValidateOnStartup`: Validate configuration on startup

#### Performance Section

```json
{
  "Performance": {
    "MaxConcurrentOperations": 10,
    "DefaultTimeoutMs": 5000,
    "EnableMetrics": true,
    "MetricsOutputPath": "logs/metrics.json"
  }
}
```

**Properties:**
- `MaxConcurrentOperations`: Maximum concurrent label operations
- `DefaultTimeoutMs`: Default operation timeout
- `EnableMetrics`: Enable performance metrics collection
- `MetricsOutputPath`: Path for metrics output

#### Security Section

```json
{
  "Security": {
    "RequireAuthentication": false,
    "AllowedHosts": ["localhost"],
    "EnableAuditLog": true,
    "AuditLogPath": "logs/audit.log"
  }
}
```

**Properties:**
- `RequireAuthentication`: Require authentication for operations
- `AllowedHosts`: Allowed host names
- `EnableAuditLog`: Enable audit logging
- `AuditLogPath`: Path for audit log file

#### AgentIntegration Section

```json
{
  "AgentIntegration": {
    "DefaultResponseFormat": "Html",
    "IncludeLabelInResponse": true,
    "PreventGroundingOnRestricted": true,
    "ShowVisualIndicators": true,
    "CopyPasteWarnings": true
  }
}
```

**Properties:**
- `DefaultResponseFormat`: Default response format (`Html`, `Markdown`, `Text`)
- `IncludeLabelInResponse`: Include label information in responses
- `PreventGroundingOnRestricted`: Prevent grounding on restricted labels
- `ShowVisualIndicators`: Show visual sensitivity indicators
- `CopyPasteWarnings`: Show warnings on copy/paste operations

### 2. Labels Configuration (`config/labels.json`)

Defines available sensitivity labels and their properties.

#### Label Structure

```json
{
  "id": "confidential",
  "name": "Confidential",
  "description": "Sensitive information requiring protection",
  "priority": 2,
  "protection": {
    "requireEncryption": true,
    "preventExtraction": true,
    "preventCopyPaste": false,
    "preventGrounding": false,
    "allowedUsers": [],
    "allowedGroups": ["managers", "authorized-users"]
  },
  "customProperties": {
    "color": "#ffc107",
    "icon": "shield",
    "displayText": "Confidential"
  },
  "isActive": true
}
```

**Properties:**
- `id`: Unique label identifier
- `name`: Display name
- `description`: Label description
- `priority`: Priority level (0-4)
- `protection`: Protection settings object
- `customProperties`: Custom display properties
- `isActive`: Whether label is active

#### Protection Settings

- `requireEncryption`: Encrypt content with this label
- `preventExtraction`: Prevent data extraction
- `preventCopyPaste`: Prevent copy/paste operations
- `preventGrounding`: Prevent use in LLM grounding
- `allowedUsers`: List of allowed user IDs
- `allowedGroups`: List of allowed group names

#### Custom Properties

- `color`: Hex color code for visual display
- `icon`: Icon name for visual indicators
- `displayText`: Text shown in UI

## Environment-Specific Configuration

### Development Environment

```json
{
  "SensitivityLabels": {
    "DefaultProvider": "InMemory",
    "EnableEncryption": false,
    "ValidateOnStartup": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Production Environment

```json
{
  "SensitivityLabels": {
    "DefaultProvider": "Json",
    "EnableEncryption": true,
    "EncryptionKey": "${ENCRYPTION_KEY}",
    "ValidateOnStartup": true
  },
  "Security": {
    "RequireAuthentication": true,
    "EnableAuditLog": true
  }
}
```

## Custom Label Creation

### Creating Custom Labels

1. **Add to labels.json:**

```json
{
  "id": "custom-restricted",
  "name": "Custom Restricted",
  "description": "Custom restriction level",
  "priority": 3,
  "protection": {
    "requireEncryption": true,
    "preventExtraction": true,
    "preventCopyPaste": true,
    "preventGrounding": true,
    "allowedUsers": ["admin"],
    "allowedGroups": ["executives"]
  },
  "customProperties": {
    "color": "#8b0000",
    "icon": "ban",
    "displayText": "Custom Restricted"
  },
  "isActive": true
}
```

2. **Or create programmatically:**

```csharp
var customLabel = new SensitivityLabel
{
    Id = "custom-restricted",
    Name = "Custom Restricted",
    Description = "Custom restriction level",
    Priority = LabelPriority.HighlyConfidential,
    Protection = new ProtectionSettings
    {
        RequireEncryption = true,
        PreventExtraction = true,
        PreventCopyPaste = true,
        PreventGrounding = true,
        AllowedUsers = new List<string> { "admin" },
        AllowedGroups = new List<string> { "executives" }
    },
    CustomProperties = new Dictionary<string, string>
    {
        ["color"] = "#8b0000",
        ["icon"] = "ban",
        ["displayText"] = "Custom Restricted"
    },
    IsActive = true
};

await labelRepository.CreateAsync(customLabel);
```

## Validation

### Configuration Validation

The system validates configuration on startup:

- Label IDs must be unique
- Priority levels must be within valid range (0-4)
- Protection settings must be valid
- Custom properties must be properly formatted

### Manual Validation

```csharp
var validator = serviceProvider.GetRequiredService<ILabelValidator>();
var validationResult = await validator.ValidateDetailedAsync(label);

if (!validationResult.IsValid)
{
    Console.WriteLine($"Validation failed: {validationResult.ErrorMessage}");
}
```

## Best Practices

1. **Use descriptive label names** that clearly indicate the sensitivity level
2. **Set appropriate priority levels** to ensure proper label hierarchy
3. **Configure protection settings** based on your security requirements
4. **Use custom properties** for consistent visual representation
5. **Enable encryption** for production environments
6. **Validate configuration** before deployment
7. **Monitor audit logs** for security compliance
8. **Regular backup** of configuration files

## Troubleshooting

### Common Issues

1. **Invalid configuration format**
   - Check JSON syntax
   - Verify required properties are present

2. **Label validation errors**
   - Ensure unique IDs
   - Check priority range (0-4)
   - Verify protection settings

3. **Encryption issues**
   - Verify encryption key is set
   - Check key format and length

4. **Performance issues**
   - Adjust MaxConcurrentOperations
   - Enable caching
   - Optimize label configuration

### Debug Configuration

Enable debug logging to troubleshoot configuration issues:

```json
{
  "Logging": {
    "LogLevel": {
      "LLMSensitiveDataGoverance": "Debug"
    }
  }
}
```