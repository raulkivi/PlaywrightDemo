# Playwright Artifact Management Script for Local Development
# This script mimics the Azure DevOps pipeline artifact organization locally

param(
    [string]$BuildNumber = "local-$(Get-Date -Format 'yyyyMMdd-HHmmss')",
    [string]$OutputPath = "./test-artifacts",
    [switch]$CleanOldArtifacts,
    [switch]$OpenReport
)

Write-Host "🎭 Playwright Artifact Manager" -ForegroundColor Cyan
Write-Host "Build: $BuildNumber" -ForegroundColor Yellow

# Create organized artifact structure
$artifactsDir = Join-Path $OutputPath $BuildNumber
$videosDir = Join-Path $artifactsDir "videos"
$screenshotsDir = Join-Path $artifactsDir "screenshots"
$tracesDir = Join-Path $artifactsDir "traces"
$reportsDir = Join-Path $artifactsDir "reports"

Write-Host "📁 Creating artifact directories..." -ForegroundColor Green

New-Item -ItemType Directory -Force -Path $videosDir | Out-Null
New-Item -ItemType Directory -Force -Path $screenshotsDir | Out-Null
New-Item -ItemType Directory -Force -Path $tracesDir | Out-Null
New-Item -ItemType Directory -Force -Path $reportsDir | Out-Null

# Find test output directories
$testBinDir = "PlaywrightDemo.Tests/bin/Debug/net9.0"
$testBinReleaseDir = "PlaywrightDemo.Tests/bin/Release/net9.0"

# Use Release if available, otherwise Debug
$sourceDir = if (Test-Path $testBinReleaseDir) { $testBinReleaseDir } else { $testBinDir }

Write-Host "📹 Collecting artifacts from: $sourceDir" -ForegroundColor Blue

