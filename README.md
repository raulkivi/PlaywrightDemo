# Playwright Test Recording & Playback Demo

## Overview

This project demonstrates how to create and use Playwright for automated testing with a focus on test recording and playback. It includes a sample ASP.NET Core web application with va   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target mstest https://localhost:7294
   ```

### Recording Best Practices

1. **Use data-testid attributes**: All interactive elements in the demo app have `data-testid` attributes for reliable element selection.

2. **Record in logical chunks**: Don't record entire user flows in one go. Break them into smaller, focused test scenarios.

3. **Copy-paste friendly workflow**: Modern Playwright recorder generates code that can be directly copy-pasted into your tests:
   ```csharp
   // ✅ These work exactly as recorded - no modification needed!
   await Page.GetByTestId("first-name").FillAsync("John");
   await Page.GetByTestId("submit-button").ClickAsync();
   await Page.GetByRole("button", new() { Name = "Save" }).ClickAsync();
   ```

4. **Clean up generated code** when needed:
   - Remove unnecessary waits
   - Add meaningful assertions
   - Group related actions
   - Add descriptive comments

5. **Use Page Object Model**: For complex applications, organize your selectors and actions into page objects.

#### Testing Recorder Behaviorelements and a comprehensive test suite.

## Project Structure

```
PlaywrightDemo/
├── PlaywrightDemo.sln
├── PlaywrightDemo.WebApp/          # Sample web application
│   ├── Pages/
│   │   ├── Index.cshtml            # Home page with navigation
│   │   ├── Contact.cshtml          # Contact form
│   │   ├── Products.cshtml         # Product catalog with search/filter
│   │   ├── Interactive.cshtml      # Interactive elements (modals, drag-drop, etc.)
│   │   └── Register.cshtml         # Multi-step registration form
│   └── Program.cs
└── PlaywrightDemo.Tests/           # Playwright test project
    ├── PlaywrightTestBase.cs       # Base test class
    ├── ContactFormTests.cs         # Contact form tests
    ├── ProductCatalogTests.cs      # Product catalog tests
    └── InteractiveElementsTests.cs # Interactive elements tests
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- PowerShell (for Playwright browser installation)

### 1. Running the Web Application

1. Navigate to the web app directory:
   ```powershell
   cd PlaywrightDemo.WebApp
   ```

2. Run the application:
   ```powershell
   dotnet run
   ```

3. Open your browser and navigate to `https://localhost:7294` (or the URL shown in the console)

### 2. Installing Playwright Browsers

1. Build the test project:
   ```powershell
   cd PlaywrightDemo.Tests
   dotnet build
   ```

2. Install Playwright browsers:
   ```powershell
   pwsh -File bin/Debug/net9.0/playwright.ps1 install
   ```

### 3. Running Tests

Run all tests:
```powershell
dotnet test
```

Run specific test class:
```powershell
dotnet test --filter "ContactFormTests"
```

Run specific test:
```powershell
dotnet test --filter "FillContactForm_ShouldShowSuccessMessage"
```

## Recording Tests with Playwright

### How Playwright Recorder Chooses Selectors

When recording interactions, Playwright follows a **priority order** for selector generation. Understanding this helps you predict what selectors will be generated, especially for legacy applications without `data-testid` attributes.

#### Playwright's Selector Priority (High to Low):

1. **`data-testid` attributes** - Highest priority, most reliable
   ```html
   <button data-testid="submit-btn">Submit</button>
   ```
   **Generated:** `await page.getByTestId('submit-btn').click();`

2. **Other `data-*` attributes** - High priority
   ```html
   <button data-automation="save-button">Save</button>
   ```
   **Generated:** `await page.locator('[data-automation="save-button"]').click();`

3. **Accessible roles and names** - Semantic and reliable
   ```html
   <button type="submit">Submit Form</button>
   <input aria-label="Email Address" type="email">
   ```
   **Generated:** 
   - `await page.getByRole('button', { name: 'Submit Form' }).click();`
   - `await page.getByRole('textbox', { name: 'Email Address' }).fill('test@example.com');`

4. **Labels and text content** - Good for user-facing elements
   ```html
   <label for="email">Email</label>
   <input id="email" name="email">
   ```
   **Generated:** `await page.getByLabel('Email').fill('test@example.com');`

5. **Placeholder text** - For inputs without labels
   ```html
   <input type="text" placeholder="Enter your name">
   ```
   **Generated:** `await page.getByPlaceholder('Enter your name').fill('John');`

