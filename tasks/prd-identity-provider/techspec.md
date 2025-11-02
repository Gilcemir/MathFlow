# Tech Spec: Identity Provider - Autenticação e Autorização

**Versão:** 1.0  
**Data:** 2025-11-02  
**Autor:** Tech Team  
**Status:** Em Revisão  
**PRD Relacionado:** `tasks/prd-identity-provider/_prd.md`

---

## 1. Visão Geral da Arquitetura

### 1.1 Contexto Técnico

O MathFlow é uma aplicação ASP.NET Core 9.0 Razor Pages que atualmente não possui autenticação ou autorização. Esta especificação técnica detalha a implementação de um sistema de identidade completo usando ASP.NET Core Identity com as seguintes características:

- **Framework**: ASP.NET Core Identity 9.0
- **Banco de Dados**: PostgreSQL 17 (já configurado no Docker)
- **ORM**: Entity Framework Core 9.0
- **Autenticação Externa**: Google OAuth 2.0
- **Two-Factor Authentication**: TOTP (Time-based One-Time Password) via authenticator apps
- **Hashing**: PBKDF2 (padrão do ASP.NET Core Identity)

### 1.2 Decisões Arquiteturais Principais

| Decisão | Justificativa | Alternativas Consideradas |
|---------|---------------|---------------------------|
| Estrutura monolítica com pastas organizadas | Simplicidade para solo developer, fácil evolução | Clean Architecture com projetos separados |
| ApplicationUser sem interface IUser | Reduz complexidade, entidades de domínio usam apenas UserId (string) | Interface IUser no domínio |
| Policies nomeadas para autorização | Flexibilidade para adicionar lógica complexa (créditos) futuramente | Atributos [Authorize(Roles="")] diretos |
| Seed de master admin via appsettings | Configurável por ambiente, fácil troca de senha em produção | Hardcoded ou script SQL separado |
| Razor Pages em Pages/Account e Pages/Admin | Estrutura clara entre público e autenticado | Areas/Identity padrão |
| TestContainers para testes | Testes com PostgreSQL real, maior confiabilidade | InMemory database |

### 1.3 Requisitos Não-Funcionais

- **Performance**: Overhead de autenticação < 50ms por requisição
- **Segurança**: Passwords com PBKDF2, 10.000 iterações mínimo
- **Disponibilidade**: Falha de Google OAuth não deve impedir login via email/password
- **Auditabilidade**: Todos os eventos de autenticação e mudanças de role logados
- **Escalabilidade**: Suportar até 1.000 usuários concorrentes (baixa prioridade inicial)

---

## 2. Estrutura de Pastas Detalhada

```
MathFlow/
├── src/MathFlow/
│   ├── Domain/                          # Nova camada
│   │   └── Entities/
│   │       └── Wallet.cs                # Exemplo futuro
│   ├── Application/                     # Nova camada
│   │   └── Services/
│   │       └── Identity/
│   │           ├── UserService.cs
│   │           └── RoleService.cs
│   ├── Infrastructure/
│   │   ├── Converters/                  # Existente
│   │   ├── Observability/               # Existente
│   │   └── IdentityServer/              # Novo
│   │       ├── Data/
│   │       │   ├── ApplicationDbContext.cs
│   │       │   └── Migrations/
│   │       ├── Models/
│   │       │   └── ApplicationUser.cs
│   │       ├── Configuration/
│   │       │   ├── IdentityConfiguration.cs
│   │       │   └── AuthorizationPolicies.cs
│   │       └── Seeders/
│   │           └── IdentitySeeder.cs
│   ├── Pages/
│   │   ├── Public/                      # Rotas públicas
│   │   ├── Account/                     # Autenticação (público)
│   │   ├── Identity/Manage/             # Gestão de conta
│   │   ├── Admin/Users/                 # Administrativo
│   │   └── Converters/                  # Existente
│   ├── Program.cs
│   └── MathFlow.csproj
└── tests/
    ├── MathFlow.UnitTests/Identity/
    └── MathFlow.IntegrationTests/Identity/
```

---

## 3. Componentes Principais

### 3.1 ApplicationUser

**Localização**: `Infrastructure/IdentityServer/Models/ApplicationUser.cs`

