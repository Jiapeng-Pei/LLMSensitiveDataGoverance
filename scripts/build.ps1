#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Build script for LLM Sensitive Data Governance system
.DESCRIPTION
    Builds all projects, runs tests, and creates deployment packages
.PARAMETER Configuration
    Build configuration (Debug, Release)
.PARAMETER SkipTests
    Skip running tests
.PARAMETER SkipPackaging
    Skip creating NuGet packages
.PARAMETER OutputPath
    Output directory for build artifacts
#>

param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter()]
    [switch]$SkipTests,
    
    [Parameter()]
    [switch]$SkipPackaging,
    
    [Parameter()]
    [string]$OutputPath = "artifacts"
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Build information
$BuildId = if ($env:BUILD_ID) { $env:BUILD_ID } else { Get-Date -Format "yyyyMMdd-HHmmss" }
$Version = if ($env:BUILD_VERSION) { $env:BUILD_VERSION } else { "1.0.0-dev" }

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "LLM Sensitive Data Governance Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Green
Write-Host "Build ID: $BuildId" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Green
Write-Host "Output Path: $OutputPath" -ForegroundColor Green
Write-Host ""

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Projects to build
$Projects = @(
    "src/LLMSensitiveDataGoverance.Core/LLMSensitiveDataGoverance.Core.csproj",
    "src/LLMSensitiveDataGoverance.CLI/LLMSensitiveDataGoverance.CLI.csproj",
    "src/LLMSensitiveDataGoverance.AgentIntegration/LLMSensitiveDataGoverance.AgentIntegration.csproj"
)

# Test projects
$TestProjects = @(
    "tests/LLMSensitiveDataGoverance.Core.Tests/LLMSensitiveDataGoverance.Core.Tests.csproj",
    "tests/LLMSensitiveDataGoverance.CLI.Tests/LLMSensitiveDataGoverance.CLI.Tests.csproj",
    "tests/LLMSensitiveDataGoverance.AgentIntegration.Tests/LLMSensitiveDataGoverance.AgentIntegration.Tests.csproj"
)

