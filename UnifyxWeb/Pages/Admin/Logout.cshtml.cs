using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UnifyxWeb.Pages.Admin;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    public LogoutModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    public IActionResult OnGet() => Redirect("/admin");

    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();
        return Redirect("/admin/login");
    }
}
