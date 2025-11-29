# Identity Migrations

Este diretório contém as migrations do Entity Framework Core para o schema de Identity.

## Comandos Úteis

### Comandos do Makefile (Recomendado)

```bash
# Aplicar migrações pendentes
make migrate

# Criar nova migração
make migrate-create NAME=NomeDaMigration

# Resetar banco (apaga e recria do zero)
make migrate-reset
```

### Comandos Manuais (Alternativa)

#### Criar Nova Migration
```bash
cd src/MathFlow
dotnet ef migrations add <NomeDaMigration> \
  --context ApplicationDbContext \
  --output-dir Infrastructure/IdentityServer/Data/Migrations
```

#### Aplicar Migrations
```bash
cd src/MathFlow
dotnet ef database update --context ApplicationDbContext
```

**⚠️ IMPORTANTE**: As migrações **NÃO** são aplicadas automaticamente ao iniciar a aplicação.  
Você deve executar `make migrate` após resetar o banco ou criar novas migrações.

### Reverter Migration
```bash
# Reverter última migration
dotnet ef database update <MigrationAnterior> --context ApplicationDbContext

# Remover migration não aplicada
dotnet ef migrations remove --context ApplicationDbContext
```

### Gerar Script SQL
```bash
# Script completo
dotnet ef migrations script --context ApplicationDbContext \
  --output identity-schema.sql

# Script idempotente (produção)
dotnet ef migrations script --context ApplicationDbContext \
  --idempotent --output identity-schema-idempotent.sql
```

## Migrations Existentes

- `20251102175606_InitialIdentity`: Schema inicial com tabelas Users, Roles, UserRoles, UserClaims, UserLogins, RoleClaims, UserTokens
- `20251112214430_AddDisplayNameToApplicationUser`: Adiciona campo DisplayName à tabela Users

## Produção

Em produção, aplicar migrations via script SQL:
1. Gerar script idempotente
2. Revisar script
3. Aplicar via psql ou ferramenta de deploy
4. Validar schema criado