# Step 1: Clean previous build
Write-Host "Step 1: Cleaning previous build..." -ForegroundColor Yellow
try {
    dotnet clean --configuration $Configuration --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }
    Write-Host "✓ Clean completed" -ForegroundColor Green
}
catch {
    Write-Host "✗ Clean failed: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Restore dependencies
Write-Host "Step 2: Restoring dependencies..." -ForegroundColor Yellow
try {
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    Write-Host "✓ Restore completed" -ForegroundColor Green
}
catch {
    Write-Host "✗ Restore failed: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Build projects
Write-Host "Step 3: Building projects..." -ForegroundColor Yellow
foreach ($project in $Projects) {
    Write-Host "  Building $project..." -ForegroundColor Gray
    try {
        dotnet build $project --configuration $Configuration --no-restore --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw "Build failed for $project" }
        Write-Host "  ✓ $project built successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Build failed for $project: $_" -ForegroundColor Red
        exit 1
    }
}

# Step 4: Run tests
if (-not $SkipTests) {
    Write-Host "Step 4: Running tests..." -ForegroundColor Yellow
    
    $TestResults = @()
    foreach ($testProject in $TestProjects) {
        if (Test-Path $testProject) {
            Write-Host "  Running tests for $testProject..." -ForegroundColor Gray
            try {
                $TestResultFile = "$OutputPath/TestResults_$(Split-Path -Leaf $testProject).xml"
                dotnet test $testProject --configuration $Configuration --no-build --verbosity quiet --logger "trx;LogFileName=$TestResultFile"
                if ($LASTEXITCODE -ne 0) { throw "Tests failed for $testProject" }
                Write-Host "  ✓ Tests passed for $testProject" -ForegroundColor Green
                $TestResults += $TestResultFile
            }
            catch {
                Write-Host "  ✗ Tests failed for $testProject: $_" -ForegroundColor Red
                exit 1
            }
        }
    }
    
    Write-Host "✓ All tests completed" -ForegroundColor Green
    Write-Host "  Test results saved to: $OutputPath" -ForegroundColor Gray
}
else {
    Write-Host "Step 4: Skipping tests..." -ForegroundColor Yellow
}

# Step 5: Create packages
if (-not $SkipPackaging) {
    Write-Host "Step 5: Creating packages..." -ForegroundColor Yellow
    
    $PackageProjects = @(
        "src/LLMSensitiveDataGoverance.Core/LLMSensitiveDataGoverance.Core.csproj",
        "src/LLMSensitiveDataGoverance.AgentIntegration/LLMSensitiveDataGoverance.AgentIntegration.csproj"
    )
    
    foreach ($project in $PackageProjects) {
        Write-Host "  Creating package for $project..." -ForegroundColor Gray
        try {
            dotnet pack $project --configuration $Configuration --no-build --output $OutputPath --verbosity quiet -p:PackageVersion=$Version
            if ($LASTEXITCODE -ne 0) { throw "Package creation failed for $project" }
            Write-Host "  ✓ Package created for $project" -ForegroundColor Green
        }
        catch {
            Write-Host "  ✗ Package creation failed for $project: $_" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "✓ All packages created" -ForegroundColor Green
}
else {
    Write-Host "Step 5: Skipping packaging..." -ForegroundColor Yellow
}

# Step 6: Publish CLI
Write-Host "Step 6: Publishing CLI application..." -ForegroundColor Yellow
try {
    $PublishPath = "$OutputPath/cli"
    dotnet publish "src/LLMSensitiveDataGoverance.CLI/LLMSensitiveDataGoverance.CLI.csproj" --configuration $Configuration --output $PublishPath --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "CLI publish failed" }
    Write-Host "✓ CLI published to: $PublishPath" -ForegroundColor Green
}
catch {
    Write-Host "✗ CLI publish failed: $_" -ForegroundColor Red
    exit 1
}

# Step 7: Create deployment archive
Write-Host "Step 7: Creating deployment archive..." -ForegroundColor Yellow
try {
    $ArchivePath = "$OutputPath/LLMSensitiveDataGoverance-$Version.zip"
    
    # Create temporary directory for archive contents
    $TempDir = "$OutputPath/temp"
    if (Test-Path $TempDir) { Remove-Item $TempDir -Recurse -Force }
    New-Item -ItemType Directory -Path $TempDir -Force | Out-Null
    
    # Copy files to archive
    Copy-Item -Path "config" -Destination "$TempDir/config" -Recurse
    Copy-Item -Path "examples" -Destination "$TempDir/examples" -Recurse
    Copy-Item -Path "docs" -Destination "$TempDir/docs" -Recurse
    Copy-Item -Path "$OutputPath/cli" -Destination "$TempDir/cli" -Recurse
    Copy-Item -Path "README.md" -Destination "$TempDir/"
    Copy-Item -Path "LICENSE" -Destination "$TempDir/" -ErrorAction SilentlyContinue
    
    # Create zip archive
    if (Get-Command "7z" -ErrorAction SilentlyContinue) {
        & 7z a -tzip "$ArchivePath" "$TempDir/*"
    }
    elseif (Get-Command "Compress-Archive" -ErrorAction SilentlyContinue) {
        Compress-Archive -Path "$TempDir/*" -DestinationPath $ArchivePath -Force
    }
    else {
        Write-Host "  Warning: No compression utility found, skipping archive creation" -ForegroundColor Yellow
    }
    
    # Clean up temporary directory
    Remove-Item $TempDir -Recurse -Force
    
    if (Test-Path $ArchivePath) {
        Write-Host "✓ Deployment archive created: $ArchivePath" -ForegroundColor Green
    }
}
catch {
    Write-Host "✗ Archive creation failed: $_" -ForegroundColor Red
    exit 1
}

# Step 8: Generate build report
Write-Host "Step 8: Generating build report..." -ForegroundColor Yellow
try {
    $BuildReport = @{
        BuildId = $BuildId
        Version = $Version
        Configuration = $Configuration
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Projects = $Projects
        TestResults = if ($SkipTests) { "Skipped" } else { "Passed" }
        Packages = if ($SkipPackaging) { "Skipped" } else { "Created" }
        OutputPath = $OutputPath
    }
    
    $BuildReportPath = "$OutputPath/build-report.json"
    $BuildReport | ConvertTo-Json -Depth 3 | Out-File -FilePath $BuildReportPath -Encoding UTF8
    
    Write-Host "✓ Build report generated: $BuildReportPath" -ForegroundColor Green
}
catch {
    Write-Host "✗ Build report generation failed: $_" -ForegroundColor Red
    exit 1
}

# Build summary
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Build Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Status: SUCCESS" -ForegroundColor Green
Write-Host "Build ID: $BuildId" -ForegroundColor White
Write-Host "Version: $Version" -ForegroundColor White
Write-Host "Configuration: $Configuration" -ForegroundColor White
Write-Host "Output Path: $OutputPath" -ForegroundColor White

# List output files
Write-Host ""
Write-Host "Build Artifacts:" -ForegroundColor White
Get-ChildItem -Path $OutputPath -File | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "✓ Build completed successfully!" -ForegroundColor Green