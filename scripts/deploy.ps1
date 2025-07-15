#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Deployment script for LLM Sensitive Data Governance system
.DESCRIPTION
    Deploys the system to target environments with configuration validation
.PARAMETER Environment
    Target environment (Development, Staging, Production)
.PARAMETER DeploymentPath
    Target deployment directory
.PARAMETER ConfigPath
    Configuration file path for the environment
.PARAMETER SkipValidation
    Skip configuration validation
.PARAMETER BackupExisting
    Backup existing deployment before updating
#>

param(
    [Parameter(Mandatory)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment,
    
    [Parameter(Mandatory)]
    [string]$DeploymentPath,
    
    [Parameter()]
    [string]$ConfigPath,
    
    [Parameter()]
    [switch]$SkipValidation,
    
    [Parameter()]
    [switch]$BackupExisting
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Deployment configuration
$DeploymentConfig = @{
    Development = @{
        RequiredFiles = @("config/appsettings.json", "config/labels.json")
        ValidationEnabled = $true
        BackupRetention = 3
    }
    Staging = @{
        RequiredFiles = @("config/appsettings.json", "config/labels.json")
        ValidationEnabled = $true
        BackupRetention = 7
    }
    Production = @{
        RequiredFiles = @("config/appsettings.json", "config/labels.json")
        ValidationEnabled = $true
        BackupRetention = 30
    }
}

$Config = $DeploymentConfig[$Environment]
$DeploymentId = Get-Date -Format "yyyyMMdd-HHmmss"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "LLM Sensitive Data Governance Deploy" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Green
Write-Host "Deployment Path: $DeploymentPath" -ForegroundColor Green
Write-Host "Deployment ID: $DeploymentId" -ForegroundColor Green
Write-Host ""

# Step 1: Pre-deployment validation
Write-Host "Step 1: Pre-deployment validation..." -ForegroundColor Yellow

# Check if artifacts directory exists
$ArtifactsPath = "artifacts"
if (-not (Test-Path $ArtifactsPath)) {
    Write-Host "✗ Artifacts directory not found. Please run build script first." -ForegroundColor Red
    exit 1
}

# Check required files
foreach ($file in $Config.RequiredFiles) {
    if (-not (Test-Path $file)) {
        Write-Host "✗ Required file not found: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✓ Pre-deployment validation completed" -ForegroundColor Green

# Step 2: Configuration validation
if (-not $SkipValidation -and $Config.ValidationEnabled) {
    Write-Host "Step 2: Configuration validation..." -ForegroundColor Yellow
    
    try {
        $CLIPath = "$ArtifactsPath/cli"
        if (-not (Test-Path "$CLIPath/LLMSensitiveDataGoverance.CLI.exe") -and 
            -not (Test-Path "$CLIPath/LLMSensitiveDataGoverance.CLI.dll")) {
            Write-Host "✗ CLI not found in artifacts. Please build first." -ForegroundColor Red
            exit 1
        }
        
        # Run validation
        $ValidationArgs = @("validate", "--quiet")
        if ($ConfigPath) {
            $ValidationArgs += @("--config", $ConfigPath)
        }
        
        if (Test-Path "$CLIPath/LLMSensitiveDataGoverance.CLI.exe") {
            & "$CLIPath/LLMSensitiveDataGoverance.CLI.exe" $ValidationArgs
        } else {
            dotnet "$CLIPath/LLMSensitiveDataGoverance.CLI.dll" $ValidationArgs
        }
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Configuration validation failed" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "✓ Configuration validation completed" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Configuration validation failed: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "Step 2: Skipping configuration validation..." -ForegroundColor Yellow
}

# Step 3: Backup existing deployment
if ($BackupExisting -and (Test-Path $DeploymentPath)) {
    Write-Host "Step 3: Backing up existing deployment..." -ForegroundColor Yellow
    
    try {
        $BackupPath = "$DeploymentPath.backup.$DeploymentId"
        Copy-Item -Path $DeploymentPath -Destination $BackupPath -Recurse -Force
        Write-Host "✓ Backup created: $BackupPath" -ForegroundColor Green
        
        # Clean old backups
        $BackupPattern = "$DeploymentPath.backup.*"
        $ExistingBackups = Get-ChildItem -Path (Split-Path $DeploymentPath -Parent) -Directory | 
                          Where-Object { $_.Name -like (Split-Path $BackupPattern -Leaf) } |
                          Sort-Object CreationTime -Descending
        
        if ($ExistingBackups.Count -gt $Config.BackupRetention) {
            $BackupsToDelete = $ExistingBackups | Select-Object -Skip $Config.BackupRetention
            foreach ($backup in $BackupsToDelete) {
                Remove-Item -Path $backup.FullName -Recurse -Force
                Write-Host "  Removed old backup: $($backup.Name)" -ForegroundColor Gray
            }
        }
    }
    catch {
        Write-Host "✗ Backup failed: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "Step 3: Skipping backup..." -ForegroundColor Yellow
}

# Step 4: Deploy files
Write-Host "Step 4: Deploying files..." -ForegroundColor Yellow

try {
    # Create deployment directory
    if (-not (Test-Path $DeploymentPath)) {
        New-Item -ItemType Directory -Path $DeploymentPath -Force | Out-Null
    }
    
    # Copy CLI application
    $CLISourcePath = "$ArtifactsPath/cli"
    $CLITargetPath = "$DeploymentPath/cli"
    
    if (Test-Path $CLITargetPath) {
        Remove-Item -Path $CLITargetPath -Recurse -Force
    }
    
    Copy-Item -Path $CLISourcePath -Destination $CLITargetPath -Recurse -Force
    Write-Host "  ✓ CLI application deployed" -ForegroundColor Green
    
    # Copy configuration files
    $ConfigTargetPath = "$DeploymentPath/config"
    if (Test-Path $ConfigTargetPath) {
        Remove-Item -Path $ConfigTargetPath -Recurse -Force
    }
    
    Copy-Item -Path "config" -Destination $ConfigTargetPath -Recurse -Force
    Write-Host "  ✓ Configuration files deployed" -ForegroundColor Green
    
    # Copy documentation
    $DocsTargetPath = "$DeploymentPath/docs"
    if (Test-Path $DocsTargetPath) {
        Remove-Item -Path $DocsTargetPath -Recurse -Force
    }
    
    Copy-Item -Path "docs" -Destination $DocsTargetPath -Recurse -Force
    Write-Host "  ✓ Documentation deployed" -ForegroundColor Green
    
    # Copy examples
    $ExamplesTargetPath = "$DeploymentPath/examples"
    if (Test-Path $ExamplesTargetPath) {
        Remove-Item -Path $ExamplesTargetPath -Recurse -Force
    }
    
    Copy-Item -Path "examples" -Destination $ExamplesTargetPath -Recurse -Force
    Write-Host "  ✓ Examples deployed" -ForegroundColor Green
    
    # Copy NuGet packages if available
    $PackageFiles = Get-ChildItem -Path $ArtifactsPath -Filter "*.nupkg" -ErrorAction SilentlyContinue
    if ($PackageFiles) {
        $PackagesTargetPath = "$DeploymentPath/packages"
        if (-not (Test-Path $PackagesTargetPath)) {
            New-Item -ItemType Directory -Path $PackagesTargetPath -Force | Out-Null
        }
        
        foreach ($package in $PackageFiles) {
            Copy-Item -Path $package.FullName -Destination $PackagesTargetPath -Force
        }
        Write-Host "  ✓ NuGet packages deployed" -ForegroundColor Green
    }
    
    # Copy additional files
    $AdditionalFiles = @("README.md", "LICENSE")
    foreach ($file in $AdditionalFiles) {
        if (Test-Path $file) {
            Copy-Item -Path $file -Destination $DeploymentPath -Force
        }
    }
    
    Write-Host "✓ File deployment completed" -ForegroundColor Green
}
catch {
    Write-Host "✗ File deployment failed: $_" -ForegroundColor Red
    exit 1
}

# Step 5: Post-deployment validation
Write-Host "Step 5: Post-deployment validation..." -ForegroundColor Yellow

try {
    # Verify CLI is executable
    $CLIExecutable = if (Test-Path "$DeploymentPath/cli/LLMSensitiveDataGoverance.CLI.exe") {
        "$DeploymentPath/cli/LLMSensitiveDataGoverance.CLI.exe"
    } else {
        "$DeploymentPath/cli/LLMSensitiveDataGoverance.CLI.dll"
    }
    
    if ($CLIExecutable.EndsWith(".exe")) {
        & $CLIExecutable --version | Out-Null
    } else {
        dotnet $CLIExecutable --version | Out-Null
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ CLI validation failed" -ForegroundColor Red
        exit 1
    }
    
    # Verify configuration files
    $ConfigFiles = @("$DeploymentPath/config/appsettings.json", "$DeploymentPath/config/labels.json")
    foreach ($configFile in $ConfigFiles) {
        if (-not (Test-Path $configFile)) {
            Write-Host "✗ Configuration file missing: $configFile" -ForegroundColor Red
            exit 1
        }
        
        # Test JSON parsing
        try {
            Get-Content $configFile | ConvertFrom-Json | Out-Null
        }
        catch {
            Write-Host "✗ Invalid JSON in configuration file: $configFile" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "✓ Post-deployment validation completed" -ForegroundColor Green
}
catch {
    Write-Host "✗ Post-deployment validation failed: $_" -ForegroundColor Red
    exit 1
}

# Step 6: Generate deployment report
Write-Host "Step 6: Generating deployment report..." -ForegroundColor Yellow

try {
    $DeploymentReport = @{
        DeploymentId = $DeploymentId
        Environment = $Environment
        DeploymentPath = $DeploymentPath
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        BackupCreated = $BackupExisting.IsPresent
        ValidationSkipped = $SkipValidation.IsPresent
        DeployedFiles = @()
        Status = "Success"
    }
    
    # Get list of deployed files
    $DeployedFiles = Get-ChildItem -Path $DeploymentPath -Recurse -File | 
                    ForEach-Object { $_.FullName.Replace($DeploymentPath, "").TrimStart("\", "/") }
    $DeploymentReport.DeployedFiles = $DeployedFiles
    
    $ReportPath = "$DeploymentPath/deployment-report.json"
    $DeploymentReport | ConvertTo-Json -Depth 3 | Out-File -FilePath $ReportPath -Encoding UTF8
    
    Write-Host "✓ Deployment report generated: $ReportPath" -ForegroundColor Green
}
catch {
    Write-Host "✗ Deployment report generation failed: $_" -ForegroundColor Red
    exit 1
}

# Deployment summary
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Status: SUCCESS" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor White
Write-Host "Deployment ID: $DeploymentId" -ForegroundColor White
Write-Host "Deployment Path: $DeploymentPath" -ForegroundColor White
Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor White

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor White
Write-Host "1. Test the deployment using the CLI:" -ForegroundColor Gray
Write-Host "   $DeploymentPath/cli/LLMSensitiveDataGoverance.CLI.exe --help" -ForegroundColor Gray
Write-Host "2. Validate configuration:" -ForegroundColor Gray
Write-Host "   $DeploymentPath/cli/LLMSensitiveDataGoverance.CLI.exe validate" -ForegroundColor Gray
Write-Host "3. Review deployment report:" -ForegroundColor Gray
Write-Host "   $DeploymentPath/deployment-report.json" -ForegroundColor Gray

Write-Host ""
Write-Host "✓ Deployment completed successfully!" -ForegroundColor Green