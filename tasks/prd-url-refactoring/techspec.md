# Technical Specification: URL Structure Refactoring

## Resumo Executivo

Esta especificação técnica detalha a implementação da refatoração de URLs do MathFlow, transformando URLs implementation-centric para user-centric com kebab-case. A abordagem utiliza a diretiva `@page` do Razor Pages para customizar rotas mantendo a organização de código em PascalCase.

**Decisões Arquiteturais:**
- Usar `@page "/custom-route"` para definir URLs
- Atualizar todos `asp-page` e `RedirectToPage`
- Não implementar redirects (pré-produção)
- Big bang implementation

## Arquitetura do Sistema

### Componentes Afetados

1. **Razor Pages (.cshtml):** Adicionar `@page` directive
2. **Page Models (.cshtml.cs):** Atualizar `RedirectToPage()`
3. **Tag Helpers:** Atualizar `asp-page`
4. **Layout:** Atualizar navegação global
5. **IdentityConfiguration:** Atualizar cookie paths

## Mapeamento de URLs

### Autenticação
- `/Account/Login` → `/account/login`
- `/Account/Register` → `/account/register`
- `/Account/Logout` → `/account/logout`
- `/Account/TwoFactor` → `/account/two-factor`
- `/Account/TwoFactorSetup` → `/account/two-factor-setup`
- `/Account/ExternalLogin` → `/account/external-login`
- `/Account/AccessDenied` → `/account/access-denied`
- `/Account/Lockout` → `/account/lockout`

### Perfil e Configurações
- `/Identity/Manage` → `/profile`
- `/Identity/Manage/ChangePassword` → `/settings/password`
- `/Identity/Manage/TwoFactorAuthentication` → `/settings/security`

### Admin
- `/Admin/Users` → `/admin/users`
- `/Admin/Users/Edit` → `/admin/users/edit`

### Públicas
- `/Public/Privacy` → `/privacy`
- `/Error` → `/error`
- `/Converters/Converter` → `/converter`

## Sequenciamento

1. **Configuração** (30min): Atualizar `IdentityConfiguration.cs`
2. **Account/** (1h): 8 páginas de autenticação
3. **Identity/Manage/** (45min): 3 páginas de perfil
4. **Admin/** (30min): 2 páginas admin
5. **Públicas** (30min): 3 páginas públicas
6. **Navegação** (30min): Layout e Index
7. **Testes** (1h): End-to-end manual

**Total:** 4-5 horas

## Testes Críticos

- [ ] Login flow completo
- [ ] Registro de usuário
- [ ] 2FA setup e verificação
- [ ] Troca de senha
- [ ] Admin user management
- [ ] Navegação global
- [ ] OAuth Google
- [ ] Acesso direto via URL

## Riscos e Mitigações

**R1: Links esquecidos**
- Mitigação: Busca global por `asp-page` e `RedirectToPage`

**R2: Typos em rotas**
- Mitigação: Testes manuais de cada URL

**R3: Case sensitivity**
- Mitigação: Convenção strict lowercase + hífens

## Conformidade

✅ Simplicidade (built-in ASP.NET Core)
✅ Mantém convenções C# (PascalCase em arquivos)
✅ URLs kebab-case (padrão web)
✅ Sem over-engineering