6. **ID attributes** - Stable but may not be semantic
   ```html
   <button id="submit-button">Submit</button>
   ```
   **Generated:** `await page.locator('#submit-button').click();`

7. **CSS selectors** - Last resort, more fragile
   ```html
   <button class="btn btn-primary">Submit</button>
   ```
   **Generated:** `await page.locator('.btn-primary').click();`

#### Real Examples: Legacy App Recording

**HTML without data-testid:**
```html
<form class="contact-form">
  <label for="firstName">First Name</label>
  <input id="firstName" name="firstName" type="text" required>
  
  <label>Email Address</label>
  <input name="email" type="email" placeholder="Enter email">
  
  <button type="submit" class="btn btn-primary">Submit Form</button>
  <a href="/home">Go Home</a>
</form>
```

**What Playwright Recorder Generates:**
```csharp
// Input with ID and label - uses getByLabel
await page.GetByLabel("First Name").FillAsync("John");

// Input with placeholder but no ID - uses getByPlaceholder  
await page.GetByPlaceholder("Enter email").FillAsync("john@example.com");

// Button with semantic role - uses getByRole
await page.GetByRole("button", new() { Name = "Submit Form" }).ClickAsync();

// Link with text - uses getByRole
await page.GetByRole("link", new() { Name = "Go Home" }).ClickAsync();
```

#### When Recorder Falls Back to CSS Selectors

The recorder generates CSS selectors when elements lack semantic attributes:

**HTML:**
```html
<div class="form-group">
  <input type="text" class="form-control legacy-input">
  <button class="btn save-btn">Save</button>
</div>
```

**Generated (less reliable):**
```csharp
await page.Locator(".legacy-input").FillAsync("value");
await page.Locator(".save-btn").ClickAsync();
```

#### Improving Generated Selectors

After recording, you should **refactor fragile selectors**:

**Before (generated):**
```csharp
await page.Locator(".btn-primary").ClickAsync();
await page.Locator("input:nth-child(2)").FillAsync("John");
```

**After (improved):**
```csharp
await page.GetByRole("button", new() { Name = "Submit" }).ClickAsync();
await page.Locator("input[name='firstName']").FillAsync("John");
```

### Method 1: Using Playwright Codegen (Recommended)

1. **Install Playwright globally** (if not already installed):
   ```powershell
   npm install -g playwright
   ```

2. **Start your web application**:
   ```powershell
   cd PlaywrightDemo.WebApp
   dotnet run
   ```

3. **Start recording** (choose your test framework target):
   ```powershell
   # For NUnit (default)
   playwright codegen https://localhost:7294
   
   # For NUnit (explicit)
   playwright codegen --target nunit https://localhost:7294
   
   # For MSTest
   playwright codegen --target mstest https://localhost:7294
   ```

4. **Interact with your application**:
   - A browser window will open along with the Playwright Inspector
   - Perform actions on your web app (clicking, typing, selecting, etc.)
   - The Inspector will generate code in real-time

5. **Copy the generated code** directly into your test methods - most recorded actions can be copy-pasted without modification!

### Method 2: Using .NET Playwright

1. **Build your test project**:
   ```powershell
   cd PlaywrightDemo.Tests
   dotnet build
   ```

2. **Use Playwright CLI through .NET** (choose your target framework):
   ```powershell
   # For NUnit (default)
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen https://localhost:7294
   
   # For NUnit (explicit)
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target nunit https://localhost:7294
   
   # For MSTest
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target mstest https://localhost:7294
   ```

### Recording Best Practices

1. **Use data-testid attributes**: All interactive elements in the demo app have `data-testid` attributes for reliable element selection.

2. **Record in logical chunks**: Don't record entire user flows in one go. Break them into smaller, focused test scenarios.

3. **Clean up generated code**: Recorded code often needs refinement:
   - Remove unnecessary waits
   - Add meaningful assertions
   - Group related actions
   - Add descriptive comments

4. **Use Page Object Model**: For complex applications, organize your selectors and actions into page objects.

#### Testing Recorder Behavior

To see how the recorder handles your legacy application:

1. **Start recording on your legacy page**:
   ```powershell
   playwright codegen https://localhost:7294/legacy-test
   ```

2. **Interact with different element types** and observe the generated selectors:
   - Form inputs with/without labels
   - Buttons with different text content
   - Links and navigation elements
   - Dynamic elements

3. **Compare generated selectors** with your expectations and improve as needed.

#### Common Recorder Patterns for Legacy Apps

