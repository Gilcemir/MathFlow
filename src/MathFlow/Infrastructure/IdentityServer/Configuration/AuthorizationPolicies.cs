using Microsoft.AspNetCore.Authorization;

namespace MathFlow.Infrastructure.IdentityServer.Configuration;

/// <summary>
/// Provides extension methods for configuring authorization policies.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy name for master administrator access only.
    /// Requires the 'masterAdmin' role.
    /// </summary>
    public const string MasterAdminOnly = "MasterAdminOnly";

    /// <summary>
    /// Policy name for administrator access.
    /// Requires either 'admin' or 'masterAdmin' role.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Policy name for premium feature access.
    /// Requires 'premium', 'admin', or 'masterAdmin' role.
    /// </summary>
    public const string PremiumAccess = "PremiumAccess";

    /// <summary>
    /// Policy name for any authenticated user.
    /// Requires the user to be authenticated but no specific role.
    /// </summary>
    public const string AuthenticatedUser = "AuthenticatedUser";

    /// <summary>
    /// Adds and configures authorization policies for the application.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(MasterAdminOnly, policy =>
                policy.RequireRole("masterAdmin"));

            options.AddPolicy(AdminOnly, policy =>
                policy.RequireRole("admin", "masterAdmin"));

            options.AddPolicy(PremiumAccess, policy =>
                policy.RequireRole("premium", "admin", "masterAdmin"));

            options.AddPolicy(AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}
