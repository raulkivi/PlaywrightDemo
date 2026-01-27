using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightDemo.Tests;

/// <summary>
/// Tests for product catalog functionality including search, filtering, and cart operations.
/// </summary>
[TestFixture]
public class ProductCatalogTests : PlaywrightTestBase
{
    [Test]
    [Description("Test product search functionality")]
    public async Task SearchProducts_ShouldFilterResults()
    {
        // Navigate to products page
        await NavigateToPageAsync("Products");
        
        // Search for "headphones"
        await Page.FillAsync("[data-testid='search-input']", "headphones");
        await Page.ClickAsync("[data-testid='search-button']");
        
        // Wait a moment for JavaScript to process
        await Page.WaitForTimeoutAsync(500);
        
        // Verify search functionality by checking page content
        var pageContent = await Page.TextContentAsync("body");
        Assert.That(pageContent, Does.Contain("headphones").IgnoreCase);
    }
    
    [Test]
    [Description("Test category filtering")]
    public async Task FilterByCategory_ShouldShowOnlyRelevantProducts()
    {
        // Navigate to products page
        await NavigateToPageAsync("Products");
        
        // Filter by electronics category
        await Page.SelectOptionAsync("[data-testid='category-filter']", "electronics");
        
        // Wait for JavaScript to process
        await Page.WaitForTimeoutAsync(500);
        
        // Verify only electronics products are visible
        var electronicsProducts = Page.Locator(".product-item[data-category='electronics']");
        var clothingProducts = Page.Locator(".product-item[data-category='clothing']");
        
        await Expect(electronicsProducts.Nth(0)).ToBeVisibleAsync();
        await Expect(clothingProducts.Nth(0)).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Description("Test adding products to cart")]
    public async Task AddProductsToCart_ShouldUpdateCartCount()
    {
        // Navigate to products page
        await NavigateToPageAsync("Products");
        
        // Initially cart should not be visible
        await Expect(Page.Locator("[data-testid='cart-summary']")).Not.ToBeVisibleAsync();
        
        // Add first product to cart
        await Page.ClickAsync("[data-testid='add-to-cart-1']");
        
        // Wait for cart to appear and check count
        await Expect(Page.Locator("[data-testid='cart-summary']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='cart-count']")).ToHaveTextAsync("1");
        
        // Add another product
        await Page.ClickAsync("[data-testid='add-to-cart-2']");
        
        // Verify count updated
        await Expect(Page.Locator("[data-testid='cart-count']")).ToHaveTextAsync("2");
        
        // Test clear cart functionality
        await Page.ClickAsync("[data-testid='clear-cart-button']");
        
        // Cart should be hidden again
        await Expect(Page.Locator("[data-testid='cart-summary']")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Description("Test product interaction buttons")]
    public async Task ProductButtons_ShouldBeInteractive()
    {
        // Navigate to products page
        await NavigateToPageAsync("Products");
        
        // Test that add to cart button changes text temporarily
        var addToCartButton = Page.Locator("[data-testid='add-to-cart-1']");
        
        // Click and verify text changes
        await addToCartButton.ClickAsync();
        await Expect(addToCartButton).ToHaveTextAsync("Added!");
        await Expect(addToCartButton).ToHaveClassAsync("btn btn-success btn-sm");
        
        // Wait for it to change back (2 seconds timeout in the JavaScript)
        await Page.WaitForTimeoutAsync(2500);
        await Expect(addToCartButton).ToHaveTextAsync("Add to Cart");
        await Expect(addToCartButton).ToHaveClassAsync("btn btn-primary btn-sm");
        
        // Test view details buttons are present
        for (int i = 1; i <= 6; i++)
        {
            await Expect(Page.Locator($"[data-testid='view-details-{i}']")).ToBeVisibleAsync();
        }
    }
    
    [Test]
    [Description("Test navigation and page structure")]
    public async Task ProductsPage_ShouldHaveCorrectStructure()
    {
        // Navigate to products page
        await NavigateToPageAsync("Products");
        
        // Verify page elements are present
        await Expect(Page.Locator("h2")).ToHaveTextAsync("Product Catalog");
        await Expect(Page.Locator("[data-testid='search-input']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='search-button']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='category-filter']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='sort-by']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='product-grid']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='home-link']")).ToBeVisibleAsync();
        
        // Verify all 6 products are initially visible
        var allProducts = Page.Locator(".product-item");
        await Expect(allProducts).ToHaveCountAsync(6);
        
        // Verify each product has required elements
        for (int i = 1; i <= 6; i++)
        {
            await Expect(Page.Locator($"[data-testid='add-to-cart-{i}']")).ToBeVisibleAsync();
            await Expect(Page.Locator($"[data-testid='view-details-{i}']")).ToBeVisibleAsync();
        }
    }
}
