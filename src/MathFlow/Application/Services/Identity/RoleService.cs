using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.Application.Services.Identity;

/// <summary>
/// Provides services for managing user roles.
/// </summary>
public class RoleService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        UserManager<ApplicationUser> userManager,
        ILogger<RoleService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Adds a role to a user. Users can have multiple roles simultaneously.
    /// If the user already has the role, this operation succeeds without changes.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleName">The role name to add.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<IdentityResult> AssignRoleAsync(string userId, string roleName)
    {
        ValidateUserId(userId);
        ValidateRoleName(roleName);

        if (!Roles.IsValid(roleName))
        {
            _logger.LogWarning("Attempted to assign invalid role {RoleName} to user {UserId}", roleName, userId);
            return IdentityResult.Failed(
                new IdentityError { Description = $"Invalid role: {roleName}" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to assign role to non-existent user {UserId}", userId);
            return IdentityResult.Failed(
                new IdentityError { Description = "User not found" });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(roleName))
        {
            _logger.LogInformation("User {UserId} already has role {RoleName}", userId, roleName);
            return IdentityResult.Success;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "User {UserId} ({Email}) assigned to role {RoleName}",
                userId,
                user.Email,
                roleName);
        }
        else
        {
            _logger.LogError(
                "Failed to assign role {RoleName} to user {UserId}: {Errors}",
                roleName,
                userId,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    /// <summary>
    /// Removes a role from a user. The masterAdmin role cannot be removed.
    /// If the user does not have the role, this operation succeeds without changes.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleName">The role name to remove.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<IdentityResult> RemoveRoleAsync(string userId, string roleName)
    {
        ValidateUserId(userId);
        ValidateRoleName(roleName);

        if (roleName == Roles.MasterAdmin)
        {
            _logger.LogWarning("Attempted to remove masterAdmin role from user {UserId}", userId);
            return IdentityResult.Failed(
                new IdentityError { Description = "Cannot remove masterAdmin role" });
        }

        if (!Roles.IsValid(roleName))
        {
            _logger.LogWarning("Attempted to remove invalid role {RoleName} from user {UserId}", roleName, userId);
            return IdentityResult.Failed(
                new IdentityError { Description = $"Invalid role: {roleName}" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to remove role from non-existent user {UserId}", userId);
            return IdentityResult.Failed(
                new IdentityError { Description = "User not found" });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(roleName))
        {
            _logger.LogInformation("User {UserId} does not have role {RoleName}", userId, roleName);
            return IdentityResult.Success;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "User {UserId} ({Email}) removed from role {RoleName}",
                userId,
                user.Email,
                roleName);
        }
        else
        {
            _logger.LogError(
                "Failed to remove role {RoleName} from user {UserId}: {Errors}",
                roleName,
                userId,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of role names.</returns>
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        ValidateUserId(userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to get roles for non-existent user {UserId}", userId);
            return new List<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        _logger.LogDebug("User {UserId} has roles: {Roles}", userId, string.Join(", ", roles));
        
        return roles;
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        }
    }

    private static void ValidateRoleName(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("Role name cannot be empty", nameof(roleName));
        }
    }
}
