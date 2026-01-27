using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlaywrightDemo.WebApp.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string FirstName { get; set; } = string.Empty;
        
        [BindProperty]
        public string LastName { get; set; } = string.Empty;
        
        [BindProperty]
        public DateTime? DateOfBirth { get; set; }
        
        [BindProperty]
        public string Gender { get; set; } = string.Empty;
        
        [BindProperty]
        public string Email { get; set; } = string.Empty;
        
        [BindProperty]
        public string Phone { get; set; } = string.Empty;
        
        [BindProperty]
        public string Address { get; set; } = string.Empty;
        
        [BindProperty]
        public string City { get; set; } = string.Empty;
        
        [BindProperty]
        public string State { get; set; } = string.Empty;
        
        [BindProperty]
        public string ZipCode { get; set; } = string.Empty;
        
        [BindProperty]
        public string Username { get; set; } = string.Empty;
        
        [BindProperty]
        public string Password { get; set; } = string.Empty;
        
        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [BindProperty]
        public List<string> Interests { get; set; } = new List<string>();
        
        [BindProperty]
        public bool Newsletter { get; set; }
        
        [BindProperty]
        public bool AcceptTerms { get; set; }
        
        public bool IsRegistered { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid && AcceptTerms)
            {
                // Simulate user registration
                IsRegistered = true;
                return Page();
            }
            
            return Page();
        }
    }
}
