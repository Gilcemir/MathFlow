using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Infrastructure.IdentityServer.Models;
using MathFlow.Infrastructure.IdentityServer.Configuration;

namespace MathFlow.Pages.Profile;

[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public IList<string>? Roles { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        Username = user.UserName;
        Email = user.Email;
        EmailConfirmed = user.EmailConfirmed;
        TwoFactorEnabled = user.TwoFactorEnabled;
        Roles = await _userManager.GetRolesAsync(user);

        return Page();
    }
}
