using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlaywrightDemo.WebApp.Pages
{
    public class ProductsModel : PageModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public void OnGet(string searchTerm = "")
        {
            SearchTerm = searchTerm ?? string.Empty;
        }
    }
}
