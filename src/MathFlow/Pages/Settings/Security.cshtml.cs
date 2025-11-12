using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Infrastructure.IdentityServer.Models;
using MathFlow.Infrastructure.IdentityServer.Configuration;
using MathFlow.Application.Services.Identity;
using System.ComponentModel.DataAnnotations;

namespace MathFlow.Pages.Settings;

[Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
public class SecurityModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserService _userService;

    public SecurityModel(
        UserManager<ApplicationUser> userManager,
        UserService userService)
    {
        _userManager = userManager;
        _userService = userService;
    }

    public bool TwoFactorEnabled { get; set; }
    public bool IsMasterAdmin { get; set; }
    public string? AuthenticatorKey { get; set; }
    public string? QrCodeUri { get; set; }
    public bool ShowSetup { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, ErrorMessage = "Code must be 6 digits", MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        [Display(Name = "Verification Code")]
        public string Code { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(bool? showSetup = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        TwoFactorEnabled = user.TwoFactorEnabled;
        IsMasterAdmin = await _userManager.IsInRoleAsync(user, "masterAdmin");
        ShowSetup = showSetup ?? false;

        if (ShowSetup && !TwoFactorEnabled)
        {
            AuthenticatorKey = await _userService.GetTwoFactorSetupKeyAsync(user.Id);
            QrCodeUri = $"otpauth://totp/MathFlow:{user.Email}?secret={AuthenticatorKey}&issuer=MathFlow";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostEnableAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        if (user.TwoFactorEnabled)
        {
            TempData["StatusMessage"] = "2FA is already enabled.";
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            ShowSetup = true;
            TwoFactorEnabled = false;
            IsMasterAdmin = await _userManager.IsInRoleAsync(user, "masterAdmin");
            AuthenticatorKey = await _userService.GetTwoFactorSetupKeyAsync(user.Id);
            QrCodeUri = $"otpauth://totp/MathFlow:{user.Email}?secret={AuthenticatorKey}&issuer=MathFlow";
            return Page();
        }

        var isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            Input.Code);

        if (!isCodeValid)
        {
            ModelState.AddModelError(string.Empty, "Invalid verification code. Please try again.");
            ShowSetup = true;
            TwoFactorEnabled = false;
            IsMasterAdmin = await _userManager.IsInRoleAsync(user, "masterAdmin");
            AuthenticatorKey = await _userService.GetTwoFactorSetupKeyAsync(user.Id);
            QrCodeUri = $"otpauth://totp/MathFlow:{user.Email}?secret={AuthenticatorKey}&issuer=MathFlow";
            return Page();
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Two-Factor Authentication enabled successfully!";
            return RedirectToPage();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return RedirectToPage();
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
