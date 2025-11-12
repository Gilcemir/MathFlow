---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>backend/configuration</domain>
<type>configuration</type>
<scope>foundation</scope>
<complexity>low</complexity>
<dependencies>identity_configuration</dependencies>
<unblocks>2.0,3.0,4.0,5.0</unblocks>
</task_context>

# Tarefa 1.0: Setup - Atualizar Configuração de Cookies

## Visão Geral
Atualizar os paths de cookies do ASP.NET Core Identity para refletir as novas URLs de autenticação. Esta é uma tarefa bloqueante que deve ser completada antes de qualquer refatoração de páginas, pois define onde o sistema redirecionará automaticamente para login, logout e acesso negado.

## Contexto da Tech Spec
**Referência:** Seção "Pontos de Integração" da Tech Spec

O ASP.NET Core Identity usa cookie authentication com paths configuráveis que determinam para onde redirecionar em eventos de autenticação. Estes paths devem ser atualizados para as novas URLs kebab-case antes de modificar qualquer página.

## Requisitos
- [ ] Atualizar `LoginPath` de `/Account/Login` para `/account/login`
- [ ] Atualizar `LogoutPath` de `/Account/Logout` para `/account/logout`
- [ ] Atualizar `AccessDeniedPath` de `/Account/AccessDenied` para `/account/access-denied`

## Subtarefas

### 1.1: Localizar Configuração de Cookies
**Ação:** Encontrar onde `ConfigureApplicationCookie` é chamado

**Arquivos:**
- Modificar: `src/MathFlow/Infrastructure/IdentityServer/Configuration/IdentityConfiguration.cs`

**Localização esperada:**
```csharp
services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    // ... outras configurações
});
```

### 1.2: Atualizar Cookie Paths
**Ação:** Modificar os três paths para kebab-case

**Código esperado:**
```csharp
services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/access-denied";
    // ... outras configurações permanecem iguais
});
```

**Atenção:**
- Usar **lowercase** completo
- Usar **hífens** para separar palavras
- Manter barra inicial `/`
- NÃO modificar outras propriedades do cookie

### 1.3: Verificar Compilação
**Ação:** Garantir que o código compila sem erros

**Comando:**
```bash
cd src/MathFlow
dotnet build
```

**Resultado esperado:** `Build succeeded. 0 Warning(s). 0 Error(s).`

## Sequenciamento
- **Bloqueado por:** Nenhuma (tarefa inicial)
- **Desbloqueia:** 2.0, 3.0, 4.0, 5.0 (todas refatorações de páginas)
- **Paralelizável com:** Nenhuma (deve ser feita primeiro)
- **Lane:** Lane 1 (início do caminho crítico)

## Detalhes de Implementação

### Arquitetura
Esta configuração é parte do setup do ASP.NET Core Identity e é carregada durante a inicialização da aplicação. Os paths configurados são usados pelo middleware de autenticação para redirecionar automaticamente em cenários como:
- Acesso a página protegida sem autenticação → `LoginPath`
- Logout bem-sucedido → `LogoutPath`
- Acesso negado por falta de permissão → `AccessDeniedPath`

### Componentes Principais
**IdentityConfiguration:**
- Responsabilidade: Configurar ASP.NET Core Identity e autenticação externa
- Localização: `src/MathFlow/Infrastructure/IdentityServer/Configuration/IdentityConfiguration.cs`
- Método relevante: `AddIdentityServices()`

### Integrações
- **ASP.NET Core Identity:** Middleware de autenticação usa estes paths
- **Cookie Authentication:** Paths são armazenados em configuração de cookies
- **Razor Pages:** Páginas protegidas por `[Authorize]` usam estes redirects

### Tratamento de Erros
Não aplicável. Esta é uma configuração estática que não tem lógica de runtime que possa falhar.

## Critérios de Sucesso
- [ ] `LoginPath` atualizado para `/account/login`
- [ ] `LogoutPath` atualizado para `/account/logout`
- [ ] `AccessDeniedPath` atualizado para `/account/access-denied`
- [ ] Código compila sem erros
- [ ] Nenhuma outra configuração foi modificada acidentalmente
- [ ] Commit com mensagem descritiva

## Observabilidade
Não aplicável. Esta é uma mudança de configuração que não requer logging adicional.

## Configuração
Nenhuma variável de ambiente adicional necessária.

## Riscos
| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Typo nos paths | Baixa | Alto | Revisão cuidadosa, testes manuais |
| Modificar outras configs | Baixa | Médio | Modificar apenas os 3 paths especificados |

## Recursos
- **Tech Spec:** `tasks/prd-url-refactoring/techspec.md#pontos-de-integração`
- **PRD:** `tasks/prd-url-refactoring/_prd.md`
- **Arquivo:** `src/MathFlow/Infrastructure/IdentityServer/Configuration/IdentityConfiguration.cs`

## Estimativa
- **Desenvolvimento:** 15 minutos
- **Verificação:** 5 minutos
- **Commit:** 5 minutos
- **Total:** 25-30 minutos

## Notas de Implementação
<!-- Adicionar durante desenvolvimento -->

---
**Criado:** 2025-01-12
**Atualizado:** 2025-01-12
