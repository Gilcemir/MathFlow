using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Infrastructure.IdentityServer.Models;
using System.Security.Claims;

namespace MathFlow.Pages.Account;

public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ExternalLoginModel> _logger;

    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<ExternalLoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information.";
            return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
        }

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            return RedirectToPage("/Account/TwoFactor", new { email, returnUrl });
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("/Account/Lockout");
        }

        var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail))
        {
            ErrorMessage = "Email not provided by external provider.";
            return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
        }

        var user = new ApplicationUser
        {
            UserName = userEmail,
            Email = userEmail,
            EmailConfirmed = true,
            TwoFactorEnabled = false
        };

        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            createResult = await _userManager.AddLoginAsync(user, info);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "normal");

                _logger.LogInformation("User created account using {Provider} provider", info.LoginProvider);
                
                await _signInManager.SignInAsync(user, isPersistent: false);

                return LocalRedirect(returnUrl);
            }
        }

        foreach (var error in createResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        ErrorMessage = "Failed to create account.";
        return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
    }
}
