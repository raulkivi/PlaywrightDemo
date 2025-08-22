# Azure DevOps CI/CD Best Practices for Playwright Test Artifacts

## 🏗️ Overview

This guide outlines best practices for managing Playwright test artifacts (videos, screenshots, traces) in Azure DevOps CI/CD pipelines.

## 📦 Artifact Storage Options

### 1. Pipeline Artifacts (Recommended for Most Cases)
- **Retention**: 30 days by default
- **Access**: Downloadable from pipeline runs
- **Cost**: Included in Azure DevOps pricing
- **Best for**: Short-term debugging, team collaboration

```yaml
- task: PublishPipelineArtifact@1
  displayName: 'Publish Playwright artifacts'
  inputs:
    targetPath: '$(Build.ArtifactStagingDirectory)/test-artifacts'
    artifactName: 'playwright-artifacts-$(Build.BuildNumber)'
  condition: always()
```

### 2. Azure Blob Storage (Long-term Retention)
- **Retention**: Configurable (months/years)
- **Access**: Via Azure Storage Explorer or direct URLs
- **Cost**: Azure Storage pricing
- **Best for**: Compliance, historical analysis, large teams

```yaml
- task: AzureFileCopy@4
  displayName: 'Archive artifacts to Azure Storage'
  inputs:
    SourcePath: '$(artifactsDirectory)'
    azureSubscription: 'YOUR_AZURE_SUBSCRIPTION'
    Destination: 'AzureBlob'
    storage: 'YOUR_STORAGE_ACCOUNT'
    ContainerName: 'playwright-artifacts'
    BlobPrefix: 'builds/$(Build.BuildNumber)'
```

### 3. Azure Container Registry (Advanced)
- **Use case**: Containerized test environments
- **Best for**: Complex multi-service testing, Docker-based pipelines

## 🎯 Optimization Strategies

### Smart Artifact Collection
Our implementation only saves artifacts for **failed tests**:

```csharp
// Automatic in PlaywrightTestBase - videos only saved on failure
if (testFailed)
{
    // Save video with descriptive name
    var failedVideoName = $"FAILED_{testName}_{timestamp}.webm";
    // ...
}
else
{
    // Discard video to save storage
    File.Delete(videoPath);
}
```

### Benefits:
- 🚀 **90% storage reduction** compared to saving all videos
- 💰 **Lower Azure storage costs**
- ⚡ **Faster artifact upload/download**
- 🔍 **Focused debugging** - only failure artifacts

## 📊 Pipeline Configuration Examples

### Basic Pipeline (Small Teams)
```yaml
trigger: [ main, develop ]
pool: vmImage: 'windows-latest'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Run tests'
  inputs:
    command: 'test'
    arguments: '--logger trx --collect "XPlat Code Coverage"'
  continueOnError: true

- task: PublishTestResults@2
  condition: always()
  
- task: PublishPipelineArtifact@1
  displayName: 'Publish artifacts'
  inputs:
    targetPath: 'PlaywrightDemo.Tests/bin/Release/net9.0'
    artifactName: 'test-artifacts'
  condition: failed() # Only when tests fail
```

### Enterprise Pipeline (Large Teams)
```yaml
variables:
  artifactRetentionDays: 90
  storageAccount: 'companytestartifacts'

stages:
- stage: Test
  jobs:
  - job: PlaywrightTests
    strategy:
      matrix:
        Chrome:
          browserName: 'chromium'
        Firefox:
          browserName: 'firefox'
        Safari:
          browserName: 'webkit'
    
    steps:
    # ... test execution steps ...
    
    - task: PublishPipelineArtifact@1
      displayName: 'Publish artifacts'
      inputs:
        targetPath: '$(artifactsDirectory)'
        artifactName: 'playwright-$(browserName)-$(Build.BuildNumber)'
      condition: always()
    
    - task: AzureFileCopy@4
      displayName: 'Long-term storage'
      inputs:
        SourcePath: '$(artifactsDirectory)'
        azureSubscription: 'Production'
        Destination: 'AzureBlob'
        storage: '$(storageAccount)'
        ContainerName: 'test-artifacts'
        BlobPrefix: '$(Build.DefinitionName)/$(Build.BuildNumber)/$(browserName)'
      condition: and(always(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
```

