using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightDemo.Tests;

/// <summary>
/// Example of how to use custom folders for videos and screenshots.
/// This demonstrates different ways to configure the output directories.
/// </summary>

// Example 1: Using custom folders with inheritance
[TestFixture]
public class CustomVideoFolderTests : PlaywrightTestBase
{
    public CustomVideoFolderTests()
    {
        // Custom folder names - will be created under TestContext.CurrentContext.WorkDirectory
        VideosFolder = "test-recordings";        // Videos will go to: bin/Debug/net9.0/test-recordings/
        ScreenshotsFolder = "test-screenshots";  // Screenshots will go to: bin/Debug/net9.0/test-screenshots/
    }

    [Test]
    public async Task ExampleTest_WithCustomFolders()
    {
        await NavigateToPageAsync("Contact");
        await Expect(Page.GetByTestId("first-name")).ToBeVisibleAsync();
        
        // This test will save videos to 'test-recordings' folder if it fails
        // Screenshots would go to 'test-screenshots' folder
    }
}

// Example 2: Using absolute paths for specific project structure
[TestFixture] 
public class AbsolutePathTests : PlaywrightTestBase
{
    public AbsolutePathTests()
    {
        // Using absolute paths for specific locations
        VideosFolder = @"C:\TestResults\Videos";
        ScreenshotsFolder = @"C:\TestResults\Screenshots";
        
        // Make sure the directories exist
        Directory.CreateDirectory(VideosFolder);
        Directory.CreateDirectory(ScreenshotsFolder);
    }

    [Test]
    public async Task ExampleTest_WithAbsolutePaths()
    {
        await NavigateToPageAsync("Products");
        await Expect(Page.GetByText("Our Products")).ToBeVisibleAsync();
        
        // This test will save videos/screenshots to C:\TestResults\ folders
    }
}

// Example 3: Using date-based folder organization
[TestFixture]
public class DateBasedFolderTests : PlaywrightTestBase
{
    public DateBasedFolderTests()
    {
        var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
        VideosFolder = Path.Combine("test-results", dateFolder, "videos");
        ScreenshotsFolder = Path.Combine("test-results", dateFolder, "screenshots");
    }

    [Test]
    public async Task ExampleTest_WithDateBasedFolders()
    {
        await NavigateToPageAsync("Interactive");
        await Expect(Page.GetByTestId("open-modal-button")).ToBeVisibleAsync();
        
        // This test will organize recordings by date:
        // bin/Debug/net9.0/test-results/2025-08-22/videos/
        // bin/Debug/net9.0/test-results/2025-08-22/screenshots/
    }
}
