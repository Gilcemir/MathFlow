using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Application.Services.Identity;
using System.ComponentModel.DataAnnotations;

namespace MathFlow.Pages.Account;

public class TwoFactorModel : PageModel
{
    private readonly UserService _userService;
    private readonly ILogger<TwoFactorModel> _logger;

    public TwoFactorModel(UserService userService, ILogger<TwoFactorModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public bool RememberMe { get; set; }

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

    public IActionResult OnGet(string? email, bool rememberMe = false, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Account/Login");
        }

        Email = email;
        RememberMe = rememberMe;
        ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? email, bool rememberMe = false, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Account/Login");
        }

        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            Email = email;
            RememberMe = rememberMe;
            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await _userService.VerifyTwoFactorCodeAsync(email, Input.Code);

        if (result)
        {
            _logger.LogInformation("User {Email} completed 2FA verification", email);
            return LocalRedirect(returnUrl);
        }

        _logger.LogWarning("Invalid 2FA code for user {Email}", email);
        ModelState.AddModelError(string.Empty, "Invalid verification code. Please try again.");

        Email = email;
        RememberMe = rememberMe;
        ReturnUrl = returnUrl;

        return Page();
    }
}
