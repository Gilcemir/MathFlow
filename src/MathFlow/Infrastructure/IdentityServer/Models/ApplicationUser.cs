using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MathFlow.Infrastructure.IdentityServer.Models;

/// <summary>
/// Represents an application user with authentication and authorization capabilities.
/// Extends IdentityUser to leverage ASP.NET Core Identity infrastructure.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Display name of the user (full name).
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-ZÀ-ÿ\s'-]+$", ErrorMessage = "Display name can only contain letters, spaces, hyphens, and apostrophes")]
    public string DisplayName { get; set; } = string.Empty;

    // FUTURE EXTENSIONS:
    // - CreatedAt: timestamp de criação do usuário
    // - LastLoginAt: timestamp do último login
    // - ProfilePictureUrl: URL da foto de perfil
    // - Relacionamento com Wallet (quando implementado)
    // - Relacionamento com UsageLogs (quando implementado)
}
