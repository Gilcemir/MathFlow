using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Infrastructure.IdentityServer.Models;
using MathFlow.Infrastructure.IdentityServer.Configuration;

namespace MathFlow.Pages.Identity.Manage;

[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public class TwoFactorAuthenticationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TwoFactorAuthenticationModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public bool TwoFactorEnabled { get; set; }
    public bool IsMasterAdmin { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        TwoFactorEnabled = user.TwoFactorEnabled;
        IsMasterAdmin = await _userManager.IsInRoleAsync(user, "masterAdmin");

        return Page();
    }

    public async Task<IActionResult> OnPostDisableAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        if (await _userManager.IsInRoleAsync(user, "masterAdmin"))
        {
            TempData["StatusMessage"] = "Master admin cannot disable 2FA.";
            return RedirectToPage();
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "2FA disabled successfully.";
        }

        return RedirectToPage();
    }
}
