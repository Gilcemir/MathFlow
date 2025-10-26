# PRD: Reestruturação do Projeto e Implementação de Stack de Observabilidade

**Versão:** 1.0  
**Data:** 2025-10-26  
**Autor:** Equipe MathFlow  
**Status:** Aprovado

---

## 1. Visão Geral

### 1.1 Contexto

O MathFlow é uma aplicação ASP.NET Core Razor Pages que converte documentos OMML (Office MathML) para MathML. Atualmente, o projeto possui uma estrutura simples e funcional, mas carece de:

- Organização escalável de pastas
- Infraestrutura de observabilidade (logs, métricas, traces)
- Containerização adequada
- Separação clara entre aplicação e infraestrutura de suporte

### 1.2 Problema

A estrutura atual do projeto não suporta adequadamente:

1. **Escalabilidade**: Todos os arquivos estão na raiz, dificultando crescimento
2. **Observabilidade**: Não há coleta estruturada de logs, métricas ou traces
3. **Desenvolvimento Local**: Falta de ambiente containerizado para desenvolvimento
4. **Produção**: Ausência de Dockerfile e orquestração de serviços
5. **Testes**: Não há estrutura definida para testes unitários e de integração

### 1.3 Objetivos

**Objetivo Principal**: Reestruturar o projeto MathFlow para suportar crescimento, observabilidade completa e deployment containerizado.

**Objetivos Específicos**:

1. Reorganizar estrutura de pastas seguindo padrões .NET modernos
2. Implementar stack completa de observabilidade (OpenTelemetry, Prometheus, Loki, Grafana)
3. Criar infraestrutura Docker separada para aplicação e serviços de suporte
4. Integrar aplicação com stack de observabilidade
5. Preparar estrutura para testes futuros
6. Adicionar PostgreSQL para persistência de dados

---

## 2. Requisitos Funcionais

### RF01: Reestruturação de Pastas

**Prioridade**: Alta  
**Descrição**: Reorganizar projeto seguindo estrutura escalável

**Estrutura Alvo**:
```
MathFlow/
├── src/
│   └── MathFlow/              # Aplicação principal
├── tests/
│   ├── MathFlow.UnitTests/
│   └── MathFlow.IntegrationTests/
├── docker/
│   ├── app/                   # Dockerfile da aplicação
│   └── infra/                 # Configurações de infraestrutura
├── docker-compose.yml         # Orquestração completa
├── .env.example
└── README.md
```

**Critérios de Aceitação**:
- Todos os arquivos da aplicação movidos para `src/MathFlow/`
- Estrutura de pastas `tests/` criada
- Estrutura `docker/` criada com subpastas `app/` e `infra/`
- Aplicação compila e executa após reestruturação

### RF02: Containerização da Aplicação

**Prioridade**: Alta  
**Descrição**: Criar Dockerfile multi-stage para aplicação ASP.NET Core

**Requisitos**:
- Dockerfile otimizado com build e runtime stages
- Suporte a Node.js para scripts de conversão
- Cópia correta de `node_modules` e scripts JavaScript
- Imagem final leve (baseada em `mcr.microsoft.com/dotnet/aspnet:9.0`)

**Critérios de Aceitação**:
- Dockerfile cria imagem funcional da aplicação
- Aplicação inicia corretamente no container
- Scripts Node.js funcionam dentro do container
- Imagem final < 500MB

### RF03: Stack de Observabilidade - OpenTelemetry Collector

**Prioridade**: Alta  
**Descrição**: Configurar OTEL Collector para receber telemetria da aplicação

**Requisitos**:
- Receber traces, logs e métricas via OTLP (gRPC port 4317)
- Exportar logs para Loki
- Exportar métricas para Prometheus
- Expor métricas próprias do collector

**Critérios de Aceitação**:
- OTEL Collector recebe telemetria da aplicação
- Logs são enviados para Loki
- Métricas são enviadas para Prometheus
- Health check do collector responde corretamente

### RF04: Stack de Observabilidade - Loki (Logs)

**Prioridade**: Alta  
**Descrição**: Configurar Loki para armazenamento e consulta de logs

**Requisitos**:
- Receber logs do OTEL Collector
- Configuração otimizada para ambiente local
- Retenção de logs configurável

**Critérios de Aceitação**:
- Loki recebe e armazena logs
- Logs são consultáveis via Grafana
- API Loki responde em `http://loki:3100`

### RF05: Stack de Observabilidade - Prometheus (Métricas)

