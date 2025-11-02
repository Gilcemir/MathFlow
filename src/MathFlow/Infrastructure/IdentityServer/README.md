# Identity Server

Implementação do ASP.NET Core Identity para autenticação e autorização do MathFlow.

## Estrutura

- **Data/**: DbContext e Migrations do Entity Framework Core
- **Models/**: ApplicationUser e modelos relacionados
- **Configuration/**: Configurações de Identity, OAuth e Authorization Policies
- **Seeders/**: Scripts de seed para roles e usuários iniciais

## Tecnologias

- ASP.NET Core Identity 9.0
- Entity Framework Core 9.0
- PostgreSQL 17
- Google OAuth 2.0
- TOTP (Two-Factor Authentication)

## Referências

- PRD: `tasks/prd-identity-provider/_prd.md`
- Tech Spec: `tasks/prd-identity-provider/techspec.md`
