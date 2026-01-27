using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightDemo.Tests;

/// <summary>
/// Tests for interactive UI elements like modals, alerts, drag & drop, etc.
/// </summary>
[TestFixture]
public class InteractiveElementsTests : PlaywrightTestBase
{
    [Test]
    [Description("Test modal dialog interactions")]
    public async Task OpenModal_ShouldShowAndAllowInteraction()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Initially modal should not be visible
        await Expect(Page.Locator("[data-testid='test-modal']")).Not.ToBeVisibleAsync();
        
        // Open modal
        await Page.ClickAsync("[data-testid='open-modal-button']");
        
        // Modal should now be visible
        await Expect(Page.Locator("[data-testid='test-modal']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='modal-body']")).ToBeVisibleAsync();
        
        // Interact with modal input
        await Page.FillAsync("[data-testid='modal-input']", "Test input in modal");
        await Expect(Page.Locator("[data-testid='modal-input']")).ToHaveValueAsync("Test input in modal");
        
        // Close modal using X button
        await Page.ClickAsync("[data-testid='close-modal-x']");
        
        // Modal should be hidden
        await Expect(Page.Locator("[data-testid='test-modal']")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Description("Test modal close functionality")]
    public async Task CloseModal_ShouldHideModal()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Open modal
        await Page.ClickAsync("[data-testid='open-modal-button']");
        await Expect(Page.Locator("[data-testid='test-modal']")).ToBeVisibleAsync();
        
        // Close using close button
        await Page.ClickAsync("[data-testid='close-modal-button']");
        await Expect(Page.Locator("[data-testid='test-modal']")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Description("Test range slider functionality")]
    public async Task RangeSlider_ShouldUpdateValue()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Initial value should be 50
        await Expect(Page.Locator("[data-testid='slider-value']")).ToHaveTextAsync("50");
        
        // Change slider value
        await Page.FillAsync("[data-testid='range-slider']", "75");
        
        // Value should update
        await Expect(Page.Locator("[data-testid='slider-value']")).ToHaveTextAsync("75");
        
        // Test another value
        await Page.FillAsync("[data-testid='range-slider']", "25");
        await Expect(Page.Locator("[data-testid='slider-value']")).ToHaveTextAsync("25");
    }
    
    [Test]
    [Description("Test form input elements")]
    public async Task FormInputs_ShouldAcceptValues()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Test color picker
        await Page.FillAsync("[data-testid='color-picker']", "#ff5733");
        await Expect(Page.Locator("[data-testid='color-picker']")).ToHaveValueAsync("#ff5733");
        
        // Test date picker
        await Page.FillAsync("[data-testid='date-picker']", "2024-12-31");
        await Expect(Page.Locator("[data-testid='date-picker']")).ToHaveValueAsync("2024-12-31");
        
        // Test toggle switch
        await Page.CheckAsync("[data-testid='toggle-switch']");
        await Expect(Page.Locator("[data-testid='toggle-switch']")).ToBeCheckedAsync();
        
        await Page.UncheckAsync("[data-testid='toggle-switch']");
        await Expect(Page.Locator("[data-testid='toggle-switch']")).Not.ToBeCheckedAsync();
    }
    
    [Test]
    [Description("Test dynamic content loading")]
    public async Task LoadDynamicContent_ShouldDisplayContent()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Initially no dynamic content
        var dynamicContent = Page.Locator("[data-testid='dynamic-content']");
        await Expect(dynamicContent).ToBeEmptyAsync();
        
        // Load content
        await Page.ClickAsync("[data-testid='load-content-button']");
        
        // Content should appear
        await Expect(dynamicContent).Not.ToBeEmptyAsync();
        await Expect(dynamicContent.Locator(".alert-info")).ToBeVisibleAsync();
        await Expect(dynamicContent).ToContainTextAsync("Content loaded dynamically!");
        await Expect(dynamicContent).ToContainTextAsync("Timestamp:");
    }
    
    [Test]
    [Description("Test tab navigation")]
    public async Task TabNavigation_ShouldSwitchContent()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Initially tab 1 should be active
        await Expect(Page.Locator("[data-testid='tab-1']")).ToHaveClassAsync("nav-link active");
        await Expect(Page.Locator("[data-testid='tab-content-1']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='tab-content-2']")).Not.ToBeVisibleAsync();
        
        // Click tab 2
        await Page.ClickAsync("[data-testid='tab-2']");
        
        // Tab 2 should now be active
        await Expect(Page.Locator("[data-testid='tab-2']")).ToHaveClassAsync("nav-link active");
        await Expect(Page.Locator("[data-testid='tab-content-2']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='tab-content-1']")).Not.ToBeVisibleAsync();
        
        // Click tab 3
        await Page.ClickAsync("[data-testid='tab-3']");
        
        // Tab 3 should now be active
        await Expect(Page.Locator("[data-testid='tab-3']")).ToHaveClassAsync("nav-link active");
        await Expect(Page.Locator("[data-testid='tab-content-3']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='tab-content-2']")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Description("Test drag and drop functionality")]
    public async Task DragAndDrop_ShouldMoveElement()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Initially draggable item should be in drop zone 1
        var dropZone1 = Page.Locator("[data-testid='drop-zone-1']");
        var dropZone2 = Page.Locator("[data-testid='drop-zone-2']");
        var draggableItem = Page.Locator("[data-testid='draggable-item-1']");
        
        // Verify initial state
        await Expect(dropZone1.Locator("[data-testid='draggable-item-1']")).ToBeVisibleAsync();
        
        // Perform drag and drop
        await draggableItem.DragToAsync(dropZone2);
        
        // Verify item moved to drop zone 2
        await Expect(dropZone2.Locator("[data-testid='draggable-item-1']")).ToBeVisibleAsync();
        await Expect(dropZone1.Locator("[data-testid='draggable-item-1']")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Description("Test JavaScript alert handling")]
    public async Task HandleAlerts_ShouldWorkCorrectly()
    {
        // Navigate to interactive page
        await NavigateToPageAsync("Interactive");
        
        // Set up alert handler
        Page.Dialog += async (_, dialog) =>
        {
            Assert.That(dialog.Message, Is.EqualTo("This is a JavaScript alert!"));
            await dialog.AcceptAsync();
        };
        
        // Trigger alert
        await Page.ClickAsync("[data-testid='alert-button']");
        
        // Test confirm dialog
        Page.Dialog += async (_, dialog) =>
        {
            if (dialog.Message.Contains("Are you sure"))
            {
                await dialog.AcceptAsync();
            }
            else if (dialog.Message.Contains("You clicked OK"))
            {
                await dialog.AcceptAsync();
            }
        };
        
        // Trigger confirm
        await Page.ClickAsync("[data-testid='confirm-button']");
    }
}
