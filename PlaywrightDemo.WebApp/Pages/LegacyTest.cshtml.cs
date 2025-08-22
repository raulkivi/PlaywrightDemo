using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlaywrightDemo.WebApp.Pages;

public class LegacyTestModel : PageModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Country { get; set; } = "";
    public string Message { get; set; } = "";
    public bool Newsletter { get; set; }
    public bool IsSubmitted { get; set; }

    public void OnGet()
    {
        // Initialize form
    }

    public void OnPost(string firstName, string lastName, string email, string country, string message, bool newsletter = false)
    {
        FirstName = firstName ?? "";
        LastName = lastName ?? "";
        Email = email ?? "";
        Country = country ?? "";
        Message = message ?? "";
        Newsletter = newsletter;
        IsSubmitted = true;
    }
}
