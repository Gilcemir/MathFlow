---
status: pending
parallelizable: false
blocked_by: ["2.0","3.0","4.0","5.0"]
---

<task_context>
<domain>frontend/layout</domain>
<type>implementation</type>
<scope>navigation</scope>
<complexity>medium</complexity>
<dependencies>all_pages</dependencies>
<unblocks>10.0</unblocks>
</task_context>

# Tarefa 9.0: Atualizar Layout e Navegação Global

## Visão Geral
Atualizar todos os links de navegação global no `_Layout.cshtml` e `Index.cshtml` para usar as novas URLs.

## Subtarefas

### 9.1: _Layout.cshtml
**Arquivo:** `Pages/Shared/_Layout.cshtml`

**Links a atualizar:**
- [ ] Navbar brand: `asp-page="/Index"` → `asp-page="/"`
- [ ] Home link: `asp-page="/Index"` → `asp-page="/"`
- [ ] Privacy: `asp-page="/Privacy"` → `asp-page="/privacy"`
- [ ] Converter: `asp-page="/Converters/Converter"` → `asp-page="/converter"`
- [ ] Admin: `asp-page="/Admin/Users/Index"` → `asp-page="/admin/users"`
- [ ] Profile dropdown: `asp-page="/Identity/Manage/Index"` → `asp-page="/profile"`
- [ ] Logout form: `asp-page="/Account/Logout"` → `asp-page="/account/logout"`
- [ ] Login: `asp-page="/Account/Login"` → `asp-page="/account/login"`
- [ ] Register: `asp-page="/Account/Register"` → `asp-page="/account/register"`

**Total:** ~9 links

### 9.2: Index.cshtml
**Arquivo:** `Pages/Index.cshtml`

**Links a atualizar:**
- [ ] Converter card: `asp-page="/Converters/Converter"` → `asp-page="/converter"`
- [ ] Register button: `asp-page="/Account/Register"` → `asp-page="/account/register"` (2x)
- [ ] Login button: `asp-page="/Account/Login"` → `asp-page="/account/login"` (2x)
- [ ] Manage Profile: `asp-page="/Identity/Manage/Index"` → `asp-page="/profile"`
- [ ] Admin Panel: `asp-page="/Admin/Users/Index"` → `asp-page="/admin/users"`

**Total:** ~7 links

### 9.3: Verificar Outros Links
**Ação:** Buscar globalmente por `asp-page` para garantir que nenhum link foi esquecido

**Comando:**
```bash
cd src/MathFlow/Pages
grep -r "asp-page=" --include="*.cshtml" | grep -E "(Account|Identity|Admin|Converters|Privacy)" | grep -v "/account/" | grep -v "/profile" | grep -v "/settings/" | grep -v "/admin/" | grep -v "/converter" | grep -v "/privacy"
```

Se retornar resultados, atualizar também.

### 9.4: Compilar e Verificar
```bash
cd src/MathFlow
dotnet build
```

## Sequenciamento
- **Bloqueado por:** 2.0, 3.0, 4.0, 5.0 (todas páginas refatoradas)
- **Desbloqueia:** 10.0 (Testes E2E)
- **Paralelizável com:** Nenhuma
- **Lane:** Lane 4 (final)

## Critérios de Sucesso
- [ ] Todos links em `_Layout.cshtml` atualizados (~9 links)
- [ ] Todos links em `Index.cshtml` atualizados (~7 links)
- [ ] Busca global não retorna links antigos
- [ ] Código compila
- [ ] Commit descritivo

## Estimativa
**Total:** 30 minutos

---
**Criado:** 2025-01-12