| Element Type | HTML Example | Recorder Generates |
|--------------|-------------|-------------------|
| **Labeled Input** | `<label>Name</label><input name="name">` | `getByLabel('Name')` |
| **ID Input** | `<input id="email" name="email">` | `locator('#email')` |
| **Placeholder Input** | `<input placeholder="Search...">` | `getByPlaceholder('Search...')` |
| **Submit Button** | `<button type="submit">Save</button>` | `getByRole('button', { name: 'Save' })` |
| **Class Button** | `<button class="save-btn">Save</button>` | `getByRole('button', { name: 'Save' })` |
| **Generic Button** | `<button class="btn-123">Click</button>` | `locator('.btn-123')` |
| **Text Link** | `<a href="/home">Home</a>` | `getByRole('link', { name: 'Home' })` |
| **Generic Div** | `<div class="item" onclick="...">` | `locator('.item')` |

#### When Generated Selectors Are Fragile

Watch out for these patterns that the recorder might generate:

```csharp
// ❌ Fragile - based on styling classes
await page.Locator(".btn-primary").ClickAsync();
await page.Locator(".form-control:nth-child(3)").FillAsync("value");

// ❌ Fragile - positional selectors
await page.Locator("div:nth-child(2) > input").FillAsync("value");
await page.Locator("table tr:nth-child(4) td:nth-child(2)").ClickAsync();

// ✅ Better alternatives
await page.GetByRole("button", new() { Name = "Submit" }).ClickAsync();
await page.Locator("input[name='firstName']").FillAsync("value");
await page.Locator("//tr[contains(., 'John Doe')]//button").ClickAsync();
```

## ✅ Copy-Paste Recording Workflow

**Great news!** Modern Playwright recorder generates code that you can **directly copy-paste** into your test methods with minimal modification.

### What Works Out-of-the-Box:
```csharp
// ✅ Copy-paste these directly from recorder
await Page.GetByTestId("first-name").FillAsync("John");
await Page.GetByTestId("submit-button").ClickAsync();
await Page.GetByRole("button", new() { Name = "Submit Form" }).ClickAsync();
await Page.GetByLabel("Email").FillAsync("john@example.com");
await Page.GetByPlaceholder("Enter your name").FillAsync("John");
```

### Simple 3-Step Process:
1. **Record** using `playwright codegen --target nunit` or `--target mstest`
2. **Copy-paste** the actions directly into your test method
3. **Add assertions** to verify expected outcomes

### Example Recording-to-Test:

**Recorded Code:**
```csharp
await Page.GotoAsync("https://localhost:7294/Contact");
await Page.GetByTestId("first-name").FillAsync("John");
await Page.GetByTestId("email").FillAsync("john@example.com");
await Page.GetByTestId("submit-button").ClickAsync();
```

**Final Test (just wrap and add assertions):**
```csharp
[Test]
public async Task SubmitContactForm_ShouldWork()
{
    // Copy-pasted recorded actions
    await Page.GotoAsync("https://localhost:7294/Contact");
    await Page.GetByTestId("first-name").FillAsync("John");
    await Page.GetByTestId("email").FillAsync("john@example.com");
    await Page.GetByTestId("submit-button").ClickAsync();
    
    // Add verification
    await Expect(Page.GetByTestId("success-message")).ToBeVisibleAsync();
}
```

This makes test creation much faster - record, copy-paste, add assertions, done!

### Recording Best Practices

1. **Use data-testid attributes**: All interactive elements in the demo app have `data-testid` attributes for reliable element selection.

2. **Record in logical chunks**: Don't record entire user flows in one go. Break them into smaller, focused test scenarios.

3. **Clean up generated code**: Recorded code often needs refinement:
   - Remove unnecessary waits
   - Add meaningful assertions
   - Group related actions
   - Add descriptive comments

4. **Use Page Object Model**: For complex applications, organize your selectors and actions into page objects.

## Alternative Selector Strategies for Legacy Applications

When working with legacy applications where you **cannot add `data-testid` attributes**, Playwright provides several robust alternatives for reliable element selection.

### 1. CSS Selectors with Existing Attributes

Use existing HTML attributes like `name`, `id`, `class`, or `type`:

```csharp
// Using name attribute (common in forms)
await Page.FillAsync("input[name='firstName']", "John");
await Page.FillAsync("select[name='country']", "US");

// Using ID attribute (most stable when available)
await Page.FillAsync("#firstName", "John");
await Page.ClickAsync("#submitButton");

// Using class names
await Page.FillAsync(".first-name-input", "John");
await Page.ClickAsync(".submit-btn");

// Using input type
await Page.FillAsync("input[type='email']", "john@example.com");
await Page.CheckAsync("input[type='checkbox']");
```

