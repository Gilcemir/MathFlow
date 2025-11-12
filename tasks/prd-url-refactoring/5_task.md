---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/pages</domain>
<type>implementation</type>
<scope>public</scope>
<complexity>low</complexity>
<dependencies>razor_pages</dependencies>
<unblocks>8.0,9.0</unblocks>
</task_context>

# Tarefa 5.0: Refatorar Páginas Públicas

## Visão Geral
Refatorar 3 páginas públicas para `/privacy`, `/error`, `/converter`.

## Páginas a Refatorar

### 5.1: Privacy
- `Pages/Public/Privacy.cshtml` → `@page "/privacy"`
- Nenhum `.cshtml.cs` com redirects

### 5.2: Error
- `Pages/Error.cshtml` → `@page "/error"`
- Nenhum redirect

### 5.3: Converter
- `Pages/Converters/Converter.cshtml` → `@page "/converter"`
- Nenhum redirect

## Sequenciamento
- **Bloqueado por:** 1.0
- **Desbloqueia:** 8.0, 9.0
- **Paralelizável com:** 2.0, 3.0, 4.0
- **Lane:** Lane 3

## Critérios de Sucesso
- [ ] 3 páginas com `@page` customizada
- [ ] Código compila
- [ ] Commit descritivo

## Estimativa
**Total:** 15-20 minutos

---
**Criado:** 2025-01-12
