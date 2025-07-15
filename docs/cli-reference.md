# CLI Reference

## Overview

The LLM Sensitive Data Governance CLI provides command-line tools for managing sensitivity labels, classifying content, and validating configurations.

## Installation

```bash
dotnet build src/LLMSensitiveDataGoverance.CLI
```

## Usage

```bash
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- [command] [options]
```

## Commands

### classify

Classifies content and assigns appropriate sensitivity labels.

#### Syntax

```bash
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- classify [options]
```

#### Options

- `--content, -c <content>`: Content to classify (required)
- `--source, -s <source>`: Source identifier (optional)
- `--data-type, -t <type>`: Data type (optional)
- `--format, -f <format>`: Output format (json, text, table) [default: table]
- `--output, -o <file>`: Output file path (optional)
- `--verbose, -v`: Enable verbose output

#### Examples

```bash
# Classify text content
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- classify --content "Company financial data"

# Classify with source information
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- classify \
  --content "Employee salary information" \
  --source "hr-database.xlsx" \
  --data-type "spreadsheet"

# Output as JSON
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- classify \
  --content "Customer data" \
  --format json

# Save output to file
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- classify \
  --content "Trade secrets" \
  --output classification-result.json \
  --format json
```

#### Sample Output

```
Classification Results
=====================

Content: Company financial data
Source: quarterly-report.pdf
Data Type: document

Assigned Label: Confidential
Priority: 2
Description: Sensitive information requiring protection

Protection Settings:
- Encryption Required: Yes
- Prevent Extraction: Yes
- Prevent Copy/Paste: No
- Prevent Grounding: No

Allowed Groups: managers, authorized-users
```

### list-labels

Lists all available sensitivity labels.

#### Syntax

```bash
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- list-labels [options]
```

#### Options

- `--priority, -p <priority>`: Filter by priority level (0-4)
- `--active-only, -a`: Show only active labels
- `--format, -f <format>`: Output format (json, text, table) [default: table]
- `--output, -o <file>`: Output file path (optional)
- `--sort, -s <field>`: Sort by field (id, name, priority) [default: priority]

#### Examples

```bash
# List all labels
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- list-labels

# List only confidential labels
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- list-labels --priority 2

# List active labels only
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- list-labels --active-only

# Export as JSON
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- list-labels \
  --format json \
  --output labels.json
```

#### Sample Output

```
Available Sensitivity Labels
============================

ID: public
Name: Public
Priority: 0 (Public)
Description: Information that can be shared publicly without restrictions
Status: Active
Protection: None

ID: internal
Name: Internal
Priority: 1 (Internal)
Description: Information for internal use within the organization
Status: Active
Protection: Group restrictions

ID: confidential
Name: Confidential
Priority: 2 (Confidential)
Description: Sensitive information requiring protection
Status: Active
Protection: Encryption, Extraction prevention

ID: highly-confidential
Name: Highly Confidential
Priority: 3 (Highly Confidential)
Description: Highly sensitive information with strict access controls
Status: Active
Protection: Full protection enabled

ID: restricted
Name: Restricted
Priority: 4 (Restricted)
Description: Top secret information with maximum protection
Status: Active
Protection: Maximum security
```

### validate

Validates system configuration and label definitions.

#### Syntax

```bash
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- validate [options]
```

#### Options

- `--config, -c <path>`: Configuration file path [default: config/appsettings.json]
- `--labels, -l <path>`: Labels file path [default: config/labels.json]
- `--fix, -f`: Attempt to fix validation issues
- `--verbose, -v`: Enable verbose output
- `--output, -o <file>`: Output file path (optional)

#### Examples

```bash
# Validate default configuration
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- validate

# Validate specific configuration files
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- validate \
  --config custom-appsettings.json \
  --labels custom-labels.json

# Validate with fix attempt
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- validate --fix

# Verbose validation
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- validate --verbose
```

#### Sample Output

```
Configuration Validation Results
================================

✓ Configuration file format is valid
✓ All required settings are present
✓ Label definitions are valid
✓ Label IDs are unique
✓ Priority levels are within valid range
✓ Protection settings are valid
✓ Custom properties are properly formatted

Validation Summary:
- Total labels: 5
- Active labels: 5
- Inactive labels: 0
- Errors: 0
- Warnings: 0

Configuration Status: VALID
```

### configure

Configures system settings and label definitions.

#### Syntax

```bash
dotnet run --project src/LLMSensitiveDataGoverance.CLI -- configure [options]
```

#### Options

- `--interactive, -i`: Interactive configuration mode
- `--set <key=value>`: Set configuration value
- `--add-