### 2. Text-Based Selectors

Use visible text content to locate elements:

```csharp
// Button text
await Page.ClickAsync("button:has-text('Submit')");
await Page.ClickAsync("text=Submit");
await Page.ClickAsync("text='Contact Us'"); // Exact match

// Link text
await Page.ClickAsync("text=Home");
await Page.ClickAsync("a:has-text('Products')");

// Labels and form fields
await Page.FillAsync("input:below(label:has-text('First Name'))", "John");
await Page.FillAsync("input:right-of(label:has-text('Email'))", "john@example.com");

// Partial text matching
await Page.ClickAsync("button:has-text('Save')"); // Matches "Save", "Save Changes", etc.
```

### 3. Structural/Hierarchical Selectors

Navigate the DOM structure using parent-child relationships:

```csharp
// Parent-child relationships
await Page.FillAsync("form >> input[type='email']", "john@example.com");
await Page.ClickAsync(".contact-form >> button[type='submit']");

// Sibling relationships
await Page.FillAsync("label:has-text('Email') + input", "john@example.com");
await Page.ClickAsync("input[name='agree'] ~ button");

// Nth child selectors
await Page.FillAsync("form input:nth-child(1)", "John");
await Page.ClickAsync("table tr:nth-child(2) td:nth-child(3)");

// Complex nested selectors
await Page.FillAsync(".form-group:has(label:has-text('Email')) input", "john@example.com");
```

### 4. Label-Based Selection

Associate form inputs with their labels:

```csharp
// Using label text to find associated input
await Page.FillAsync("label:has-text('First Name') >> input", "John");
await Page.FillAsync("text=Email Address >> input", "john@example.com");

// For radio buttons and checkboxes
await Page.CheckAsync("label:has-text('I agree to terms') >> input");
await Page.CheckAsync("text=Subscribe to newsletter >> input[type='checkbox']");
```

### 5. Placeholder and ARIA-Based Selection

```csharp
// Using placeholder text
await Page.FillAsync("input[placeholder='Enter your email']", "john@example.com");
await Page.FillAsync("textarea[placeholder='Your message here']", "Hello world");

// Using ARIA labels and roles
await Page.ClickAsync("input[aria-label='First Name']");
await Page.FillAsync("input[aria-describedby='email-help']", "john@example.com");

// ARIA roles
await Page.ClickAsync("role=button[name='Submit']");
await Page.FillAsync("role=textbox[name='Email']", "john@example.com");
await Page.ClickAsync("role=link[name='Contact']");
```

### 6. XPath Selectors

For complex scenarios requiring advanced navigation:

```csharp
// Basic XPath
await Page.FillAsync("//input[@name='firstName']", "John");
await Page.ClickAsync("//button[contains(text(), 'Submit')]");

// Advanced XPath with text content
await Page.FillAsync("//label[text()='Email']/following-sibling::input", "john@example.com");
await Page.ClickAsync("//tr[contains(., 'John Doe')]//button[text()='Edit']");

// XPath with multiple conditions
await Page.FillAsync("//input[@type='text' and @name='firstName']", "John");
```

### 7. Advanced Filtering Techniques

```csharp
// Filter by text content
var row = Page.Locator("table tr").Filter(new() { HasText = "John Doe" });
await row.Locator("button").ClickAsync();

// Filter by attribute
var activeTab = Page.Locator(".tab").Filter(new() { HasClass = "active" });
await activeTab.ClickAsync();

// Chain filters
var specificProduct = Page.Locator(".product-item")
    .Filter(new() { HasText = "Laptop" })
    .Filter(new() { HasClass = "in-stock" });
await specificProduct.Locator("button:has-text('Add to Cart')").ClickAsync();
```

### Selector Priority for Legacy Applications

**Recommended priority order:**
1. **Existing IDs** - Most stable when available
2. **Name attributes** - Common and stable in forms
3. **ARIA labels/roles** - Accessibility-friendly and semantic
4. **Text content** - Visible to users, good for buttons/links
5. **CSS classes** - May change with styling updates
6. **XPath** - Powerful but can be brittle

### Making Selectors More Robust

