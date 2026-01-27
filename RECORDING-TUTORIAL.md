# Quick Tutorial: Recording Your First Playwright Test

## 🎯 Goal
Learn how to record a Playwright test by interacting with the demo application.

## 📋 Prerequisites
1. Web application is running (`.\quickstart.ps1 run-app`)
2. Playwright browsers are installed (`.\quickstart.ps1 setup`)

## 🎬 Step-by-Step Recording Tutorial

### Step 1: Start Recording
```powershell
# Option 1: Using global Playwright (default is NUnit)
playwright codegen http://localhost:5275

# Option 2: Using local Playwright (default is NUnit)
cd PlaywrightDemo.Tests
pwsh -File bin/Debug/net9.0/playwright.ps1 codegen http://localhost:5275

# Option 3: Explicitly specify NUnit target
playwright codegen --target nunit http://localhost:5275

# Option 4: Using local Playwright with explicit NUnit target
cd PlaywrightDemo.Tests
pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target nunit http://localhost:5275

# Option 5: Generate MSTest-compatible code (instead of default NUnit)
playwright codegen --target mstest http://localhost:5275

# Option 6: Using local Playwright with MSTest target
cd PlaywrightDemo.Tests
pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target mstest http://localhost:5275
```

### Step 2: Record a Contact Form Test

1. **Browser opens with Playwright Inspector**
   - You'll see two windows: the browser and the Playwright Inspector

2. **Navigate to Contact Form**
   - Click "Go to Contact Form" button
   - Notice the Inspector generates: `await page.getByTestId('contact-link').click();`

3. **Fill the Form**
   - First Name: Type "John"
   - Last Name: Type "Doe"  
   - Email: Type "john.doe@example.com"
   - Subject: Select "General Inquiry"
   - Message: Type "This is a test message"
   - Check the newsletter checkbox

4. **Submit the Form**
   - Click "Send Message"
   - Wait for success message to appear

5. **Copy Generated Code**
   - The Inspector shows all the generated code
   - Copy it to create your test

### Step 3: Convert to Proper Test

Here's what the raw generated code looks like when using `--target nunit`:
```csharp
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    [Test]
    public async Task MyTest()
    {
        await Page.GotoAsync("http://localhost:5275/");
        await Page.GetByTestId("contact-link").ClickAsync();
        await Page.GetByTestId("first-name").FillAsync("John");
        await Page.GetByTestId("last-name").FillAsync("Doe");
        await Page.GetByTestId("email").FillAsync("john.doe@example.com");
        await Page.GetByTestId("subject").SelectOptionAsync("general");
        await Page.GetByTestId("message").FillAsync("This is a test message");
        await Page.GetByTestId("newsletter").CheckAsync();
        await Page.GetByTestId("submit-button").ClickAsync();
    }
}
```

Here's how to convert it to a proper test:

**For NUnit (default):**
```csharp
[Test]
[Description("Test contact form submission")]
public async Task SubmitContactForm_ShouldShowSuccessMessage()
{
    // Navigate to contact page
    await Page.GotoAsync("http://localhost:5275/Contact");
    
    // Fill out the form - using recorded selectors directly
    await Page.GetByTestId("first-name").FillAsync("John");
    await Page.GetByTestId("last-name").FillAsync("Doe");
    await Page.GetByTestId("email").FillAsync("john.doe@example.com");
    await Page.GetByTestId("subject").SelectOptionAsync("general");
    await Page.GetByTestId("message").FillAsync("This is a test message");
    await Page.GetByTestId("newsletter").CheckAsync();
    
    // Submit the form
    await Page.GetByTestId("submit-button").ClickAsync();
    
    // Verify success message appears
    await Expect(Page.GetByTestId("success-message")).ToBeVisibleAsync();
    await Expect(Page.GetByTestId("success-message")).ToContainTextAsync("Success!");
}
```