```csharp
using Microsoft.AspNetCore.Identity;

namespace MathFlow.Infrastructure.IdentityServer.Models;

public class ApplicationUser : IdentityUser
{
    // Propriedades adicionais podem ser adicionadas aqui no futuro
}
```

### 3.2 ApplicationDbContext

**Localização**: `Infrastructure/IdentityServer/Data/ApplicationDbContext.cs`

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.Infrastructure.IdentityServer.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customizações de schema
        builder.Entity<ApplicationUser>(entity => entity.ToTable("Users"));
        builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
        builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens"));
    }
}
```

### 3.3 IdentityConfiguration

**Localização**: `Infrastructure/IdentityServer/Configuration/IdentityConfiguration.cs`

Ver arquivo completo no repositório. Principais configurações:
- Password policy: 8+ caracteres, 1 maiúscula, 1 especial
- Lockout: 5 tentativas, 5 minutos
- Google OAuth com callback `/signin-google`
- Cookie paths customizados

### 3.4 AuthorizationPolicies

**Localização**: `Infrastructure/IdentityServer/Configuration/AuthorizationPolicies.cs`

Policies definidas:
- `MasterAdminOnly`: apenas masterAdmin
- `AdminOnly`: admin ou masterAdmin
- `PremiumAccess`: premium, admin ou masterAdmin
- `AuthenticatedUser`: qualquer usuário autenticado

### 3.5 IdentitySeeder

**Localização**: `Infrastructure/IdentityServer/Seeders/IdentitySeeder.cs`
Responsabilidades:
- Criar roles: masterAdmin, admin, premium, normal
- Criar master admin com credenciais do appsettings
- Master admin não requer 2FA

### 3.6 UserService and RoleService

**Localização**: `Application/Services/Identity/`

Serviços de aplicação para:
- Registro de usuários (role padrão: normal, 2FA obrigatório)
- Login com suporte a 2FA
- Verificação de código 2FA
- Gerenciamento de roles (admin only):
  - `AssignRoleAsync`: Adiciona uma role ao usuário (usuários podem ter múltiplas roles)
  - `RemoveRoleAsync`: Remove uma role do usuário (masterAdmin não pode ser removida)
  - `GetUserRolesAsync`: Retorna todas as roles do usuário

---

## 4. Integração no Program.cs

{{ ... }}
```csharp
// Configure Identity
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Register application services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();

var app = builder.Build();

// Seed Identity data
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(services, builder.Configuration);
}

// Ordem correta
app.UseAuthentication();
app.UseAuthorization();
```

---

## 5. Configurações

### 5.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mathflow_db;..."
  },
  "Authentication": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  },
  "Identity": {
    "MasterAdmin": {
      "Email": "admin@mathflow.com",
      "Password": ""
    }
  }
}
```

### 5.2 appsettings.Development.json

```json
{
  "Identity": {
    "MasterAdmin": {
      "Email": "admin@mathflow.local",
      "Password": "Dev@1234"
    }
  }
}
```

---

## 6. Pacotes NuGet

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="9.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="9.0.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
</ItemGroup>
```

---

## 7. Migrations

```bash
# Criar migration
dotnet ef migrations add InitialIdentity \
  --context ApplicationDbContext \
  --output-dir Infrastructure/IdentityServer/Data/Migrations

# Aplicar migration
dotnet ef database update --context ApplicationDbContext
```

---

## 8. Razor Pages - Estrutura

### 8.1 Pages/Account/

- `Login.cshtml`: Login com email/password ou Google
- `Register.cshtml`: Registro com validação de senha
- `TwoFactor.cshtml`: Verificação de código TOTP
- `ExternalLogin.cshtml`: Callback do Google OAuth
- `Logout.cshtml`: Logout

### 8.2 Pages/Identity/Manage/

- `Index.cshtml`: Perfil do usuário
- `ChangePassword.cshtml`: Troca de senha
- `TwoFactorAuthentication.cshtml`: Configurar/desabilitar 2FA

### 8.3 Pages/Admin/Users/

- `Index.cshtml`: Listagem de usuários (policy: AdminOnly)
- `Edit.cshtml`: Editar roles (policy: AdminOnly)

---

## 9. Testes

### 9.1 Estrutura de Testes

```
tests/
├── MathFlow.UnitTests/
│   └── Identity/
│       ├── UserServiceTests.cs
│       ├── RoleServiceTests.cs
│       └── AuthorizationTests.cs
└── MathFlow.IntegrationTests/
    └── Identity/
        ├── RegistrationFlowTests.cs
        ├── LoginFlowTests.cs
        ├── TwoFactorFlowTests.cs
        └── GoogleOAuthTests.cs
