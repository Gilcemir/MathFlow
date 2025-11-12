---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>testing</domain>
<type>testing</type>
<scope>authentication</scope>
<complexity>medium</complexity>
<dependencies>authentication_flow</dependencies>
<unblocks>10.0</unblocks>
</task_context>

# Tarefa 6.0: Testes de Autenticação

## Visão Geral
Testar manualmente todos os fluxos de autenticação com as novas URLs.

## Cenários de Teste

### 6.1: Login Flow
- [ ] Acessar `/account/login` → página carrega
- [ ] Login com credenciais válidas → redireciona corretamente
- [ ] Login com 2FA → redireciona para `/account/two-factor`
- [ ] Completar 2FA → redireciona para destino

### 6.2: Registration Flow
- [ ] Acessar `/account/register` → página carrega
- [ ] Registrar novo usuário → redireciona para `/account/login`
- [ ] Mensagem de sucesso exibida

### 6.3: Logout Flow
- [ ] Fazer logout → redireciona para `/`
- [ ] Sessão encerrada corretamente

### 6.4: OAuth Google
- [ ] Clicar "Sign in with Google" → fluxo OAuth inicia
- [ ] Completar OAuth → login bem-sucedido

### 6.5: Access Control
- [ ] Acessar página protegida sem login → redireciona para `/account/login`
- [ ] Acessar página admin sem permissão → redireciona para `/account/access-denied`

## Sequenciamento
- **Bloqueado por:** 2.0
- **Desbloqueia:** 10.0
- **Paralelizável com:** 7.0, 8.0
- **Lane:** Lane 1

## Critérios de Sucesso
- [ ] Todos cenários passam
- [ ] Nenhum link quebrado
- [ ] Redirects funcionam corretamente

## Estimativa
**Total:** 20 minutos

---
**Criado:** 2025-01-12
