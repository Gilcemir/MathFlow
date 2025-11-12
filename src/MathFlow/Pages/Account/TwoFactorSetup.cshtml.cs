using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Application.Services.Identity;
using System.ComponentModel.DataAnnotations;

namespace MathFlow.Pages.Account;

public class TwoFactorSetupModel : PageModel
{
    private readonly UserService _userService;
    private readonly ILogger<TwoFactorSetupModel> _logger;

    public TwoFactorSetupModel(UserService userService, ILogger<TwoFactorSetupModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public string? AuthenticatorKey { get; set; }

    public string? QrCodeUri { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? Email { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, ErrorMessage = "Code must be 6 digits", MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        [Display(Name = "Verification Code")]
        public string Code { get; set; } = string.Empty;
    }

    public Task<IActionResult> OnGetAsync(string? email, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<IActionResult>(RedirectToPage("/Account/Login"));
        }

        Email = email;
        ReturnUrl = returnUrl;

        AuthenticatorKey = "PLACEHOLDER_KEY";
        QrCodeUri = $"otpauth://totp/MathFlow:{email}?secret={AuthenticatorKey}&issuer=MathFlow";

        return Task.FromResult<IActionResult>(Page());
    }

    public async Task<IActionResult> OnPostAsync(string? email, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Account/Login");
        }

        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            Email = email;
            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await _userService.VerifyTwoFactorCodeAsync(email, Input.Code);

        if (result)
        {
            _logger.LogInformation("User {Email} completed 2FA setup", email);
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid verification code. Please try again.");

        Email = email;
        ReturnUrl = returnUrl;

        return Page();
    }
}