```

### 9.2 TestContainers Setup

```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("mathflow_test")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected string GetConnectionString() => _dbContainer.GetConnectionString();
}
```

---

## 10. Plano de Implementação (MVP)

### Fase 1: Infraestrutura Base (2-3 dias)
1. Criar estrutura de pastas Domain/Application/Infrastructure
2. Adicionar pacotes NuGet
3. Implementar ApplicationUser e ApplicationDbContext
4. Criar e aplicar migration inicial
5. Testar conectividade com PostgreSQL

### Fase 2: Configuração e Seeding (1-2 dias)
1. Implementar IdentityConfiguration
2. Implementar AuthorizationPolicies
3. Implementar IdentitySeeder
4. Integrar no Program.cs
5. Testar seed de master admin

### Fase 3: Serviços de Aplicação (2-3 dias)
1. Implementar UserService (registro, login, 2FA)
2. Implementar RoleService
3. Adicionar logging e tratamento de erros
4. Testes unitários dos serviços

### Fase 4: Razor Pages - Autenticação (3-4 dias)
1. Criar páginas de Login e Register
2. Implementar TwoFactor page
3. Configurar Google OAuth e ExternalLogin
4. Implementar Logout
5. Testes de integração dos fluxos

### Fase 5: Razor Pages - Gestão (2-3 dias)
1. Criar páginas de Manage (perfil, senha, 2FA)
2. Criar páginas Admin (listagem, edição de roles)
3. Aplicar authorization policies
4. Testes de integração

### Fase 6: Testes e Refinamento (2-3 dias)
1. Configurar TestContainers
2. Implementar testes de integração completos
3. Testes de segurança (tentativas de bypass)
4. Ajustes de UX e mensagens de erro

**Total estimado: 12-18 dias**

---

## 11. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Complexidade de 2FA | Média | Alto | Usar bibliotecas padrão do ASP.NET Core Identity |
| Google OAuth falhas | Baixa | Médio | Fallback para email/password sempre disponível |
| Performance de hashing | Baixa | Médio | PBKDF2 é otimizado, monitorar com OpenTelemetry |
| Migração de usuários futuros | Baixa | Alto | Documentar schema e manter migrations versionadas |

---

## 12. Métricas de Sucesso

- Tempo de registro completo (com 2FA) < 2 minutos
- Tempo de login (com 2FA) < 30 segundos
- Taxa de sucesso de autenticação > 99%
- Zero vulnerabilidades críticas em audit de segurança
- 100% de cobertura de testes em fluxos críticos

---

## 13. Questões em Aberto

1. **Email confirmation**: MVP não inclui, adicionar em Fase 2?
2. **Password recovery**: Implementar em Fase 2 ou MVP?
3. **Account lockout notifications**: Enviar email ao usuário?
4. **Audit log detalhado**: Criar tabela separada ou usar logging?
5. **Rate limiting**: Adicionar proteção contra brute force?

---

## 14. Próximos Passos Pós-MVP

### Fase 2: Audit e Hooks
- Dashboard de eventos de autenticação
- Hooks para criação de Wallet pós-registro
- Logs estruturados para compliance

### Fase 3: Premium Gating
- Integração com sistema de créditos/subscriptions
- Middleware para verificar acesso a features premium
- UI para upgrade de plano

---

## Anexo A: Comandos Úteis

```bash
# Migrations
dotnet ef migrations add <Name> --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
dotnet ef migrations remove --context ApplicationDbContext

# Testes
dotnet test --filter "Category=Integration"
dotnet test --filter "FullyQualifiedName~Identity"

# Seed manual
dotnet run --seed-identity

# Verificar roles
dotnet run --list-roles
```

---

## Anexo B: Referências

- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Google OAuth Setup](https://developers.google.com/identity/protocols/oauth2)
- [TOTP RFC 6238](https://datatracker.ietf.org/doc/html/rfc6238)
- [TestContainers .NET](https://dotnet.testcontainers.org/)
