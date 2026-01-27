using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightDemo.Tests;

/// <summary>
/// Sample recorded tests for the Contact form.
/// These tests demonstrate how to record user interactions and play them back.
/// </summary>
[TestFixture]
public class ContactFormTests : PlaywrightTestBase
{
    [Test]
    [Description("Test filling out and submitting the contact form")]
    public async Task FillContactForm_ShouldShowSuccessMessage()
    {
        // Navigate to contact page
        await NavigateToPageAsync("Contact");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Fill out the form using modern GetByTestId methods
        await Page.GetByTestId("first-name").FillAsync("John");
        await Page.GetByTestId("last-name").FillAsync("Doe");
        await Page.GetByTestId("email").FillAsync("john.doe@example.com");
        await Page.GetByTestId("subject").SelectOptionAsync("general");
        await Page.GetByTestId("message").FillAsync("This is a test message from Playwright automation.");
        await Page.GetByTestId("newsletter").CheckAsync();
        
        // Submit the form
        await Page.GetByTestId("submit-button").ClickAsync();
        
        // Wait for page to reload after form submission
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for and verify success message using modern assertions
        await Expect(Page.GetByTestId("success-message")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("success-message")).ToContainTextAsync("Success!");
        await Expect(Page.GetByTestId("success-message")).ToContainTextAsync("Your message has been sent successfully");
    }
    
    [Test]
    [Description("Test form validation for required fields")]
    public async Task SubmitEmptyForm_ShouldShowValidationErrors()
    {
        // Navigate to contact page
        await NavigateToPageAsync("Contact");
        
        // Try to submit without filling required fields
        await Page.GetByTestId("submit-button").ClickAsync();
        
        // Check that form doesn't submit (no success message appears)
        await Expect(Page.GetByTestId("success-message")).Not.ToBeVisibleAsync();
        
        // Verify required field validation
        var firstNameField = Page.GetByTestId("first-name");
        await Expect(firstNameField).ToHaveAttributeAsync("required", "");
        
        var lastNameField = Page.GetByTestId("last-name");
        await Expect(lastNameField).ToHaveAttributeAsync("required", "");
        
        var emailField = Page.GetByTestId("email");
        await Expect(emailField).ToHaveAttributeAsync("required", "");
        
        var messageField = Page.GetByTestId("message");
        await Expect(messageField).ToHaveAttributeAsync("required", "");
    }
    
    [Test]
    [Description("Test all form elements and their interactions")]
    public async Task InteractWithAllFormElements_ShouldWorkCorrectly()
    {
        // Navigate to contact page
        await NavigateToPageAsync("Contact");
        
        // Test text inputs
        await Page.GetByTestId("first-name").FillAsync("Jane");
        await Expect(Page.GetByTestId("first-name")).ToHaveValueAsync("Jane");
        
        await Page.GetByTestId("last-name").FillAsync("Smith");
        await Expect(Page.GetByTestId("last-name")).ToHaveValueAsync("Smith");
        
        await Page.GetByTestId("email").FillAsync("jane.smith@test.com");
        await Expect(Page.GetByTestId("email")).ToHaveValueAsync("jane.smith@test.com");
        
        // Test dropdown selection
        await Page.GetByTestId("subject").SelectOptionAsync("support");
        await Expect(Page.GetByTestId("subject")).ToHaveValueAsync("support");
        
        // Test textarea
        var testMessage = "This is a multi-line test message.\nIt contains multiple lines\nto test textarea functionality.";
        await Page.GetByTestId("message").FillAsync(testMessage);
        await Expect(Page.GetByTestId("message")).ToHaveValueAsync(testMessage);
        
        // Test checkbox
        await Page.GetByTestId("newsletter").CheckAsync();
        await Expect(Page.GetByTestId("newsletter")).ToBeCheckedAsync();
        
        // Uncheck and verify
        await Page.GetByTestId("newsletter").UncheckAsync();
        await Expect(Page.GetByTestId("newsletter")).Not.ToBeCheckedAsync();
        
        // Test navigation links
        await Expect(Page.GetByTestId("home-link")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("home-link")).ToHaveAttributeAsync("href", "/");
    }
}
