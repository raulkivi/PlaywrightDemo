# 🤖 Playwright MCP Server Guide

## Overview

The [Playwright MCP server](https://github.com/microsoft/playwright-mcp) (`@playwright/mcp`) integrates Playwright browser automation with AI assistants (GitHub Copilot, Claude, etc.) through the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/). This enables AI agents to interact with a live browser, making test writing, recording, debugging, and exploratory testing dramatically faster.

> **How it works**: The MCP server exposes browser capabilities through Playwright's accessibility tree — not screenshots. This makes AI-driven automation stable, fast, and deterministic.

## 📋 Prerequisites

- **Node.js** v18 or later (`node --version`)
- **VS Code** 1.99 or later (for built-in MCP support in GitHub Copilot Agent Mode)
- Running web application (`dotnet run` in `PlaywrightDemo.WebApp/`)

## ⚙️ Setup

### 1. VS Code with GitHub Copilot (Recommended)

This repository already includes `.vscode/mcp.json`, which configures the Playwright MCP server automatically. Open the project in VS Code and GitHub Copilot will detect it.

To verify the server is active:
1. Open **GitHub Copilot Chat** (`Ctrl+Shift+I`)
2. Switch to **Agent Mode** (select `@agent` or use the dropdown)
3. Confirm **playwright** appears in the tools list

### 2. Other MCP Clients

For Claude Desktop, Cursor, or other MCP-compatible clients, add the following server configuration:

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["@playwright/mcp@latest"]
    }
  }
}
```

### 3. Manual Installation (Optional)

To install the package globally instead of using `npx`:

```bash
npm install -g @playwright/mcp
```

Then update the `args` in `.vscode/mcp.json`:
```json
{
  "servers": {
    "playwright": {
      "command": "playwright-mcp",
      "args": [],
      "type": "stdio"
    }
  }
}
```

---

## 🚀 Use Cases

### 1. AI-Driven Test Writing

Let the AI write new Playwright tests by describing what you want to test in natural language.

**Steps:**
1. Start the web application: `dotnet run --project PlaywrightDemo.WebApp`
2. Open GitHub Copilot Agent Mode in VS Code
3. Describe the test scenario

**Example prompts:**

```
Navigate to http://localhost:5275/Contact and write a Playwright NUnit test
that fills out the contact form with valid data and verifies the success message.
```

```
Open http://localhost:5275/Products, search for "headphones", then write a C# NUnit 
test that verifies search results are filtered correctly.
```

```
Go to http://localhost:5275/Register and explore all the form steps, then generate 
a complete NUnit test for the full registration flow.
```

**What the AI does:**
- Opens the browser and navigates to the page
- Inspects the accessibility tree to discover element selectors
- Generates type-safe C# test code using `GetByTestId`, `GetByRole`, or `GetByLabel`
- Adds meaningful assertions

**Expected output** (contact form example):
```csharp
[Test]
[Description("AI-generated: Contact form submission")]
public async Task ContactForm_SubmitValidData_ShowsSuccessMessage()
{
    await Page.GotoAsync("http://localhost:5275/Contact");
    await Page.GetByTestId("first-name").FillAsync("John");
    await Page.GetByTestId("last-name").FillAsync("Doe");
    await Page.GetByTestId("email").FillAsync("john.doe@example.com");
    await Page.GetByTestId("subject").SelectOptionAsync("general");
    await Page.GetByTestId("message").FillAsync("Test message");
    await Page.GetByTestId("submit-button").ClickAsync();
    await Expect(Page.GetByTestId("success-message")).ToBeVisibleAsync();
    await Expect(Page.GetByTestId("success-message")).ToContainTextAsync("Success!");
}
```

---

### 2. Test Recording with AI Assistance

Use the MCP server to record interactions and have the AI clean them up into production-ready tests.

**Steps:**
1. Ask the AI to navigate and interact with the app on your behalf
2. The AI records all actions using the live accessibility tree
3. Ask the AI to convert the interactions into a clean NUnit test

**Example workflow:**
```
Open http://localhost:5275 and navigate to the Products page.
Add "Laptop" to the cart, then add "Headphones" too.
Verify the cart shows 2 items. Generate a test for this flow.
```

**Advantage over `codegen`**: The AI can add assertions and clean up the test in one step, without the manual copy-paste step from Playwright Inspector.

---

### 3. Exploratory Testing

Ask the AI to freely explore the application and identify potential issues or gaps in test coverage.

**Example prompts:**

```
Explore http://localhost:5275 and list all interactive elements on each page.
Identify which elements lack data-testid attributes.
```

```
Navigate through all pages of http://localhost:5275 and try edge cases:
empty form submissions, invalid email formats, special characters in inputs.
Report any unexpected behavior.
```

```
Visit http://localhost:5275/Interactive and test each feature: modals, 
drag-and-drop, range slider, tabs, and dynamic content loading.
Generate tests for any untested scenarios.
```

---

### 4. Debugging Failing Tests

Use the AI to investigate why a test is failing by running the steps live in a browser.

**Steps:**
1. Open the failing test in VS Code
2. Ask the AI to reproduce the failure and explain the root cause

**Example prompts:**

```
Run the steps from ContactFormTests.FillContactForm_ShouldShowSuccessMessage 
manually in the browser at http://localhost:5275. 
Tell me if the success message appears and what selector is used.
```

```
The test expects GetByTestId("success-message") to be visible after form submission 
at http://localhost:5275/Contact. Navigate there, submit the form, and check if 
the element exists in the DOM.
```

```
Take a screenshot of http://localhost:5275/Products after searching for "laptop".
Compare the visible products against what ProductCatalogTests.SearchForProduct expects.
```

---

### 5. Selector Verification and Repair

Verify or fix element selectors without running the full test suite.

**Example prompts:**

```
Open http://localhost:5275/Contact and list all elements that have data-testid attributes.
```

```
On http://localhost:5275/Interactive, find the drag-and-drop zone elements and 
tell me their data-testid values.
```

```
A test uses Page.GetByTestId("cart-count") but it's failing. 
Open http://localhost:5275/Products, add an item to the cart, 
and find the correct selector for the cart counter.
```

---

### 6. Visual Verification and Screenshots

Capture screenshots to verify UI state before writing assertions.

**Example prompts:**

```
Take a screenshot of http://localhost:5275 and describe all the 
navigation cards visible on the home page.
```

```
Navigate to http://localhost:5275/Products, apply the "Electronics" filter,
and take a screenshot. Describe which products are shown.
```

```
Open http://localhost:5275/Interactive, open the modal dialog, 
and take a screenshot to verify its content.
```

---

### 7. Accessibility and Locator Discovery

Use the AI to inspect the accessibility tree and discover the best locators for new tests.

**Example prompts:**

```
Inspect the accessibility tree of http://localhost:5275/Register 
and list all form fields with their recommended locator strategies.
```

```
On http://localhost:5275/Contact, what is the best locator for 
the "Subject" dropdown? Provide a code example.
```

---

## 📝 Example: Full AI-Assisted Test Development Workflow

Here is an end-to-end workflow for writing a new test with MCP assistance:

### Step 1 – Explore the Page
```
Open http://localhost:5275/Register and describe each step of the registration form.
What data-testid attributes are available?
```

### Step 2 – Generate the Test
```
Based on what you found, write a complete NUnit test class in C# for the 
registration flow. Place it in the PlaywrightDemo.Tests namespace and extend 
PlaywrightTestBase. Include assertions for the success confirmation.
```

### Step 3 – Verify the Test
```
Run the generated registration test steps manually in the browser to confirm 
all selectors work and the success message appears.
```

### Step 4 – Add Edge Cases
```
Now write an additional test for the registration form that submits without 
filling required fields and verifies validation errors appear.
```

---

## 🔧 MCP Configuration Reference

The `.vscode/mcp.json` in this repository:

```json
{
  "servers": {
    "playwright": {
      "command": "npx",
      "args": ["@playwright/mcp@latest"],
      "type": "stdio"
    }
  }
}
```

### Available MCP Capabilities

| Capability | Description |
|-----------|-------------|
| `browser_navigate` | Navigate to a URL |
| `browser_click` | Click an element |
| `browser_type` | Type text into an element |
| `browser_fill_form` | Fill multiple form fields at once |
| `browser_select_option` | Select a dropdown option |
| `browser_snapshot` | Capture accessibility tree snapshot |
| `browser_take_screenshot` | Take a visual screenshot |
| `browser_hover` | Hover over an element |
| `browser_drag` | Drag and drop between elements |
| `browser_press_key` | Press keyboard keys |
| `browser_wait_for` | Wait for text or element state |
| `browser_evaluate` | Execute JavaScript in the browser |
| `browser_handle_dialog` | Handle alert/confirm dialogs |

---

## 💡 Tips and Best Practices

1. **Start the web app first** before using any MCP browser commands — the AI needs a running server to interact with.

2. **Be specific in prompts** — include the exact URL and the test framework (NUnit, MSTest) you want code generated for.

3. **Use accessibility-tree mode** (the default) for faster, more reliable automation compared to screenshot-based approaches.

4. **Iterate in small steps** — explore first, then generate code, then verify, rather than asking for everything in one prompt.

5. **Combine with `codegen`** — use MCP to explore and understand the page structure, then use traditional `playwright codegen` for interactive recording when preferred.

---

## 📚 Resources

- [Playwright MCP GitHub Repository](https://github.com/microsoft/playwright-mcp)
- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [VS Code MCP Support](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)
- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [GitHub Copilot Agent Mode](https://docs.github.com/en/copilot/using-github-copilot/copilot-chat/using-github-copilot-chat-in-your-ide)
