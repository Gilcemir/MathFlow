---
status: pending
parallelizable: false
blocked_by: ["6.0","7.0","8.0","9.0"]
---

<task_context>
<domain>testing</domain>
<type>testing</type>
<scope>end_to_end</scope>
<complexity>high</complexity>
<dependencies>all_components</dependencies>
<unblocks>[]</unblocks>
</task_context>

# Tarefa 10.0: Testes End-to-End Completos

## Visão Geral
Executar testes end-to-end completos de todos os fluxos da aplicação para garantir que a refatoração não quebrou nada.

## Cenários de Teste Completos

### 10.1: Fluxo Completo de Novo Usuário
- [ ] Acessar home `/`
- [ ] Clicar "Register" → `/account/register`
- [ ] Registrar novo usuário
- [ ] Redirecionado para `/account/login`
- [ ] Fazer login
- [ ] Redirecionado para home `/`
- [ ] Clicar "Manage Profile" → `/profile`
- [ ] Clicar "Change Password" → `/settings/password`
- [ ] Trocar senha
- [ ] Redirecionado para `/profile`
- [ ] Clicar "Manage 2FA" → `/settings/security`
- [ ] Habilitar 2FA
- [ ] Fazer logout
- [ ] Fazer login novamente
- [ ] Redirecionado para `/account/two-factor`
- [ ] Completar 2FA
- [ ] Login bem-sucedido

### 10.2: Fluxo OAuth Google
- [ ] Acessar `/account/login`
- [ ] Clicar "Sign in with Google"
- [ ] Completar OAuth
- [ ] Login bem-sucedido
- [ ] Acessar `/profile`

### 10.3: Fluxo Admin
- [ ] Login como admin
- [ ] Clicar "Admin" no menu → `/admin/users`
- [ ] Clicar "Edit Roles" → `/admin/users/edit?id={userId}`
- [ ] Atualizar role
- [ ] Redirecionado para `/admin/users`
- [ ] Deletar usuário
- [ ] Confirmação exibida

### 10.4: Navegação Global
- [ ] Clicar todos links do menu principal
- [ ] Clicar todos links do dropdown de perfil
- [ ] Verificar que todos carregam corretamente

### 10.5: Acesso Direto via URL
- [ ] Digitar `/account/login` → carrega
- [ ] Digitar `/profile` (autenticado) → carrega
- [ ] Digitar `/profile` (não autenticado) → redireciona para `/account/login`
- [ ] Digitar `/admin/users` (admin) → carrega
- [ ] Digitar `/admin/users` (não admin) → redireciona para `/account/access-denied`
- [ ] Digitar `/privacy` → carrega
- [ ] Digitar `/converter` → carrega

### 10.6: Verificação de Links Quebrados
- [ ] Nenhum link retorna 404
- [ ] Nenhum redirect infinito
- [ ] Nenhum erro no console do browser

## Sequenciamento
- **Bloqueado por:** 6.0, 7.0, 8.0, 9.0 (todos testes individuais e layout)
- **Desbloqueia:** Nenhuma (tarefa final)
- **Paralelizável com:** Nenhuma
- **Lane:** Lane 4 (final)

## Critérios de Sucesso
- [ ] Todos fluxos E2E passam
- [ ] Nenhum link quebrado encontrado
- [ ] Nenhum erro de redirecionamento
- [ ] Aplicação funciona 100% como antes
- [ ] Documentação atualizada (README)

## Dados de Teste Necessários
- Usuário normal sem 2FA
- Usuário normal com 2FA
- Usuário admin
- Usuário master admin
- Conta Google para OAuth

## Estimativa
**Total:** 30-45 minutos

## Ação Final
Após todos testes passarem:
- [ ] Criar PR com todas mudanças
- [ ] Atualizar README com novas convenções de URL
- [ ] Marcar PRD como "Implemented"

---
**Criado:** 2025-01-12