**Prioridade**: Alta  
**Descrição**: Configurar Prometheus para coleta de métricas

**Requisitos**:
- Scrape de métricas do OTEL Collector
- Scrape de métricas da aplicação (se exposta diretamente)
- Retenção de 15 dias de métricas

**Critérios de Aceitação**:
- Prometheus coleta métricas do OTEL Collector
- Métricas ASP.NET Core disponíveis (requests, duration, errors)
- UI Prometheus acessível em `http://localhost:9090`

### RF06: Stack de Observabilidade - Grafana (Visualização)

**Prioridade**: Alta  
**Descrição**: Configurar Grafana com datasources e dashboards pré-configurados

**Requisitos**:
- Datasource Prometheus configurado automaticamente
- Datasource Loki configurado automaticamente
- Dashboard básico de métricas ASP.NET Core
- Dashboard básico de logs

**Critérios de Aceitação**:
- Grafana acessível em `http://localhost:3000`
- Datasources conectados e funcionais
- Dashboards exibem dados da aplicação

### RF07: Banco de Dados PostgreSQL

**Prioridade**: Alta  
**Descrição**: Adicionar PostgreSQL para persistência de dados

**Requisitos**:
- PostgreSQL 16 ou superior
- Volume persistente para dados
- Credenciais configuráveis via `.env`

**Critérios de Aceitação**:
- PostgreSQL acessível em `localhost:5432`
- Dados persistem entre restarts
- Aplicação conecta com sucesso ao banco

### RF08: Integração da Aplicação com OpenTelemetry

**Prioridade**: Alta  
**Descrição**: Instrumentar aplicação ASP.NET Core com OpenTelemetry SDK

**Requisitos**:
- Logs estruturados enviados via OTLP
- Traces de requisições HTTP
- Métricas ASP.NET Core (Kestrel, Hosting, Routing)
- Métricas de runtime (.NET)
- Endpoint OTLP configurável via `appsettings.json`

**Critérios de Aceitação**:
- Logs aparecem no Grafana via Loki
- Traces aparecem no Grafana (se Tempo configurado) ou logs
- Métricas aparecem no Prometheus
- Aplicação funciona sem OTEL Collector (fallback gracioso)

---

## 3. Requisitos Não Funcionais

### RNF01: Performance

- Overhead de observabilidade < 5% no tempo de resposta
- Imagem Docker da aplicação < 500MB
- Startup da aplicação < 10 segundos

### RNF02: Confiabilidade

- Aplicação deve funcionar mesmo se OTEL Collector estiver indisponível
- Logs devem ser bufferizados se Loki estiver temporariamente indisponível
- Restart automático de containers em caso de falha

### RNF03: Manutenibilidade

- Configurações externalizadas via `.env`
- Documentação clara de como executar localmente
- Separação clara entre infra de desenvolvimento e produção

### RNF04: Segurança

- Credenciais não devem ser commitadas (usar `.env.example`)
- PostgreSQL com senha forte configurável
- Grafana com credenciais padrão documentadas

---

## 4. Escopo

### 4.1 Dentro do Escopo

- Reestruturação completa de pastas
- Dockerfile da aplicação
- Docker Compose para aplicação + infra
- Configuração completa de OTEL, Prometheus, Loki, Grafana
- PostgreSQL containerizado
- Integração da aplicação com OpenTelemetry
- Documentação de setup local

### 4.2 Fora do Escopo

- Implementação de testes (estrutura será criada, mas sem testes)
- CI/CD pipelines (estrutura de pastas preparada, mas sem workflows)
- Dashboards Grafana customizados avançados
- Alertas no Prometheus
- Backup automatizado do PostgreSQL
- Deploy em produção (Kubernetes, cloud, etc.)
- Tempo (distributed tracing backend) - apenas logs e métricas

---

## 5. Dependências e Integrações

### 5.1 Dependências Externas

- **Docker**: Versão 24.0+
- **Docker Compose**: Versão 2.20+
- **.NET SDK**: 9.0
- **Node.js**: 18+ (para build local)

### 5.2 Integrações

- **Aplicação → OTEL Collector**: OTLP/gRPC (port 4317)
- **OTEL Collector → Loki**: HTTP API
- **OTEL Collector → Prometheus**: Exposição de métricas (port 8889)
- **Prometheus → OTEL Collector**: Scrape de métricas
- **Grafana → Prometheus**: Datasource
- **Grafana → Loki**: Datasource
- **Aplicação → PostgreSQL**: Npgsql (connection string)

---