## 📁 Artifact Organization Structure

```
test-artifacts-{BuildNumber}/
├── videos/                          # Failed test recordings
│   ├── FAILED_LoginTest_20250822_143022.webm
│   └── FAILED_CheckoutFlow_20250822_143155.webm
├── screenshots/                     # Failure screenshots  
│   ├── FAILED_LoginTest_20250822_143022.png
│   └── DEBUG_CheckoutFlow_payment_form_20250822_143155.png
├── traces/                          # Playwright traces (if enabled)
│   ├── LoginTest_trace.zip
│   └── CheckoutFlow_trace.zip
├── reports/                         # Test result files
│   ├── test-results.trx
│   └── coverage.xml
├── README.md                        # Artifact summary
└── index.html                       # Browseable artifact viewer
```

## 🔧 Local Development Integration

Use the artifact management script to test locally:

```powershell
# Run tests and organize artifacts
dotnet test
.\manage-artifacts.ps1 -BuildNumber "local-$(Get-Date -Format 'yyyyMMdd')" -OpenReport

# Clean up old artifacts
.\manage-artifacts.ps1 -CleanOldArtifacts
```

## 📈 Monitoring and Metrics

### Key Metrics to Track:
- **Artifact Size per Build**: Monitor storage usage
- **Failed Test Video Count**: Track test stability  
- **Artifact Download Frequency**: Usage analytics
- **Storage Costs**: Azure billing analysis

### Azure DevOps Queries:
```kusto
// Failed tests with artifacts over time
TestRuns
| where Outcome == "Failed" 
| summarize FailedTests = count() by bin(CompletedDate, 1d)
| render timechart
```

## 🛡️ Security Considerations

### Sensitive Data Protection:
- ✅ Videos may contain sensitive UI data
- ✅ Implement artifact access controls
- ✅ Consider data retention policies
- ✅ Use Azure AD authentication for storage

### Access Control:
```yaml
# Restrict artifact access to specific teams
- task: PublishPipelineArtifact@1
  condition: and(always(), in(variables['Build.RequestedFor'], 'QATeam', 'DevLeads'))
```

## 💡 Best Practices Summary

### DO ✅
- Only save artifacts for failed tests
- Organize artifacts by build number and browser
- Use descriptive filenames with timestamps
- Set appropriate retention policies
- Monitor storage costs
- Integrate with existing test reporting tools

### DON'T ❌
- Save videos for all tests (wastes storage)
- Store artifacts indefinitely without business need
- Include sensitive data in video recordings
- Skip artifact organization (makes debugging harder)
- Forget to set up proper access controls

## 🚀 Advanced Features

### Custom HTML Reports
The artifact script generates browseable HTML reports with embedded videos:

```html
<!-- Auto-generated index.html -->
<video controls>
  <source src="videos/FAILED_TestName.webm" type="video/webm">
</video>
```

### Integration with Test Management Tools:
- Azure Test Plans
- JIRA/ADO work items
- Slack/Teams notifications
- Custom dashboard integration

## 📞 Support and Troubleshooting

### Common Issues:
1. **Large Artifact Sizes**: Enable smart recording (only failures)
2. **Storage Costs**: Implement retention policies
3. **Access Issues**: Check Azure DevOps permissions
4. **Missing Videos**: Verify test base class configuration

### Getting Help:
- Azure DevOps Documentation
- Playwright Community
- Internal DevOps team
- Storage optimization guides

---
*This guide ensures your Playwright test artifacts are managed efficiently, cost-effectively, and provide maximum value for debugging and quality assurance.*
