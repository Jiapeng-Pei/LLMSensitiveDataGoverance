# LLM Sensitive Data Governance CLI

A command-line interface for managing sensitivity labels in LLM grounding data systems.

## Installation

### Prerequisites
- .NET 8.0 SDK or later
- Windows, macOS, or Linux

### Install as Global Tool
```bash
dotnet pack
dotnet tool install --global --add-source ./nupkg llm-sensitivity-cli
```

### Local Build
```bash
dotnet build
dotnet run -- [command] [options]
```

## Usage

### Basic Commands

#### Classify Grounding Data
```bash
# Classify content from file
llm-sensitivity-cli classify --file data.txt --source "documents" --data-type text

# Classify direct content
llm-sensitivity-cli classify --content "Confidential employee information" --source "hr-system"

# Output to file with JSON format
llm-sensitivity-cli classify --file data.txt --source "documents" --output result.json --format json
```

#### List Sensitivity Labels
```bash
# List all labels
llm-sensitivity-cli list

# List with details
llm-sensitivity-cli list --details

# Filter by priority
llm-sensitivity-cli list --priority Confidential --format json

# Show only active labels
llm-sensitivity-cli list --active-only
```

#### Validate Labels
```bash
# Validate all labels
llm-sensitivity-cli validate --all

# Validate specific label
llm-sensitivity-cli validate --label-name "Confidential"

# Detailed validation results
llm-sensitivity-cli validate --all --detailed --format json
```

#### Configure Labels
```bash
# Add new label
llm-sensitivity-cli configure add \
  --name "HighlyConfidential" \
  --description "Highly sensitive company data" \
  --priority HighlyConfidential \
  --encryption \
  --prevent-grounding

# Update existing label
llm-sensitivity-cli configure update \
  --id "label-123" \
  --name "UpdatedName" \
  --active true

# Delete label
llm-sensitivity-cli configure delete --id "label-123" --force

# Import labels from JSON
llm-sensitivity-cli configure import --file labels.json --overwrite

# Export labels to JSON
llm-sensitivity-cli configure export --file exported-labels.json
```

### Options

#### Global Options
- `--help`, `-h`: Show help information
- `--version`: Show version information

#### Output Options
- `--format`: Output format (table, json, xml)
- `--output`, `-o`: Output file path
- `--verbose`, `-v`: Enable verbose output

#### Classification Options
- `--file`, `-f`: Input file containing grounding data
- `--content`, `-c`: Direct content to classify
- `--source`, `-s`: Source identifier for the data
- `--data-type`, `-t`: Type of data (text, document, etc.)

#### Label Management Options
- `--priority`, `-p`: Filter by label priority
- `--active-only`, `-a`: Show only active labels
- `--details`, `-d`: Show detailed information
- `--force`: Force operation without confirmation

### Examples

#### Example 1: Basic Classification Workflow
```bash
# 1. List available labels
llm-sensitivity-cli list

# 2. Classify a document
llm-sensitivity-cli classify \
  --file "sensitive-doc.txt" \
  --source "document-management" \
  --data-type "document" \
  --verbose

# 3. Export results
llm-sensitivity-cli classify \
  --file "sensitive-doc.txt" \
  --source "document-management" \
  --output "classification-result.json" \
  --format json
```

#### Example 2: Label Management
```bash
# 1. Create a new sensitivity label
llm-sensitivity-cli configure add \
  --name "CustomerPII" \
  --description "Customer personally identifiable information" \
  --priority Confidential \
  --encryption \
  --prevent-copy-paste

# 2. Validate the new label
llm-sensitivity-cli validate --label-name "CustomerPII" --detailed

# 3. Export all labels for backup
llm-sensitivity-cli configure export --file "labels-backup.json"
```

#### Example 3: Batch Processing
```bash
# 1. Create a script for multiple files
for file in ./data/*.txt; do
  llm-sensitivity-cli classify \
    --file "$file" \
    --source "batch-processing" \
    --output "./results/$(basename "$file" .txt)-result.json" \
    --format json
done

# 2. Validate all labels after updates
llm-sensitivity-cli validate --all --format xml --output validation-report.xml
```

## Configuration

### Configuration Files
- `appsettings.json`: Main configuration
- `appsettings.Development.json`: Development overrides
- `config/labels.json`: Label definitions

### Environment Variables
- `LLMSDG_LOG_LEVEL`: Override logging level
- `LLMSDG_CONFIG_PATH`: Override configuration file path
- `LLMSDG_LABELS_PATH`: Override labels file path

### Label JSON Format
```json
[
  {
    "id": "confidential-001",
    "name": "Confidential",
    "description": "Confidential company information",
    "priority": 2,
    "protection": {
      "requireEncryption": true,
      "preventExtraction": true,
      "preventCopyPaste": false,
      "preventGrounding": false,
      "allowedUsers": [],
      "allowedGroups": []
    },
    "customProperties": {},
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z",
    "isActive": true
  }
]
```

## Output Formats

### Table Format (Default)
Human-readable tabular output suitable for console viewing.

### JSON Format
Machine-readable JSON output suitable for automation and integration.

### XML Format
Structured XML output for systems requiring XML input.

## Error Handling

The CLI provides comprehensive error handling with:
- Descriptive error messages
- Exit codes for automation
- Stack traces in development mode
- Logging to console and files

### Exit Codes
- `0`: Success
- `1`: General error
- `2`: Invalid arguments
- `3`: File not found
- `4`: Permission denied
- `5`: Validation error

## Integration

### PowerShell Integration
```powershell
# Classification with error handling
$result = llm-sensitivity-cli classify --file "data.txt" --source "ps-script" --format json
if ($LASTEXITCODE -eq 0) {
    $classification = $result | ConvertFrom-Json
    Write-Host "Classification: $($classification.label)"
}
```

### Bash Integration
```bash
#!/bin/bash
# Batch classification script
for file in ./documents/*.txt; do
    if llm-sensitivity-cli classify --file "$file" --source "batch" > /dev/null; then
        echo "✓ Classified: $file"
    else
        echo "✗ Failed: $file"
    fi
done
```

## Troubleshooting

### Common Issues

1. **File Permission Errors**
   - Ensure read/write permissions for input/output files
   - Run with appropriate user privileges

2. **Configuration Errors**
   - Verify JSON syntax in configuration files
   - Check file paths and permissions

3. **Label Validation Failures**
   - Review label definitions against validation rules
   - Check for duplicate names or invalid priorities

### Debug Mode
```bash
# Enable detailed logging
export LLMSDG_LOG_LEVEL=Debug
llm-sensitivity-cli [command] --verbose
```

### Support
For issues and support, please refer to the project documentation or create an issue in the project repository. ==========================================
