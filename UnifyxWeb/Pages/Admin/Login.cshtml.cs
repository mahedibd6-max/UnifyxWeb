using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UnifyxWeb.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    public LoginModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    [BindProperty, Required, EmailAddress] public string Email { get; set; } = "";
    [BindProperty, Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();
        var result = await _signIn.PasswordSignInAsync(Email, Password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
            return LocalRedirect(Url.IsLocalUrl(returnUrl) && returnUrl!.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ? returnUrl : "/admin");
        Error = result.IsLockedOut
            ? "Too many failed attempts. Try again in 15 minutes."
            : "Email or password is not correct.";
        return Page();
    }
}