```csharp
// Combine multiple attributes for better stability
await Page.FillAsync("input[name='email'][type='email']", "john@example.com");
await Page.ClickAsync("button[type='submit'][class*='primary']");

// Use partial matches for dynamic content
await Page.ClickAsync("button[class*='submit-btn']");
await Page.FillAsync("input[id*='firstName']", "John");

// Wait strategies for dynamic content
await Page.WaitForSelectorAsync("input[name='firstName']");
await Page.Locator("input[name='firstName']").WaitForAsync();

// Handle dynamic tables and lists
var tableRow = Page.Locator("table tr").Filter(new() { HasText = "Product A" });
await tableRow.Locator("td button").ClickAsync();
```

### Example: Converting from data-testid to Legacy-Friendly Selectors

**Before (using data-testid):**
```csharp
await Page.FillAsync("[data-testid='first-name']", "John");
await Page.FillAsync("[data-testid='email']", "john@example.com");
await Page.ClickAsync("[data-testid='submit-button']");
```

**After (legacy-friendly alternatives):**
```csharp
// Option 1: Use name attributes
await Page.FillAsync("input[name='firstName']", "John");
await Page.FillAsync("input[name='email']", "john@example.com");
await Page.ClickAsync("button[type='submit']");

// Option 2: Use label-based selection
await Page.FillAsync("label:has-text('First Name') >> input", "John");
await Page.FillAsync("label:has-text('Email') >> input", "john@example.com");
await Page.ClickAsync("button:has-text('Submit')");

// Option 3: Use ARIA roles
await Page.FillAsync("role=textbox[name='First Name']", "John");
await Page.FillAsync("role=textbox[name='Email']", "john@example.com");
await Page.ClickAsync("role=button[name='Submit']");
```

### Testing Legacy Selector Strategies

Create a helper method to test selector robustness:

```csharp
public async Task TestSelectorRobustness(string selector, string action = "click")
{
    var element = Page.Locator(selector);
    await Expect(element).ToBeVisibleAsync();
    
    if (action == "click")
        await element.ClickAsync();
    else if (action == "fill")
        await element.FillAsync("test value");
        
    Console.WriteLine($"Selector '{selector}' is working correctly");
}
```

These alternative strategies ensure your tests remain reliable even when working with legacy applications that cannot be modified to include test-specific attributes.

## Demo Application Features

### 1. Home Page (`/`)
- Navigation cards to different test scenarios
- Demonstrates basic element interaction and navigation

### 2. Contact Form (`/Contact`)
**Test Scenarios:**
- Form validation (required fields)
- Text input, email validation
- Dropdown selection
- Textarea input
- Checkbox interaction
- Form submission and success message

**Key Elements:**
- `[data-testid='first-name']` - First name input
- `[data-testid='last-name']` - Last name input
- `[data-testid='email']` - Email input
- `[data-testid='subject']` - Subject dropdown
- `[data-testid='message']` - Message textarea
- `[data-testid='newsletter']` - Newsletter checkbox
- `[data-testid='submit-button']` - Submit button
- `[data-testid='success-message']` - Success message (appears after submission)

### 3. Products Page (`/Products`)
**Test Scenarios:**
- Product search functionality
- Category filtering
- Add to cart interactions
- Dynamic cart updates
- Product grid display

**Key Elements:**
- `[data-testid='search-input']` - Search input
- `[data-testid='search-button']` - Search button
- `[data-testid='category-filter']` - Category dropdown
- `[data-testid='add-to-cart-{n}']` - Add to cart buttons (n = 1-6)
- `[data-testid='cart-summary']` - Cart summary section
- `[data-testid='cart-count']` - Cart item count

### 4. Interactive Elements (`/Interactive`)
**Test Scenarios:**
- Modal dialogs
- JavaScript alerts and confirms
- Range sliders
- Color picker
- Date picker
- Toggle switches
- Dynamic content loading
- Tab navigation
- Drag and drop

**Key Elements:**
- `[data-testid='open-modal-button']` - Open modal button
- `[data-testid='test-modal']` - Modal dialog
- `[data-testid='alert-button']` - Show alert button
- `[data-testid='range-slider']` - Range input
- `[data-testid='slider-value']` - Slider value display
- `[data-testid='tab-{n}']` - Tab buttons (n = 1-3)
- `[data-testid='draggable-item-1']` - Draggable element
- `[data-testid='drop-zone-{n}']` - Drop zones (n = 1-2)

### 5. Registration Form (`/Register`)
**Test Scenarios:**
- Multi-step form completion
- Various input types (text, date, select, checkbox, radio)
- Form validation
- Multi-checkbox selection
- Terms acceptance