**For MSTest (when using --target mstest):**
```csharp
[TestMethod]
[Description("Test contact form submission")]
public async Task SubmitContactForm_ShouldShowSuccessMessage()
{
    // Navigate to contact page
    await Page.GotoAsync("http://localhost:5275/Contact");
    
    // Fill out the form - using recorded selectors directly
    await Page.GetByTestId("first-name").FillAsync("John");
    await Page.GetByTestId("last-name").FillAsync("Doe");
    await Page.GetByTestId("email").FillAsync("john.doe@example.com");
    await Page.GetByTestId("subject").SelectOptionAsync("general");
    await Page.GetByTestId("message").FillAsync("This is a test message");
    await Page.GetByTestId("newsletter").CheckAsync();
    
    // Submit the form
    await Page.GetByTestId("submit-button").ClickAsync();
    
    // Verify success message appears
    await Expect(Page.GetByTestId("success-message")).ToBeVisibleAsync();
    await Expect(Page.GetByTestId("success-message")).ToContainTextAsync("Success!");
}
```

## ✅ **Copy-Paste Friendly Recording**

**Great news!** You can directly copy-paste most of the recorded code into your test methods. The main things you need to do are:

1. **Wrap in a proper test method** with `[Test]` or `[TestMethod]` attribute
2. **Add meaningful assertions** at the end
3. **Optionally refactor navigation** (replace direct GotoAsync with helper methods)

### What You Can Copy-Paste Directly:
```csharp
// ✅ These work exactly as recorded
await Page.GetByTestId("first-name").FillAsync("John");
await Page.GetByTestId("submit-button").ClickAsync();
await Page.GetByRole("button", { name: "Save" }).ClickAsync();
await Page.GetByLabel("Email").FillAsync("test@example.com");
```

### What You Might Want to Refactor:
```csharp
// Recorded navigation (works but could be improved)
await Page.GotoAsync("http://localhost:5275/Contact");

// Refactored navigation (more maintainable)
await NavigateToPageAsync("Contact");
```

## 🎯 More Recording Scenarios

### Recording Product Search
1. **Navigate to Products**
   - Click "Browse Products"

2. **Test Search**
   - Type "headphones" in search box
   - Click search button
   - Verify results show only headphones

3. **Test Filtering**
   - Select "Electronics" from category filter
   - Verify only electronics products are visible

4. **Test Cart Operations**
   - Click "Add to Cart" on first product
   - Verify cart appears and shows count
   - Add another product
   - Verify count updates

### Recording Interactive Elements
1. **Navigate to Interactive Page**
   - Click "Try Interactive Features"

2. **Test Modal**
   - Click "Open Modal"
   - Type text in modal input
   - Close modal with X button

3. **Test Dynamic Content**
   - Click "Load Content"
   - Verify content appears with timestamp

4. **Test Drag & Drop**
   - Drag the blue element from zone 1 to zone 2
   - Verify element moved

## 🎭 How Recorder Chooses Selectors

### Understanding Playwright's Selector Priority

When recording, Playwright automatically chooses the **best available selector** based on a priority order. This is especially important when working with legacy applications that lack `data-testid` attributes.

#### Selector Priority (High to Low):

1. **`data-testid` attributes** (Highest Priority)
   ```html
   <button data-testid="submit-btn">Submit</button>
   ```
   **Generates:** `await page.getByTestId('submit-btn').click();`

2. **Accessible roles and names** (Semantic & Reliable)
   ```html
   <button type="submit">Submit Form</button>
   <input aria-label="Email Address" type="email">
   ```
   **Generates:**
   ```csharp
   await page.getByRole('button', { name: 'Submit Form' }).click();
   await page.getByRole('textbox', { name: 'Email Address' }).fill('test@example.com');
   ```

3. **Labels and text content**
   ```html
   <label for="email">Email</label><input id="email">
   <a href="/home">Go Home</a>
   ```
   **Generates:**
   ```csharp
   await page.getByLabel('Email').fill('test@example.com');
   await page.getByRole('link', { name: 'Go Home' }).click();
   ```

4. **Placeholder text**
   ```html
   <input type="text" placeholder="Enter your name">
   ```
   **Generates:** `await page.getByPlaceholder('Enter your name').fill('John');`

5. **ID attributes**
   ```html
   <button id="submit-button">Submit</button>
   ```
   **Generates:** `await page.locator('#submit-button').click();`

6. **CSS selectors** (Last Resort - Less Reliable)
   ```html
   <button class="btn btn-primary">Submit</button>
   ```
   **Generates:** `await page.locator('.btn-primary').click();`

### 🎯 Recording Without data-testid

Let's see what happens when recording on elements **without** `data-testid`:

