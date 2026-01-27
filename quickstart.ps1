#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Quick start script for Playwright Demo
.DESCRIPTION
    This script sets up and runs the Playwright demo application and tests
.PARAMETER Action
    The action to perform: setup, run-app, run-tests, record, or all
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("setup", "run-app", "run-tests", "record", "all")]
    [string]$Action = "all"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "🔹 $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Setup-Project {
    Write-Step "Setting up the Playwright demo project..."
    
    # Build the solution
    Write-Step "Building the solution..."
    dotnet build PlaywrightDemo.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build the solution"
        exit 1
    }
    
    # Install Playwright browsers
    Write-Step "Installing Playwright browsers..."
    Push-Location "PlaywrightDemo.Tests"
    try {
        pwsh -File "bin/Debug/net9.0/playwright.ps1" install
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to install Playwright browsers"
            exit 1
        }
    }
    finally {
        Pop-Location
    }
    
    Write-Success "Setup completed successfully!"
}

function Start-WebApp {
    Write-Step "Starting the web application..."
    Write-Host "The web app will start at http://localhost:5275" -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
    
    Push-Location "PlaywrightDemo.WebApp"
    try {
        dotnet run
    }
    finally {
        Pop-Location
    }
}

function Run-Tests {
    Write-Step "Running Playwright tests..."
    Write-Host "Make sure the web application is running at http://localhost:5275" -ForegroundColor Yellow
    
    Push-Location "PlaywrightDemo.Tests"
    try {
        dotnet test --logger:console --verbosity normal
        if ($LASTEXITCODE -eq 0) {
            Write-Success "All tests passed!"
        } else {
            Write-Error "Some tests failed"
        }
    }
    finally {
        Pop-Location
    }
}

function Start-Recording {
    Write-Step "Starting Playwright test recording..."
    Write-Host "Make sure the web application is running at http://localhost:5275" -ForegroundColor Yellow
    Write-Host "A browser window will open with the Playwright Inspector" -ForegroundColor Yellow
    Write-Host "Interact with your application to record tests" -ForegroundColor Yellow
    
    # Check if playwright is installed globally
    $playwrightGlobal = Get-Command "playwright" -ErrorAction SilentlyContinue
    
    if ($playwrightGlobal) {
        playwright codegen http://localhost:5275
    } else {
        Write-Step "Global Playwright not found, using local installation..."
        Push-Location "PlaywrightDemo.Tests"
        try {
            pwsh -File "bin/Debug/net9.0/playwright.ps1" codegen http://localhost:5275
        }
        finally {
            Pop-Location
        }
    }
}

function Show-Help {
    Write-Host @"
🎭 Playwright Demo Quick Start

Available actions:
  setup     - Build project and install Playwright browsers
  run-app   - Start the web application
  run-tests - Run all Playwright tests
  record    - Start test recording with codegen
  all       - Perform setup and show next steps

Examples:
  .\quickstart.ps1 setup
  .\quickstart.ps1 run-app
  .\quickstart.ps1 run-tests
  .\quickstart.ps1 record

Quick Start Workflow:
1. Run: .\quickstart.ps1 setup
2. In terminal 1: .\quickstart.ps1 run-app
3. In terminal 2: .\quickstart.ps1 run-tests
4. For recording: .\quickstart.ps1 record

"@ -ForegroundColor White
}

# Main execution
Write-Host "🎭 Playwright Demo Quick Start" -ForegroundColor Magenta
Write-Host "==============================" -ForegroundColor Magenta

switch ($Action) {
    "setup" {
        Setup-Project
    }
    "run-app" {
        Start-WebApp
    }
    "run-tests" {
        Run-Tests
    }
    "record" {
        Start-Recording
    }
    "all" {
        Setup-Project
        Write-Host ""
        Write-Host "🚀 Setup completed! Next steps:" -ForegroundColor Green
        Write-Host ""
        Write-Host "1. Start the web application:" -ForegroundColor White
        Write-Host "   .\quickstart.ps1 run-app" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "2. In a new terminal, run tests:" -ForegroundColor White
        Write-Host "   .\quickstart.ps1 run-tests" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "3. To record new tests:" -ForegroundColor White
        Write-Host "   .\quickstart.ps1 record" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "4. Open the project in your IDE and explore:" -ForegroundColor White
        Write-Host "   - Web app pages in PlaywrightDemo.WebApp/Pages/" -ForegroundColor Yellow
        Write-Host "   - Test files in PlaywrightDemo.Tests/" -ForegroundColor Yellow
        Write-Host "   - README.md for detailed documentation" -ForegroundColor Yellow
    }
    default {
        Show-Help
    }
}
