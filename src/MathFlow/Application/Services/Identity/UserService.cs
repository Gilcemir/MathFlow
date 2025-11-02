using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.Application.Services.Identity;

/// <summary>
/// Provides services for user authentication and management.
/// </summary>
public class UserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user with the default 'normal' role. Two-factor authentication is optional.
    /// </summary>
    /// <param name="email">User email address.</param>
    /// <param name="password">User password.</param>
    /// <param name="userName">User name.</param>
    /// <returns>The result of the registration operation.</returns>
    public async Task<IdentityResult> RegisterUserAsync(
        string email,
        string password,
        string userName)
    {
        ValidateEmail(email);
        ValidateUserName(userName);

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            TwoFactorEnabled = false,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Roles.Normal);

            _logger.LogInformation(
                "User {Email} registered successfully with role '{Role}'",
                email,
                Roles.Normal);
        }
        else
        {
            _logger.LogWarning(
                "Failed to register user {Email}: {Errors}",
                email,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    /// <summary>
    /// Authenticates a user with email and password, detecting 2FA requirement.
    /// </summary>
    /// <param name="email">User email address.</param>
    /// <param name="password">User password.</param>
    /// <param name="rememberMe">Whether to persist the authentication cookie.</param>
    /// <returns>The result of the sign-in operation.</returns>
    public async Task<SignInResult> LoginAsync(
        string email,
        string password,
        bool rememberMe)
    {
        ValidateEmail(email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user {Email}", email);
            return SignInResult.Failed;
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            password,
            rememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully", email);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} is locked out", email);
        }

        return result;
    }

    /// <summary>
    /// Verifies a two-factor authentication code and completes sign-in if valid.
    /// </summary>
    /// <param name="email">User email address.</param>
    /// <param name="code">TOTP code from authenticator app.</param>
    /// <returns>True if code is valid and sign-in completed, false otherwise.</returns>
    public async Task<bool> VerifyTwoFactorCodeAsync(
        string email,
        string code)
    {
        ValidateEmail(email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("2FA verification attempt for non-existent user {Email}", email);
            return false;
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (isValid)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            _logger.LogInformation("User {Email} completed 2FA successfully", email);
        }
        else
        {
            _logger.LogWarning("Invalid 2FA code for user {Email}", email);
        }

        return isValid;
    }

    /// <summary>
    /// Gets or generates the authenticator key for setting up 2FA.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>The authenticator key.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user is not found.</exception>
    public async Task<string> GetTwoFactorSetupKeyAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError("Attempted to get 2FA key for non-existent user {UserId}", userId);
            throw new InvalidOperationException("User not found");
        }

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
            _logger.LogInformation("Generated new authenticator key for user {UserId}", userId);
        }

        return key!;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("Invalid email format", nameof(email));
        }
    }

    private static void ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("Username cannot be empty", nameof(userName));
        }
    }
}