#### Example: Legacy Form
```html
<form class="contact-form">
  <label for="firstName">First Name</label>
  <input id="firstName" name="firstName" type="text" required>
  
  <label>Email Address</label>
  <input name="email" type="email" placeholder="Enter email" class="form-control">
  
  <select name="country" class="form-select">
    <option value="">Select country...</option>
    <option value="US">United States</option>
  </select>
  
  <button type="submit" class="btn btn-primary">Submit Form</button>
  <button type="button" class="cancel-btn">Cancel</button>
</form>
```

#### What Recorder Actually Generates:
```csharp
// ✅ Good - Uses label association
await page.getByLabel("First Name").fill("John");

// ✅ Good - Uses placeholder text  
await page.getByPlaceholder("Enter email").fill("john@example.com");

// ✅ Good - Uses name attribute with role
await page.getByRole("combobox", { name: "country" }).selectOption("US");

// ✅ Good - Uses button role with text
await page.getByRole("button", { name: "Submit Form" }).click();

// ⚠️ Less reliable - Falls back to CSS class
await page.locator(".cancel-btn").click();
```

#### When Recorder Uses CSS Selectors

**HTML with minimal semantic attributes:**
```html
<div class="form-group">
  <input type="text" class="form-control legacy-input">
  <div class="btn-group">
    <button class="btn save-btn">Save</button>
    <button class="btn-outline delete-btn">Delete</button>
  </div>
</div>
```

**Generated (less reliable):**
```csharp
await page.locator(".legacy-input").fill("value");
await page.locator(".save-btn").click();
await page.locator(".delete-btn").click();
```

**How to improve after recording:**
```csharp
// Better alternatives using structure/text
await page.locator("input[type='text']").fill("value");
await page.getByRole("button", { name: "Save" }).click();
await page.getByRole("button", { name: "Delete" }).click();
```

### 🔧 Recording Tips

### 1. **Choose the Right Test Framework Target**

Playwright codegen can generate code for different test frameworks:

```powershell
# Generate NUnit tests (default)
playwright codegen http://localhost:5275

# Generate NUnit tests (explicit)
playwright codegen --target nunit http://localhost:5275

# Generate MSTest tests
playwright codegen --target mstest http://localhost:5275

# Generate xUnit tests
playwright codegen --target xunit http://localhost:5275

# Generate plain C# (no test framework)
playwright codegen --target csharp http://localhost:5275

# Using local Playwright with NUnit (default)
cd PlaywrightDemo.Tests
pwsh -File bin/Debug/net9.0/playwright.ps1 codegen http://localhost:5275

# Using local Playwright with explicit NUnit target
cd PlaywrightDemo.Tests
pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target nunit http://localhost:5275

# Using local Playwright with MSTest
cd PlaywrightDemo.Tests
pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target mstest http://localhost:5275
```

**Key Differences:**

| Framework | Attribute | Base Class | Assertions |
|-----------|-----------|------------|------------|
| **NUnit** | `[Test]` | `PlaywrightTest` | `await Expect()` |
| **MSTest** | `[TestMethod]` | `PlaywrightTest` | `await Expect()` |
| **xUnit** | `[Fact]` | `PlaywrightTest` | `await Expect()` |

### 2. Use Descriptive Actions
- Instead of generic clicks, add context
- Use meaningful test data
- Add verification steps

### 2. Handle Timing Issues
```csharp
// Wait for element to appear
await Page.WaitForSelectorAsync("[data-testid='element']");

// Wait for page to load completely
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Wait for specific text
await Page.WaitForFunctionAsync("() => document.querySelector('[data-testid=\"element\"]').textContent.includes('Expected')");
```

### 3. Add Better Assertions
```csharp
// Instead of just checking visibility
await Expect(Page.Locator("[data-testid='message']")).ToBeVisibleAsync();

// Also check content
await Expect(Page.Locator("[data-testid='message']")).ToHaveTextAsync("Expected message");
await Expect(Page.Locator("[data-testid='message']")).ToContainTextAsync("Success");
```

### 4. Group Related Actions
```csharp
// Group form filling
private async Task FillContactFormAsync(string firstName, string lastName, string email)
{
    await Page.FillAsync("[data-testid='first-name']", firstName);
    await Page.FillAsync("[data-testid='last-name']", lastName);
    await Page.FillAsync("[data-testid='email']", email);
}

// Use in test
await FillContactFormAsync("John", "Doe", "john@example.com");
```