**Key Elements:**
- Personal info: `[data-testid='first-name']`, `[data-testid='last-name']`, `[data-testid='date-of-birth']`
- Contact info: `[data-testid='email']`, `[data-testid='phone']`, `[data-testid='address']`
- Account setup: `[data-testid='username']`, `[data-testid='password']`
- Preferences: `[data-testid='interest-{type}']`, `[data-testid='accept-terms']`

## Sample Test Recording Workflow

### Recording a Contact Form Test

1. **Start recording**:
   ```powershell
   playwright codegen https://localhost:7294
   ```

2. **Navigate and interact**:
   - Click "Go to Contact Form"
   - Fill in the form fields
   - Submit the form
   - Verify the success message

3. **Generated code example**:
   ```csharp
   await page.GotoAsync("https://localhost:7294/");
   await page.GetByTestId("contact-link").ClickAsync();
   await page.GetByTestId("first-name").FillAsync("John");
   await page.GetByTestId("last-name").FillAsync("Doe");
   await page.GetByTestId("email").FillAsync("john@example.com");
   await page.GetByTestId("submit-button").ClickAsync();
   await Expect(page.GetByTestId("success-message")).ToBeVisibleAsync();
   ```

4. **Refine the test**:
   ```csharp
   [Test]
   public async Task FillContactForm_ShouldShowSuccessMessage()
   {
       // Navigate to contact page
       await Page.GotoAsync("https://localhost:7294/Contact");
       
       // Fill out the form - copy-paste recorded selectors directly
       await Page.GetByTestId("first-name").FillAsync("John");
       await Page.GetByTestId("last-name").FillAsync("Doe");
       await Page.GetByTestId("email").FillAsync("john.doe@example.com");
       await Page.GetByTestId("subject").SelectOptionAsync("general");
       await Page.GetByTestId("message").FillAsync("Test message");
       
       // Submit and verify
       await Page.GetByTestId("submit-button").ClickAsync();
       await Expect(Page.GetByTestId("success-message")).ToBeVisibleAsync();
   }
   ```

## Test Organization

### Test Structure
Each test class inherits from `PlaywrightTestBase` which provides:
- Common setup/teardown
- Browser context configuration
- Helper methods like `NavigateToPageAsync()`

### Test Categories
1. **Functional Tests** - Test business logic and user workflows
2. **UI Tests** - Test visual elements and interactions
3. **Integration Tests** - Test component interactions
4. **Accessibility Tests** - Test keyboard navigation, screen readers, etc.

### Assertion Patterns
```csharp
// Visibility assertions
await Expect(Page.Locator("[data-testid='element']")).ToBeVisibleAsync();
await Expect(Page.Locator("[data-testid='element']")).Not.ToBeVisibleAsync();

// Text content assertions
await Expect(Page.Locator("[data-testid='element']")).ToHaveTextAsync("Expected text");
await Expect(Page.Locator("[data-testid='element']")).ToContainTextAsync("Partial text");

// Value assertions
await Expect(Page.Locator("[data-testid='input']")).ToHaveValueAsync("Expected value");

// Attribute assertions
await Expect(Page.Locator("[data-testid='element']")).ToHaveAttributeAsync("required", "");

// Count assertions
await Expect(Page.Locator(".product-item")).ToHaveCountAsync(6);
```

## Running Tests in Different Browsers

### Configuration
Tests can run in different browsers by modifying the test configuration:

```csharp
[TestFixture]
public class CrossBrowserTests : PageTest
{
    [Test]
    [TestCase("chromium")]
    [TestCase("firefox")]
    [TestCase("webkit")]
    public async Task TestInBrowser(string browserName)
    {
        // Test logic here
    }
}
```

### Command Line Options
```powershell
# Run tests in specific browser
dotnet test -- Playwright.BrowserName=chromium

# Run tests headful (visible browser)
dotnet test -- Playwright.LaunchOptions.Headless=false

# Run tests with slow motion
dotnet test -- Playwright.LaunchOptions.SlowMo=1000
```

## Debugging Tests

### Visual Debugging
1. **Run tests headful**:
   ```csharp
   await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
   { 
       Headless = false, 
       SlowMo = 1000 
   });
   ```

2. **Use Playwright Inspector**:
   ```csharp
   await Page.PauseAsync(); // Pauses execution and opens inspector
   ```

### Automatic Screenshot Capture

