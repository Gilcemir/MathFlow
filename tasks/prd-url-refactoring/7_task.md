---
status: pending
parallelizable: true
blocked_by: ["3.0"]
---

<task_context>
<domain>testing</domain>
<type>testing</type>
<scope>profile_settings</scope>
<complexity>low</complexity>
<dependencies>profile_pages</dependencies>
<unblocks>10.0</unblocks>
</task_context>

# Tarefa 7.0: Testes de Perfil e Settings

## Cenários de Teste

### 7.1: Profile View
- [ ] Acessar `/profile` → página carrega
- [ ] Informações do usuário exibidas corretamente

### 7.2: Change Password
- [ ] Clicar "Change Password" → redireciona para `/settings/password`
- [ ] Trocar senha com sucesso → redireciona para `/profile`
- [ ] Mensagem de sucesso exibida

### 7.3: Security Settings
- [ ] Clicar "Manage 2FA" → redireciona para `/settings/security`
- [ ] Habilitar 2FA → permanece em `/settings/security`
- [ ] Desabilitar 2FA (não master admin) → funciona corretamente

## Sequenciamento
- **Bloqueado por:** 3.0
- **Desbloqueia:** 10.0
- **Paralelizável com:** 6.0, 8.0
- **Lane:** Lane 2

## Critérios de Sucesso
- [ ] Todos cenários passam
- [ ] Redirects funcionam
- [ ] Mensagens de feedback exibidas

## Estimativa
**Total:** 15 minutos

---
**Criado:** 2025-01-12