## 🐛 Common Recording Issues

### Issue 1: Element Not Found
**Problem**: `TimeoutError: Waiting for selector "[data-testid='element']" failed`

**Solutions**:
- Verify the element exists and is visible
- Add wait for the element to appear
- Check if element is in an iframe
- Verify correct selector syntax

### Issue 2: Flaky Tests
**Problem**: Tests sometimes pass, sometimes fail

**Solutions**:
- Add explicit waits instead of fixed delays
- Wait for network to be idle before interactions
- Use more specific selectors
- Handle dynamic content properly

### Issue 3: Generated Code Too Long
**Problem**: Recorded test is very long and hard to maintain

**Solutions**:
- Break into smaller, focused tests
- Extract common actions into helper methods
- Use Page Object Model pattern
- Remove unnecessary steps

### Issue 4: **Fragile Selectors in Legacy Apps**
**Problem**: Recorded selectors break when CSS changes

**Example fragile selectors:**
```csharp
await page.locator(".btn-primary").click(); // ❌ CSS class may change
await page.locator("div:nth-child(2) input").fill("text"); // ❌ Position may change
await page.locator("#generated-id-123").click(); // ❌ Dynamic IDs
```

**Solutions:**
```csharp
// ✅ Use semantic selectors
await page.getByRole("button", { name: "Submit" }).click();
await page.getByLabel("First Name").fill("text");
await page.locator("input[name='firstName']").fill("text");

// ✅ Use stable attributes
await page.locator("button[type='submit']").click();
await page.locator("input[data-field='email']").fill("text");
```

### Issue 5: **Multiple Elements Match**
**Problem**: `Error: strict mode violation: multiple elements`

**Common in legacy apps:**
```html
<button class="btn">Save</button>
<button class="btn">Save Draft</button>
```

**Solutions:**
```csharp
// ❌ Ambiguous - matches both buttons
await page.locator(".btn").click();

// ✅ Be more specific
await page.getByRole("button", { name: "Save", exact: true }).click();
await page.getByRole("button", { name: "Save Draft" }).click();

// ✅ Use context
await page.locator(".save-section .btn").click();
await page.locator("form >> button:has-text('Save')").click();
```

### Issue 6: **Dynamic Content in Legacy Apps**
**Problem**: Elements load dynamically without modern loading indicators

**Legacy HTML:**
```html
<div id="content">Loading...</div>
<!-- Content loads via old jQuery/AJAX -->
```

**Solutions:**
```csharp
// ✅ Wait for specific content
await page.waitForFunction("document.getElementById('content').innerText !== 'Loading...'");

// ✅ Wait for element with expected text
await page.waitForSelector("text=Expected Content");

// ✅ Use custom wait conditions
await page.waitForFunction(() => {
    const element = document.querySelector('#dynamic-table tr');
    return element && element.children.length > 0;
});
```

### Issue 7: **Working with Tables Without IDs**
**Problem**: Legacy data tables without identifiers

**HTML:**
```html
<table class="data-table">
  <tr><td>John Doe</td><td>Manager</td><td><button>Edit</button></td></tr>
  <tr><td>Jane Smith</td><td>Developer</td><td><button>Edit</button></td></tr>
</table>
```

**Recorder generates:**
```csharp
await page.locator("table tr:nth-child(2) button").click(); // ❌ Fragile
```

**Better approach:**
```csharp
// ✅ Find row by content, then click button
await page.locator("tr:has-text('John Doe') button").click();

// ✅ Use XPath for complex navigation
await page.locator("//tr[contains(., 'Jane Smith')]//button[text()='Edit']").click();

// ✅ Filter approach
var row = page.locator("table tr").filter(new() { HasText = "John Doe" });
await row.locator("button").click();
```

## 📝 Practice Exercises

### Exercise 1: Record Registration Flow
1. **Choose your test framework and record**:
   ```powershell
   # For NUnit projects (default)
   playwright codegen http://localhost:5275
   
   # For NUnit projects (explicit)
   playwright codegen --target nunit http://localhost:5275
   
   # For MSTest projects
   playwright codegen --target mstest http://localhost:5275
   
   # Using local Playwright with explicit NUnit
   cd PlaywrightDemo.Tests
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target nunit http://localhost:5275
   
   # Using local Playwright with MSTest
   cd PlaywrightDemo.Tests
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target mstest http://localhost:5275
   ```

