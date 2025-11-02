# Google OAuth Setup

## 1. Criar Projeto no Google Cloud Console

1. Acesse https://console.cloud.google.com/
2. Crie novo projeto "MathFlow"
3. Habilite Google+ API

## 2. Configurar OAuth Consent Screen

1. OAuth consent screen → External
2. App name: MathFlow
3. User support email: seu email
4. Developer contact: seu email

## 3. Criar Credenciais

1. Credentials → Create Credentials → OAuth client ID
2. Application type: Web application
3. Name: MathFlow Web Client
4. Authorized redirect URIs:
   - http://localhost:5124/signin-google (dev)
   - https://mathflow.com/signin-google (prod)

## 4. Configurar appsettings

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    }
  }
}
```

## 5. Variáveis de Ambiente (Produção)

```bash
export GOOGLE_CLIENT_ID="..."
export GOOGLE_CLIENT_SECRET="..."
```

## 6. Testar Localmente

1. Adicionar credenciais ao `appsettings.Development.json`
2. Executar aplicação: `dotnet run`
3. Acessar http://localhost:5124/Account/Login
4. Clicar em "Sign in with Google"
5. Autorizar aplicação
6. Verificar redirecionamento e criação de conta
