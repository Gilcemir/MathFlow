# Identity Migrations

Este diretório contém as migrations do Entity Framework Core para o schema de Identity.

## Comandos Úteis

### Criar Nova Migration
```bash
cd src/MathFlow
dotnet ef migrations add <NomeDaMigration> \
  --context ApplicationDbContext \
  --output-dir Infrastructure/IdentityServer/Data/Migrations
```

### Aplicar Migrations
```bash
# Desenvolvimento (automático no startup)
dotnet run

# Manual
dotnet ef database update --context ApplicationDbContext
```

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

## Migrations Aplicadas

- `InitialIdentity`: Schema inicial com tabelas Users, Roles, UserRoles, UserClaims, UserLogins, RoleClaims, UserTokens

## Produção

Em produção, aplicar migrations via script SQL:
1. Gerar script idempotente
2. Revisar script
3. Aplicar via psql ou ferramenta de deploy
4. Validar schema criado
