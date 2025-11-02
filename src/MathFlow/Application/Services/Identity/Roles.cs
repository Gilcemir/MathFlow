namespace MathFlow.Application.Services.Identity;

/// <summary>
/// Defines the available roles in the system.
/// </summary>
public static class Roles
{
    public const string MasterAdmin = "masterAdmin";
    public const string Admin = "admin";
    public const string Premium = "premium";
    public const string Normal = "normal";

    /// <summary>
    /// Gets all valid role names.
    /// </summary>
    public static readonly string[] All = 
    {
        MasterAdmin,
        Admin,
        Premium,
        Normal
    };

    /// <summary>
    /// Checks if a role name is valid.
    /// </summary>
    public static bool IsValid(string roleName) => All.Contains(roleName);
}
