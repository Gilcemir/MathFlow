---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/pages</domain>
<type>implementation</type>
<scope>profile_settings</scope>
<complexity>low</complexity>
<dependencies>razor_pages,identity</dependencies>
<unblocks>7.0,9.0</unblocks>
</task_context>

# Tarefa 3.0: Refatorar Páginas Identity/Manage/ (Perfil e Settings)

## Visão Geral
Refatorar 3 páginas de perfil e configurações para usar URLs user-centric: `/profile`, `/settings/password`, `/settings/security`.

## Páginas a Refatorar

### 3.1: Profile Index
- `Pages/Identity/Manage/Index.cshtml` → `@page "/profile"`
- Nenhum `RedirectToPage()` no `.cshtml.cs`

### 3.2: Change Password
- `Pages/Identity/Manage/ChangePassword.cshtml` → `@page "/settings/password"`
- `.cshtml.cs`: `"./Index"` → `"/profile"`

### 3.3: Two-Factor Authentication
- `Pages/Identity/Manage/TwoFactorAuthentication.cshtml` → `@page "/settings/security"`
- `.cshtml.cs`: `RedirectToPage()` sem parâmetro (4x) - manter igual

## Sequenciamento
- **Bloqueado por:** 1.0
- **Desbloqueia:** 7.0, 9.0
- **Paralelizável com:** 2.0, 4.0, 5.0
- **Lane:** Lane 2

## Critérios de Sucesso
- [ ] 3 páginas com `@page` customizada
- [ ] 1 `RedirectToPage()` atualizado
- [ ] Código compila
- [ ] Commit descritivo

## Estimativa
**Total:** 30-45 minutos

---
**Criado:** 2025-01-12