2. Navigate to Register page, fill out all form steps, submit registration, verify success message

3. **Compare generated code** and notice the differences:
   - NUnit uses `[Test]` vs MSTest uses `[TestMethod]`
   - Both support async/await patterns
   - Assertion patterns remain the same

### Exercise 2: Record Product Workflow
1. **Generate framework-specific tests**:
   ```powershell
   # For NUnit projects (default)
   playwright codegen http://localhost:5275
   
   # For NUnit projects (explicit)
   playwright codegen --target nunit http://localhost:5275
   
   # For MSTest projects
   playwright codegen --target mstest http://localhost:5275
   
   # Using local Playwright with NUnit
   cd PlaywrightDemo.Tests
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target nunit http://localhost:5275
   ```

2. Search for specific product, filter by category, add multiple items to cart, verify cart total, clear cart

### Exercise 3: Record Interactive Elements
1. **Use local Playwright with your preferred framework**:
   ```powershell
   # NUnit example (default)
   cd PlaywrightDemo.Tests
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen http://localhost:5275
   
   # NUnit example (explicit)
   cd PlaywrightDemo.Tests
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target nunit http://localhost:5275
   
   # MSTest example
   cd PlaywrightDemo.Tests
   pwsh -File bin/Debug/net9.0/playwright.ps1 codegen --target mstest http://localhost:5275
   ```

2. Test all tabs switching, modal with input, drag and drop, range slider, dynamic content loading

### Exercise 4: **Legacy App Recording Practice**

**Goal**: Practice recording on elements without `data-testid` attributes.

1. **Create a legacy test page** (or use existing legacy app):
   ```html
   <form class="registration-form">
     <input type="text" name="username" placeholder="Username">
     <input type="email" name="email" class="email-field">
     <select name="role">
       <option value="admin">Administrator</option>
       <option value="user">User</option>
     </select>
     <button type="submit">Register User</button>
   </form>
   ```

2. **Record interactions** and observe generated selectors:
   ```powershell
   playwright codegen http://localhost:5275/legacy-page
   ```

3. **Expected generated code**:
   ```csharp
   await page.getByPlaceholder("Username").fill("testuser");
   await page.locator(".email-field").fill("test@example.com");
   await page.getByRole("combobox").selectOption("admin");
   await page.getByRole("button", { name: "Register User" }).click();
   ```

4. **Improve the selectors**:
   ```csharp
   // Better alternatives for robustness
   await page.locator("input[name='username']").fill("testuser");
   await page.locator("input[name='email']").fill("test@example.com");
   await page.locator("select[name='role']").selectOption("admin");
   await page.getByRole("button", { name: "Register User" }).click();
   ```

### Exercise 5: **Selector Analysis & Improvement**

1. **Record a complex form** without `data-testid`
2. **Analyze generated selectors** for reliability
3. **Categorize selectors** as:
   - ✅ **Reliable**: Role-based, label-based, semantic
   - ⚠️ **Moderate**: ID-based, name-based
   - ❌ **Fragile**: CSS class-based, positional

4. **Refactor fragile selectors** using alternative strategies:

   **Before (fragile):**
   ```csharp
   await page.locator(".btn-primary").click();
   await page.locator("div:nth-child(3) input").fill("value");
   ```

   **After (improved):**
   ```csharp
   await page.getByRole("button", { name: "Submit" }).click();
   await page.locator("input[name='fieldName']").fill("value");
   ```

## 🎉 Next Steps

After recording your first tests:

1. **Refactor**: Clean up the generated code
2. **Organize**: Group related tests in classes
3. **Extend**: Add edge cases and error scenarios
4. **Maintain**: Update selectors when UI changes
5. **Scale**: Implement Page Object Model for larger applications

## 📚 Resources
 
- [Playwright Documentation](https://playwright.dev/dotnet/)
 - [Playwright Codegen](https://playwright.dev/dotnet/docs/codegen)
 - [Selectors / Locators Guide](https://playwright.dev/dotnet/docs/locators)
 - [Best Practices](https://playwright.dev/docs/best-practices)
 - [Debugging Tests](https://playwright.dev/dotnet/docs/debug)

Happy testing! 🎭