This project includes built-in screenshot capture functionality in the `PlaywrightTestBase` class:

#### Automatic Screenshots on Test Failure
Screenshots are automatically captured when any test fails, including:
- **Full-page screenshot** (PNG format)
- **Complete page HTML** content for debugging
- **Organized file naming** with test name and timestamp

Files are saved to: `bin/Debug/net9.0/screenshots/`

Example filenames:
- `FAILED_ContactFormTest_20250822_161514.png`
- `FAILED_ContactFormTest_20250822_161514.html`

#### Manual Debug Screenshots
You can manually capture screenshots during test execution:

```csharp
[Test]
public async Task MyTest()
{
    await NavigateToPageAsync("Contact");
    
    // Capture a debug screenshot at any point
    await CaptureDebugScreenshotAsync("after_page_load");
    
    // Fill out form
    await Page.GetByTestId("first-name").FillAsync("Test");
    
    // Capture another screenshot
    await CaptureDebugScreenshotAsync("form_filled");
    
    // Continue with test...
}
```

Debug screenshots are named: `DEBUG_TestName_ScreenshotName_Timestamp.png`

#### Benefits
- **No manual configuration** - works automatically for all tests
- **Full context** - captures entire page state and HTML
- **Easy debugging** - immediately see what the page looked like when test failed
- **Timestamp tracking** - never overwrite previous failure screenshots

### Video Recording

This project includes **smart video recording** for efficient debugging:

#### Automatic Video Recording (Failed Tests Only)
Every test starts video recording, but videos are **only saved when tests fail**:
- **Always recording**: Every test records video during execution
- **Smart saving**: Videos are only kept when tests fail, saving disk space
- **Automatic cleanup**: Passed test videos are automatically deleted
- **Format**: WebM video files at 1280x720 resolution
- **Storage**: `bin/Debug/net9.0/videos/`
- **Naming**: `FAILED_TestName_Timestamp.webm`

#### Benefits of This Approach
- **💾 Saves disk space**: No storage wasted on successful test videos
- **🔍 Debug when needed**: Full video available for every test failure
- **⚡ Zero configuration**: Automatic recording and cleanup
- **📊 Focus on problems**: Only failed tests create files to review

#### Example Behavior
```csharp
[Test]
public async Task MyPassingTest()
{
    // Video recording starts automatically
    await NavigateToPageAsync("Contact");
    await Page.GetByTestId("submit-button").ClickAsync();
    // ✅ Test passes - video is automatically deleted
    // Console: "✅ Test passed - Video recording discarded (not saved)"
}

[Test] 
public async Task MyFailingTest()
{
    // Video recording starts automatically
    await NavigateToPageAsync("Contact");
    await Page.GetByTestId("nonexistent-element").ClickAsync();
    // ❌ Test fails - video is saved for debugging
    // Console: "❌ Test failed - Video recording saved to: FAILED_MyFailingTest_20250822_163329.webm"
}
```

#### Output Messages
- **Passed tests**: `✅ Test passed - Video recording discarded (not saved)`
- **Failed tests**: `❌ Test failed - Video recording saved to: [filename]`

#### Configuration
Video recording is configured in `PlaywrightTestBase.cs`:
```csharp
public override BrowserNewContextOptions ContextOptions()
{
    return new BrowserNewContextOptions()
    {
        RecordVideoDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "videos"),
        RecordVideoSize = new RecordVideoSize() { Width = 1280, Height = 720 }
    };
}
```

#### Customizing Video and Screenshot Folders

You can specify custom folders for videos and screenshots by overriding the folder properties:

```csharp
[TestFixture]
public class CustomVideoFolderTests : PlaywrightTestBase
{
    public CustomVideoFolderTests()
    {
        // Custom folder names (relative to test output directory)
        VideosFolder = "custom-videos";
        ScreenshotsFolder = "custom-screenshots";
    }
    
    // Your tests here...
}
```

**Advanced folder configurations:**

```csharp
// Date-based organization
public class DateBasedTests : PlaywrightTestBase
{
    public DateBasedTests()
    {
        var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
        VideosFolder = Path.Combine("test-results", dateFolder, "videos");
        ScreenshotsFolder = Path.Combine("test-results", dateFolder, "screenshots");
    }
}

// Absolute paths for specific locations
public class AbsolutePathTests : PlaywrightTestBase
{
    public AbsolutePathTests()
    {
        VideosFolder = @"C:\TestResults\Videos";
        ScreenshotsFolder = @"C:\TestResults\Screenshots";
        
        // Ensure directories exist
        Directory.CreateDirectory(VideosFolder);
        Directory.CreateDirectory(ScreenshotsFolder);
    }
}
```

