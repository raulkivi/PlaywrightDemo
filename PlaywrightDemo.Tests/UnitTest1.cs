using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightDemo.Tests;

/// <summary>
/// Base test class for Playwright tests.
/// This class provides common setup and teardown functionality for all tests.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlaywrightTestBase : PageTest
{
    protected const string BaseUrl = "http://localhost:5275"; // Web app URL
    
    // Configurable folder paths for videos and screenshots
    protected virtual string VideosFolder { get; set; } = "videos";
    protected virtual string ScreenshotsFolder { get; set; } = "screenshots";
    
    // Video recording configuration
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions()
        {
            // Enable video recording for all tests
            RecordVideoDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, VideosFolder),
            RecordVideoSize = new RecordVideoSize() { Width = 1280, Height = 720 }
        };
    }
    
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        // Create videos directory if it doesn't exist
        var videoDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, VideosFolder);
        Directory.CreateDirectory(videoDir);
    }
    
    [SetUp]
    public async Task Setup()
    {
        // Configure browser context for each test
        await Context.RouteAsync("**/*", async route =>
        {
            // Handle any route modifications if needed
            await route.ContinueAsync();
        });
    }
    
    [TearDown]
    public async Task TearDown()
    {
        var testContext = TestContext.CurrentContext;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var testName = testContext.Test.Name.Replace("(", "_").Replace(")", "").Replace(" ", "_");
        var testFailed = testContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;
        
        // Handle video recording - only save if test failed
        try
        {
            var videoDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, VideosFolder);
            Directory.CreateDirectory(videoDir);
            
            // Get the video path from the page (Playwright automatically records)
            if (Page.Video != null)
            {
                var videoPath = await Page.Video.PathAsync();
                
                // Close the page to finish video recording
                await Page.CloseAsync();
                
                // Wait for the video to be finalized
                await Task.Delay(2000);
                
                // Check if the video file exists after waiting
                if (File.Exists(videoPath))
                {
                    if (testFailed)
                    {
                        // Test failed - save the video with FAILED prefix
                        var failedVideoName = $"FAILED_{testName}_{timestamp}.webm";
                        var savedVideoPath = Path.Combine(videoDir, failedVideoName);
                        
                        try
                        {
                            // Use File.Copy instead of Move to avoid issues if file is locked
                            File.Copy(videoPath, savedVideoPath, overwrite: true);
                            File.Delete(videoPath); // Clean up original
                            Console.WriteLine($"❌ Test failed - Video recording saved to: {savedVideoPath}");
                        }
                        catch (Exception moveEx)
                        {
                            Console.WriteLine($"⚠️ Test failed - Video saved but could not rename: {videoPath} (Error: {moveEx.Message})");
                        }
                    }
                    else
                    {
                        // Test passed - delete the video to save disk space
                        try
                        {
                            File.Delete(videoPath);
                            Console.WriteLine($"✅ Test passed - Video recording discarded (not saved)");
                        }
                        catch (Exception deleteEx)
                        {
                            Console.WriteLine($"⚠️ Test passed but could not delete video: {videoPath} (Error: {deleteEx.Message})");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Video path was provided but file doesn't exist: {videoPath}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ No video recording available for test: {testName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to handle video recording: {ex.Message}");
        }
        
        // Handle screenshot capture (only on failure)
        if (testFailed)
        {
            try
            {
                // Create screenshots directory if it doesn't exist
                var screenshotDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, ScreenshotsFolder);
                Directory.CreateDirectory(screenshotDir);
                
                // Generate filename with timestamp and test name
                var filename = $"FAILED_{testName}_{timestamp}.png";
                var screenshotPath = Path.Combine(screenshotDir, filename);
                
                // Capture screenshot (if page is still available and not closed)
                // Since we closed the page above for video, we'll skip screenshot if page is closed
                // But we still have the video for failed tests which is often more useful
                
                Console.WriteLine($"❌ Test failed - Screenshot would be captured here if page wasn't closed for video");
                Console.WriteLine($"❌ Video provides more comprehensive debugging information than static screenshot");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to capture screenshot: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Helper method to navigate to a specific page relative to the base URL
    /// </summary>
    protected async Task NavigateToPageAsync(string relativePath = "")
    {
        var url = $"{BaseUrl}/{relativePath}".TrimEnd('/');
        await Page.GotoAsync(url);
    }
    
    /// <summary>
    /// Helper method to manually capture a screenshot during test execution for debugging
    /// </summary>
    protected async Task CaptureDebugScreenshotAsync(string screenshotName)
    {
        try
        {
            var screenshotDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshots");
            Directory.CreateDirectory(screenshotDir);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var testName = TestContext.CurrentContext.Test.Name.Replace("(", "_").Replace(")", "").Replace(" ", "_");
            var filename = $"DEBUG_{testName}_{screenshotName}_{timestamp}.png";
            var screenshotPath = Path.Combine(screenshotDir, filename);
            
            await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
            Console.WriteLine($"Debug screenshot saved to: {screenshotPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture debug screenshot: {ex.Message}");
        }
    }
}
