using Microsoft.AspNetCore.Identity;

namespace MathFlow.Infrastructure.IdentityServer.Models;

/// <summary>
/// Represents an application user with authentication and authorization capabilities.
/// Extends IdentityUser to leverage ASP.NET Core Identity infrastructure.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // FUTURE EXTENSIONS:
    // - CreatedAt: timestamp de criação do usuário
    // - LastLoginAt: timestamp do último login
    // - ProfilePictureUrl: URL da foto de perfil
    // - Relacionamento com Wallet (quando implementado)
    // - Relacionamento com UsageLogs (quando implementado)
}
