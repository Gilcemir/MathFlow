using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MathFlow.Application.Services.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;

namespace MathFlow.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserService _userService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserService userService,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger)
    {
        _userService = userService;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display name is required")]
        [StringLength(100, ErrorMessage = "Display name must be between {2} and {1} characters", MinimumLength = 2)]
        [Display(Name = "Full Name")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s'-]+$", ErrorMessage = "Display name can only contain letters, spaces, hyphens, and apostrophes")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).*$",
            ErrorMessage = "Password must contain at least one uppercase letter and one special character")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var result = await _userService.RegisterUserAsync(Input.Email, Input.Password, Input.DisplayName);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} registered successfully", Input.Email);

                TempData["SuccessMessage"] = "Registration successful! You can now log in. You may enable Two-Factor Authentication from your profile settings.";
                return RedirectToPage("/Account/Login", new { returnUrl });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }

    public IActionResult OnPostExternalLogin(string provider, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return new ChallengeResult(provider, properties);
    }
}
