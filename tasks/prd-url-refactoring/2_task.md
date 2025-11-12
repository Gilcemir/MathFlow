---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/pages</domain>
<type>implementation</type>
<scope>authentication</scope>
<complexity>medium</complexity>
<dependencies>razor_pages,identity</dependencies>
<unblocks>6.0,9.0</unblocks>
</task_context>

# Tarefa 2.0: Refatorar Páginas Account/ (Autenticação)

## Visão Geral
Refatorar todas as 8 páginas de autenticação na pasta `Account/` para usar novas URLs kebab-case. Isso inclui adicionar diretivas `@page` customizadas e atualizar todas as referências `RedirectToPage()` nos code-behind files.

## Contexto da Tech Spec
**Referência:** Seção "Mapeamento de URLs - Autenticação" da Tech Spec

As páginas de autenticação são o ponto de entrada crítico da aplicação. Todas devem seguir o padrão `/account/*` com kebab-case.

## Requisitos
- [ ] Adicionar `@page` directive em 8 arquivos `.cshtml`
- [ ] Atualizar `RedirectToPage()` em 6 arquivos `.cshtml.cs`
- [ ] Manter toda lógica de negócio intacta
- [ ] Código compila sem erros

## Subtarefas

### 2.1: Login Page
**Arquivos:**
- `Pages/Account/Login.cshtml`
- `Pages/Account/Login.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/login"` no `.cshtml`
2. Atualizar `.cshtml.cs`:
   - `"./TwoFactor"` → `"/account/two-factor"`

### 2.2: Register Page
**Arquivos:**
- `Pages/Account/Register.cshtml`
- `Pages/Account/Register.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/register"` no `.cshtml`
2. Atualizar `.cshtml.cs`:
   - `"./Login"` → `"/account/login"`

### 2.3: Logout Page
**Arquivos:**
- `Pages/Account/Logout.cshtml`
- `Pages/Account/Logout.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/logout"` no `.cshtml`
2. Atualizar `.cshtml.cs`:
   - `"/Index"` → `"/"` (2 ocorrências)

### 2.4: TwoFactor Page
**Arquivos:**
- `Pages/Account/TwoFactor.cshtml`
- `Pages/Account/TwoFactor.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/two-factor"` no `.cshtml`
2. Atualizar `.cshtml.cs`:
   - `"./Login"` → `"/account/login"` (2 ocorrências)

### 2.5: TwoFactorSetup Page
**Arquivos:**
- `Pages/Account/TwoFactorSetup.cshtml`
- `Pages/Account/TwoFactorSetup.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/two-factor-setup"` no `.cshtml`
2. Atualizar `.cshtml.cs`:
   - `"./Login"` → `"/account/login"` (2 ocorrências)

### 2.6: ExternalLogin Page
**Arquivos:**
- `Pages/Account/ExternalLogin.cshtml`
- `Pages/Account/ExternalLogin.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/external-login"` no `.cshtml`
2. Atualizar `.cshtml.cs`:
   - `"./Login"` → `"/account/login"` (4 ocorrências)
   - `"./TwoFactor"` → `"/account/two-factor"`
   - `"./Lockout"` → `"/account/lockout"`

### 2.7: AccessDenied Page
**Arquivos:**
- `Pages/Account/AccessDenied.cshtml`
- `Pages/Account/AccessDenied.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/access-denied"` no `.cshtml`
2. Nenhuma mudança no `.cshtml.cs` (sem redirects)

### 2.8: Lockout Page
**Arquivos:**
- `Pages/Account/Lockout.cshtml`
- `Pages/Account/Lockout.cshtml.cs`

**Mudanças:**
1. Adicionar `@page "/account/lockout"` no `.cshtml`
2. Nenhuma mudança no `.cshtml.cs` (sem redirects)

### 2.9: Compilar e Verificar
**Ação:** Garantir que todas mudanças compilam

**Comando:**
```bash
cd src/MathFlow
dotnet build
```

## Sequenciamento
- **Bloqueado por:** 1.0 (Cookie paths configurados)
- **Desbloqueia:** 6.0 (Testes de autenticação), 9.0 (Layout)
- **Paralelizável com:** 3.0 (Profile), 4.0 (Admin), 5.0 (Public)
- **Lane:** Lane 1

## Detalhes de Implementação

### Padrão de Mudança

**Para arquivos .cshtml:**
```cshtml
<!-- ANTES -->
@page
@model LoginModel

<!-- DEPOIS -->
@page "/account/login"
@model LoginModel
```

**Para arquivos .cshtml.cs:**
```csharp
// ANTES
return RedirectToPage("./TwoFactor", new { ... });

// DEPOIS
return RedirectToPage("/account/two-factor", new { ... });
```

### Atenção Especial
- **Paths relativos (`./`)** → Converter para **paths absolutos (`/`)**
- **CamelCase** → Converter para **kebab-case**
- **Manter parâmetros** de `RedirectToPage()` intactos
- **Não modificar** lógica de negócio

## Critérios de Sucesso
- [ ] Todas 8 páginas têm `@page` directive customizada
- [ ] Todos `RedirectToPage()` atualizados (total: ~15 ocorrências)
- [ ] Código compila sem erros
- [ ] Nenhuma lógica de negócio foi modificada
- [ ] Commit com mensagem descritiva

## Observabilidade
Logs existentes de autenticação permanecem inalterados:
```csharp
_logger.LogInformation("User {Email} logged in", email);
```

## Riscos
| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Esquecer algum RedirectToPage | Média | Alto | Buscar globalmente por `RedirectToPage` |
| Typo em URLs | Baixa | Alto | Revisão cuidadosa, testes manuais |
| Quebrar OAuth flow | Baixa | Alto | Testar Google login após mudanças |

## Recursos
- **Tech Spec:** `tasks/prd-url-refactoring/techspec.md#mapeamento-de-urls`
- **PRD:** `tasks/prd-url-refactoring/_prd.md#f1-authentication-url-refactoring`
- **Pasta:** `src/MathFlow/Pages/Account/`

## Estimativa
- **Desenvolvimento:** 45 minutos (8 páginas × ~5min cada)
- **Verificação:** 10 minutos
- **Commit:** 5 minutos
- **Total:** 1 hora

## Notas de Implementação
<!-- Adicionar durante desenvolvimento -->

---
**Criado:** 2025-01-12
**Atualizado:** 2025-01-12
