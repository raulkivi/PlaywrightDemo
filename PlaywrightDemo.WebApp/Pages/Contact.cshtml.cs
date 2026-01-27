using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlaywrightDemo.WebApp.Pages
{
    [IgnoreAntiforgeryToken] // Disable anti-forgery for demo purposes
    public class ContactModel : PageModel
    {
        [BindProperty]
        public string FirstName { get; set; } = string.Empty;
        
        [BindProperty]
        public string LastName { get; set; } = string.Empty;
        
        [BindProperty]
        public string Email { get; set; } = string.Empty;
        
        [BindProperty]
        public string Subject { get; set; } = string.Empty;
        
        [BindProperty]
        public string Message { get; set; } = string.Empty;
        
        [BindProperty]
        public bool Newsletter { get; set; }
        
        public bool IsSubmitted { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // For demo purposes, just set IsSubmitted = true if we have basic required fields
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) && !string.IsNullOrEmpty(Email))
            {
                IsSubmitted = true;
            }
            
            return Page();
        }
    }
}