**Default locations:**
- Videos: `bin/Debug/net9.0/videos/`
- Screenshots: `bin/Debug/net9.0/screenshots/`

### CI/CD Integration (Azure DevOps)

The test base class automatically detects Azure DevOps environments and organizes artifacts optimally:

**Azure DevOps Pipeline Integration:**
```yaml
# Key pipeline steps for artifact management
- task: DotNetCoreCLI@2
  displayName: 'Run Playwright Tests'
  inputs:
    command: 'test'
    arguments: '--logger "trx" --results-directory $(Agent.TempDirectory)/TestResults'
  continueOnError: true

- task: PublishPipelineArtifact@1
  displayName: 'Publish test artifacts'
  inputs:
    targetPath: 'PlaywrightDemo.Tests/bin/Release/net9.0'
    artifactName: 'playwright-artifacts-$(Build.BuildNumber)'
  condition: always()
```

**Artifact Management Script:**
```powershell
# Organize artifacts locally (mimics Azure DevOps structure)
.\manage-artifacts.ps1 -BuildNumber "local-build" -OpenReport
```

**Best Practices for CI/CD:**
- ✅ Only failed test videos are saved (storage optimized)
- ✅ Artifacts organized by build number
- ✅ HTML reports generated automatically
- ✅ Azure Blob Storage integration for long-term retention
- ✅ Test results integrate with Azure DevOps Test tab

**Storage Locations:**
1. **Pipeline Artifacts** (Recommended): Available for 30 days, downloadable from pipeline
2. **Azure Blob Storage** (Long-term): For extended retention and sharing
3. **Azure Container Registry** (Advanced): For containerized test environments

## Best Practices for Test Recording

### 1. Element Selection Strategy
- **Prefer data-testid**: Most reliable for test automation
- **Avoid CSS selectors**: Fragile and change frequently
- **Use semantic HTML**: When possible, select by role or text

### 2. Wait Strategies
```csharp
// Wait for element to be visible
await Page.WaitForSelectorAsync("[data-testid='element']");

// Wait for network to be idle
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Wait for specific condition
await Page.WaitForFunctionAsync("() => document.readyState === 'complete'");
```

### 3. Data Management
- Use unique test data to avoid conflicts
- Clean up test data after tests
- Consider using factories for test data generation

### 4. Error Handling
```csharp
[Test]
public async Task TestWithErrorHandling()
{
    try
    {
        await Page.GotoAsync("https://localhost:7294/");
        // Test logic
    }
    catch (PlaywrightException ex)
    {
        await Page.ScreenshotAsync(new() { Path = "error.png" });
        throw;
    }
}
```

## Continuous Integration

### GitHub Actions Example
```yaml
name: Playwright Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Install Playwright
      run: |
        cd PlaywrightDemo.Tests
        dotnet build
        pwsh bin/Debug/net9.0/playwright.ps1 install --with-deps
    - name: Run tests
      run: dotnet test --no-build --verbosity normal
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure the web app is running on the expected port
2. **Browser installation**: Run `playwright install` if browsers are missing
3. **Timing issues**: Add appropriate waits for dynamic content
4. **Element not found**: Verify selectors and element visibility

### Debug Commands
```powershell
# Check Playwright installation
pwsh -File bin/Debug/net9.0/playwright.ps1 install --help

# Run tests with detailed output
dotnet test --logger:console --verbosity detailed

# Run single test with debugging
dotnet test --filter "TestName" --logger:console
```

## Next Steps

1. **Extend test coverage** - Add more test scenarios for edge cases
2. **Page Object Model** - Refactor tests to use page objects for better maintainability
3. **API testing** - Add Playwright API testing capabilities
4. **Performance testing** - Use Playwright for performance monitoring
5. **Accessibility testing** - Add automated accessibility checks
6. **Visual regression testing** - Compare screenshots for visual changes

## Resources

- [Playwright Documentation](https://playwright.dev/dotnet/)
- [Playwright Test Generator](https://playwright.dev/dotnet/codegen)
- [Best Practices Guide](https://playwright.dev/dotnet/best-practices)
- [Debugging Guide](https://playwright.dev/dotnet/debug)

This demo provides a comprehensive foundation for understanding Playwright test recording and automation. The sample application covers most common web interactions, and the test suite demonstrates various testing patterns and best practices.