# Copy videos (only failed tests due to our smart recording)
if (Test-Path "$sourceDir/videos") {
    $videoFiles = Get-ChildItem "$sourceDir/videos" -Filter "*.webm"
    if ($videoFiles.Count -gt 0) {
        Copy-Item -Path "$sourceDir/videos/*.webm" -Destination $videosDir -Force
        Write-Host "   ✅ Copied $($videoFiles.Count) video file(s)" -ForegroundColor Green
    } else {
        Write-Host "   ℹ️  No videos found (all tests passed!)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ℹ️  No videos directory found" -ForegroundColor Yellow
}

# Copy custom video folders
@("test-recordings", "test-screenshots", "custom-videos", "custom-screenshots") | ForEach-Object {
    $customDir = "$sourceDir/$_"
    if (Test-Path $customDir) {
        $files = Get-ChildItem $customDir
        if ($files.Count -gt 0) {
            Copy-Item -Path "$customDir/*" -Destination $videosDir -Force
            Write-Host "   ✅ Copied $($files.Count) file(s) from $_" -ForegroundColor Green
        }
    }
}

# Copy screenshots (failure screenshots)
if (Test-Path "$sourceDir/screenshots") {
    $screenshotFiles = Get-ChildItem "$sourceDir/screenshots" -Filter "*.png"
    if ($screenshotFiles.Count -gt 0) {
        Copy-Item -Path "$sourceDir/screenshots/*.png" -Destination $screenshotsDir -Force
        Write-Host "   ✅ Copied $($screenshotFiles.Count) screenshot(s)" -ForegroundColor Green
    } else {
        Write-Host "   ℹ️  No screenshots found" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ℹ️  No screenshots directory found" -ForegroundColor Yellow
}

# Copy traces if they exist
if (Test-Path "$sourceDir/traces") {
    $traceFiles = Get-ChildItem "$sourceDir/traces"
    if ($traceFiles.Count -gt 0) {
        Copy-Item -Path "$sourceDir/traces/*" -Destination $tracesDir -Force -Recurse
        Write-Host "   ✅ Copied $($traceFiles.Count) trace file(s)" -ForegroundColor Green
    }
}

# Generate summary report
$videoCount = (Get-ChildItem $videosDir -Filter "*.webm" -ErrorAction SilentlyContinue).Count
$screenshotCount = (Get-ChildItem $screenshotsDir -Filter "*.png" -ErrorAction SilentlyContinue).Count
$traceCount = (Get-ChildItem $tracesDir -ErrorAction SilentlyContinue).Count

$summary = @"
# Playwright Test Artifacts - $BuildNumber

**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
**Source:** $sourceDir

## 📊 Artifact Summary

| Type | Count | Description |
|------|-------|-------------|
| Videos | $videoCount | Failed test recordings (WebM format) |
| Screenshots | $screenshotCount | Failure screenshots (PNG format) |
| Traces | $traceCount | Playwright trace files |

## 🎥 Videos
$(if ($videoCount -gt 0) {
    $videoFiles = Get-ChildItem $videosDir -Filter "*.webm"
    $videoFiles | ForEach-Object { "- [$($_.Name)](./$($_.Name)) - $([math]::Round($_.Length/1MB, 2)) MB" } | Out-String
} else {
    "No video files (all tests passed!) 🎉"
})

## 📸 Screenshots  
$(if ($screenshotCount -gt 0) {
    $screenshotFiles = Get-ChildItem $screenshotsDir -Filter "*.png"
    $screenshotFiles | ForEach-Object { "- [$($_.Name)](./$($_.Name)) - $([math]::Round($_.Length/1KB, 0)) KB" } | Out-String
} else {
    "No screenshot files"
})

## 🔍 How to View

### Videos
- Open `.webm` files in any modern browser (Chrome, Firefox, Edge)
- Use VLC Media Player or similar for desktop viewing
- Videos show complete test execution including failures

### Screenshots
- Open `.png` files in any image viewer
- Screenshots capture the exact moment of test failure
- Filenames include timestamp and test name for easy identification

### For Azure DevOps Integration
- Upload this entire folder as a pipeline artifact
- Videos and screenshots will be downloadable from the pipeline run
- Use the PublishPipelineArtifact@1 task in your YAML pipeline

## 💡 Tips
- Videos are only recorded for failed tests (space-efficient)
- Screenshots provide quick failure identification  
- Traces (if enabled) offer the most detailed debugging information
- Consider setting up Azure Blob Storage for long-term artifact retention
"@

$summaryPath = Join-Path $artifactsDir "README.md"
$summary | Out-File -FilePath $summaryPath -Encoding UTF8

# Create simple HTML index for easy browsing
$htmlIndex = @"
<!DOCTYPE html>
<html>
<head>
    <title>Playwright Test Artifacts - $BuildNumber</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; margin: 40px; }
        .header { background: #0078d4; color: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; }
        .section { margin: 20px 0; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }
        .artifact-list { display: grid; gap: 10px; }
        .artifact-item { padding: 10px; border: 1px solid #eee; border-radius: 4px; }
        .artifact-item:hover { background: #f5f5f5; }
        .no-items { color: #666; font-style: italic; }
        video { max-width: 100%; height: auto; }
        img { max-width: 100%; height: auto; border: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class="header">
        <h1>🎭 Playwright Test Artifacts</h1>
        <p>Build: $BuildNumber | Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
    </div>
    
    <div class="section">
        <h2>📹 Test Videos ($videoCount files)</h2>
        <div class="artifact-list">
"@

if ($videoCount -gt 0) {
    $videoFiles = Get-ChildItem $videosDir -Filter "*.webm"
    foreach ($video in $videoFiles) {
        $htmlIndex += @"
            <div class="artifact-item">
                <h3>$($video.Name)</h3>
                <p>Size: $([math]::Round($video.Length/1MB, 2)) MB</p>
                <video controls>
                    <source src="videos/$($video.Name)" type="video/webm">
                    Your browser does not support the video tag.
                </video>
            </div>
"@
    }
} else {
    $htmlIndex += '<div class="no-items">🎉 No videos found - all tests passed!</div>'
}

$htmlIndex += @"
        </div>
    </div>
    
    <div class="section">
        <h2>📸 Screenshots ($screenshotCount files)</h2>
        <div class="artifact-list">
"@

if ($screenshotCount -gt 0) {
    $screenshotFiles = Get-ChildItem $screenshotsDir -Filter "*.png"
    foreach ($screenshot in $screenshotFiles) {
        $htmlIndex += @"
            <div class="artifact-item">
                <h3>$($screenshot.Name)</h3>
                <p>Size: $([math]::Round($screenshot.Length/1KB, 0)) KB</p>
                <img src="screenshots/$($screenshot.Name)" alt="$($screenshot.Name)">
            </div>
"@
    }
} else {
    $htmlIndex += '<div class="no-items">No screenshots found</div>'
}

$htmlIndex += @"
        </div>
    </div>
    
    <div class="section">
        <h2>📋 Summary</h2>
        <ul>
            <li><strong>Total Videos:</strong> $videoCount (failed tests only)</li>
            <li><strong>Total Screenshots:</strong> $screenshotCount</li>
            <li><strong>Total Traces:</strong> $traceCount</li>
            <li><strong>Artifact Location:</strong> $artifactsDir</li>
        </ul>
        <p><em>This report provides a quick overview of test artifacts. Videos and screenshots help debug failed tests efficiently.</em></p>
    </div>
</body>
</html>
"@

$htmlPath = Join-Path $artifactsDir "index.html"
$htmlIndex | Out-File -FilePath $htmlPath -Encoding UTF8

# Clean old artifacts if requested
if ($CleanOldArtifacts) {
    Write-Host "🧹 Cleaning old artifacts..." -ForegroundColor Yellow
    $oldArtifacts = Get-ChildItem $OutputPath -Directory | Where-Object { $_.Name -ne $BuildNumber }
    $oldArtifacts | ForEach-Object {
        Remove-Item -Path $_.FullName -Recurse -Force
        Write-Host "   Removed: $($_.Name)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "✅ Artifacts organized successfully!" -ForegroundColor Green
Write-Host "📁 Location: $artifactsDir" -ForegroundColor Cyan
Write-Host "📊 Summary: $videoCount videos, $screenshotCount screenshots, $traceCount traces" -ForegroundColor Cyan

if ($OpenReport) {
    Write-Host "🌐 Opening HTML report..." -ForegroundColor Blue
    Start-Process $htmlPath
}

Write-Host ""
Write-Host "💡 Next steps:" -ForegroundColor Yellow
Write-Host "   - View artifacts: $artifactsDir" 
Write-Host "   - Open HTML report: $htmlPath"
Write-Host "   - For Azure DevOps: Upload entire folder as pipeline artifact"
