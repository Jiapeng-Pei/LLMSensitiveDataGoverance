# CLI Usage Examples

This document provides comprehensive examples of using the LLM Sensitive Data Governance CLI tool.

## Installation

```bash
# Build the CLI tool
dotnet build src/LLMSensitiveDataGoverance.CLI/LLMSensitiveDataGoverance.CLI.csproj

# Install globally (optional)
dotnet tool install --global --add-source ./src/LLMSensitiveDataGoverance.CLI/bin/Debug/net8.0 LLMSensitiveDataGoverance.CLI
Basic Commands
1. List Available Labels
bash# List all available sensitivity labels
llm-governance list-labels

# List labels with detailed information
llm-governance list-labels --detailed

# List labels by priority
llm-governance list-labels --priority Confidential
Expected output:
Available Sensitivity Labels:
- Public (Priority: Public)
- Internal (Priority: Internal)
- Confidential (Priority: Confidential)
- Highly Confidential (Priority: HighlyConfidential)
- Restricted (Priority: Restricted)
2. Classify Content
bash# Classify a text string
llm-governance classify --content "Employee salary: $75,000"

# Classify content from a file
llm-governance classify --file "./sample-data.txt"

# Classify with additional metadata
llm-governance classify --content "Financial report Q4" --source "Finance DB" --data-type "application/pdf"
Expected output:
Classification Result:
Label: Confidential
Priority: Confidential
Description: Contains sensitive business information
Protection Settings:
  - Requires Encryption: true
  - Prevent Extraction: true
  - Prevent Copy/Paste: true
  - Prevent Grounding: false
3. Validate Labels
bash# Validate a specific label configuration
llm-governance validate --label-id "confidential-001"

# Validate all labels
llm-governance validate --all

# Validate with verbose output
llm-governance validate --label-id "confidential-001" --verbose
4. Configure System
bash# Configure default settings
llm-governance configure --encryption-enabled true --default-priority Internal

# Set custom label repository
llm-governance configure --repository-type Json --repository-path "./custom-labels.json"

# Configure logging level
llm-governance configure --log-level Information
Advanced Examples
Batch Processing
bash# Process multiple files in a directory
llm-governance classify --directory "./documents" --output-format json

# Process with filtering
llm-governance classify --directory "./documents" --file-pattern "*.pdf" --recursive

# Batch process with parallel execution
llm-governance classify --directory "./documents" --parallel --max-threads 4
Custom Output Formats
bash# Output as JSON
llm-governance classify --content "Sensitive data" --output-format json

# Output as XML
llm-governance classify --content "Sensitive data" --output-format xml

# Output to file
llm-governance classify --content "Sensitive data" --output-file "./results.json"
Integration with Scripts
bash#!/bin/bash
# Example shell script integration

# Classify and capture result
RESULT=$(llm-governance classify --content "$1" --output-format json)

# Parse JSON result (requires jq)
LABEL=$(echo "$RESULT" | jq -r '.label.name')
PRIORITY=$(echo "$RESULT" | jq -r '.label.priority')

echo "Content classified as: $LABEL (Priority: $PRIORITY)"

# Conditional processing based on label
if [ "$PRIORITY" == "Restricted" ]; then
    echo "Access denied - content is restricted"
    exit 1
fi
PowerShell Integration
powershell# Example PowerShell script
function Classify-Content {
    param(
        [string]$Content,
        [string]$OutputFormat = "json"
    )
    
    $result = & llm-governance classify --content $Content --output-format $OutputFormat
    
    if ($OutputFormat -eq "json") {
        $parsed = $result | ConvertFrom-Json
        return $parsed
    }
    
    return $result
}

# Usage
$classification = Classify-Content "Employee performance data"
Write-Host "Label: $($classification.label.name)"
Write-Host "Can use for grounding: $($classification.allowGrounding)"
Configuration Examples
Custom Label Configuration
json{
  "labels": [
    {
      "id": "custom-001",
      "name": "Customer Data",
      "description": "Customer personal information",
      "priority": "Confidential",
      "protection": {
        "requireEncryption": true,
        "preventExtraction": true,
        "preventCopyPaste": true,
        "preventGrounding": false
      },
      "customProperties": {
        "department": "Customer Service",
        "retentionPeriod": "7 years"
      }
    }
  ]
}
Environment Variables
bash# Set default configuration via environment variables
export LLM_GOVERNANCE_LOG_LEVEL=Information
export LLM_GOVERNANCE_REPOSITORY_TYPE=Json
export LLM_GOVERNANCE_REPOSITORY_PATH=./config/labels.json
export LLM_GOVERNANCE_ENCRYPTION_ENABLED=true
Error Handling Examples
Invalid Content
bash# This will show validation errors
llm-governance classify --content ""

# Output:
# Error: Content cannot be empty
# Use --help for usage information
Missing Configuration
bash# This will show configuration errors
llm-governance validate --label-id "non-existent"

# Output:
# Error: Label with ID 'non-existent' not found
# Available labels: public, internal, confidential, highly-confidential, restricted
Performance Testing
bash# Time classification performance
time llm-governance classify --file "./large-document.txt"

# Batch performance testing
time llm-governance classify --directory "./test-data" --parallel

# Memory usage monitoring (Linux)
/usr/bin/time -v llm-governance classify --file "./large-document.txt"
Integration with CI/CD
GitHub Actions Example
yamlname: Content Classification

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  classify:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
        
    - name: Install CLI tool
      run: dotnet tool install --global LLMSensitiveDataGoverance.CLI
      
    - name: Classify documentation
      run: |
        llm-governance classify --directory "./docs" --output-format json > classification-results.json
        
    - name: Check for restricted content
      run: |
        if jq -e '.[] | select(.label.priority == "Restricted")' classification-results.json; then
          echo "Restricted content found in documentation"
          exit 1
        fi
Azure DevOps Example
yamltrigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.0.x'

- script: |
    dotnet tool install --global LLMSensitiveDataGoverance.CLI
    llm-governance classify --directory "$(Build.SourcesDirectory)" --output-format json
  displayName: 'Classify source code content'
Troubleshooting
Common Issues

Permission Denied
bash# Fix: Run with appropriate permissions
sudo llm-governance classify --file "./protected-file.txt"

Configuration Not Found
bash# Fix: Initialize configuration
llm-governance configure --initialize

Invalid File Format
bash# Fix: Check file encoding
file --mime-encoding ./problem-file.txt
llm-governance classify --file "./problem-file.txt" --encoding utf-8


Debug Mode
bash# Enable debug logging
llm-governance classify --content "test" --log-level Debug --verbose

# Save debug information
llm-governance classify --content "test" --debug-output ./debug.log

## examples/ConfigurationSamples/appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "LLMSensitiveDataGoverance": "Debug"
    }
  },
  "LLMSensitiveDataGovernance": {
    "Repository": {
      "Type": "Json",
      "ConnectionString": "./config/labels.json",
      "CacheEnabled": true,
      "CacheExpirationMinutes": 60
    },
    "Encryption": {
      "Enabled": true,
      "Algorithm": "AES256",
      "KeyRotationDays": 90,
      "RequireEncryptionForHighPriority": true
    },
    "Agent": {
      "DefaultResponseFormat": "Html",
      "EnableVisualLabels": true,
      "EnableCopyPasteProtection": true,
      "MaxConcurrentClassifications": 10,
      "ClassificationTimeoutSeconds": 30
    },
    "Validation": {
      "EnableStrictMode": true,
      "ValidateCustomProperties": true,
      "RequireDescriptionForCustomLabels": true,
      "MaxLabelNameLength": 100,
      "MaxDescriptionLength": 500
    },
    "Performance": {
      "EnableCaching": true,
      "CacheSize": 1000,
      "EnableMetrics": true,
      "MetricsCollectionInterval": 300
    }
  }
}