## 6. Métricas de Sucesso

### 6.1 Métricas Técnicas

- **Tempo de setup local**: < 5 minutos (após `docker compose up`)
- **Cobertura de observabilidade**: 100% das requisições HTTP logadas e traced
- **Disponibilidade de métricas**: 99.9% (considerando ambiente local)

### 6.2 Métricas de Qualidade

- **Documentação**: README completo com instruções claras
- **Facilidade de uso**: Desenvolvedor novo consegue rodar projeto em < 10 minutos
- **Manutenibilidade**: Configurações centralizadas e documentadas

---

## 7. Fases de Implementação

### Fase 1: Reestruturação (Prioridade Alta)
- Mover arquivos para `src/MathFlow/`
- Criar estrutura de pastas `docker/` e `tests/`
- Atualizar caminhos no `.csproj`

### Fase 2: Containerização da Aplicação (Prioridade Alta)
- Criar `docker/app/Dockerfile`
- Criar `docker-compose.yml` básico
- Testar build e execução

### Fase 3: Infraestrutura de Observabilidade (Prioridade Alta)
- Configurar OTEL Collector
- Configurar Loki
- Configurar Prometheus
- Configurar Grafana com datasources
- Criar `docker-compose.infra.yml`

### Fase 4: Integração com Aplicação (Prioridade Alta)
- Adicionar pacotes OpenTelemetry ao projeto
- Configurar instrumentação no `Program.cs`
- Testar telemetria end-to-end

### Fase 5: PostgreSQL (Prioridade Média)
- Adicionar PostgreSQL ao `docker-compose.infra.yml`
- Configurar connection string
- Testar conectividade

### Fase 6: Documentação (Prioridade Alta)
- Atualizar README
- Criar `.env.example`
- Documentar troubleshooting comum

---

## 8. Riscos e Mitigações

### Risco 1: Complexidade de Configuração OTEL
**Probabilidade**: Média  
**Impacto**: Alto  
**Mitigação**: Usar configurações testadas de projeto anterior como base

### Risco 2: Overhead de Performance
**Probabilidade**: Baixa  
**Impacto**: Médio  
**Mitigação**: Configurar sampling de traces, usar exporters assíncronos

### Risco 3: Incompatibilidade de Versões
**Probabilidade**: Baixa  
**Impacto**: Médio  
**Mitigação**: Fixar versões de todas as imagens Docker

### Risco 4: Complexidade de Manutenção
**Probabilidade**: Média  
**Impacto**: Médio  
**Mitigação**: Documentação detalhada, configurações comentadas

---

## 9. Perguntas em Aberto - RESOLVIDAS

1. **Retenção de Logs**: ✅ Configuração local padrão (7 dias). Em produção será configurado diretamente no Grafana.
2. **Dashboards Grafana**: ✅ Dashboards customizados serão criados em tarefas futuras. Métricas planejadas: quantidade de fórmulas por arquivo, requisições por usuário, etc.
3. **Alertas**: ✅ Não serão configurados nesta fase.
4. **Tempo**: ✅ Não será adicionado por simplicidade. Apenas logs e métricas.
5. **Migrations**: ✅ Não há domínio persistido ainda. PostgreSQL será configurado mas não utilizado inicialmente. Migrations ficam para o futuro.

## 10. Tarefas Adicionais

- **Makefile**: Criar Makefile na raiz do projeto com comandos úteis para desenvolvimento (build, run, docker up/down, logs, etc.)

---

## 11. Aprovações

- [x] Product Owner
- [x] Tech Lead
- [x] DevOps Engineer
- [x] Desenvolvedor Principal

**Status**: ✅ APROVADO (2025-10-26)

---

## Anexos

### A. Referências de Configuração

Configurações baseadas em projeto anterior (MathML Converter) com stack similar:
- OTEL Collector 0.89.0
- Loki (latest)
- Prometheus (latest)
- Grafana (latest)
- PostgreSQL 16

### B. Estrutura de Rede Docker

Todos os serviços compartilharão a rede `mathflow-network` para comunicação interna.

### C. Portas Expostas

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| Aplicação | 5124 | HTTP API |
| OTEL Collector | 4317 | OTLP gRPC |
| OTEL Collector | 8888 | Métricas do collector |
| OTEL Collector | 8889 | Prometheus exporter |
| Prometheus | 9090 | UI Prometheus |
| Loki | 3100 | API Loki |
| Grafana | 3000 | UI Grafana |
| PostgreSQL | 5432 | Database |
