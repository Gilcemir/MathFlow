---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/pages</domain>
<type>implementation</type>
<scope>admin</scope>
<complexity>low</complexity>
<dependencies>razor_pages,authorization</dependencies>
<unblocks>8.0,9.0</unblocks>
</task_context>

# Tarefa 4.0: Refatorar Páginas Admin/ (Administrativas)

## Visão Geral
Refatorar 2 páginas administrativas para `/admin/users` e `/admin/users/edit`.

## Páginas a Refatorar

### 4.1: Users Index
- `Pages/Admin/Users/Index.cshtml` → `@page "/admin/users"`
- `.cshtml.cs`: `RedirectToPage()` sem parâmetro (2x) - manter igual

### 4.2: Users Edit
- `Pages/Admin/Users/Edit.cshtml` → `@page "/admin/users/edit"`
- `.cshtml.cs`: `"./Index"` → `"/admin/users"`

## Sequenciamento
- **Bloqueado por:** 1.0
- **Desbloqueia:** 8.0, 9.0
- **Paralelizável com:** 2.0, 3.0, 5.0
- **Lane:** Lane 3

## Critérios de Sucesso
- [ ] 2 páginas com `@page` customizada
- [ ] 1 `RedirectToPage()` atualizado
- [ ] Código compila
- [ ] Commit descritivo

## Estimativa
**Total:** 20-30 minutos

---
**Criado:** 2025-01-12
