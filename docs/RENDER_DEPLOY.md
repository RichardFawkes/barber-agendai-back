# 🚀 **Deploy no Render.com - BarbeariaSaaS**

Guia completo para fazer deploy da API BarbeariaSaaS no Render.com.

---

## 🔗 **Configuração de Banco de Dados Inteligente**

A aplicação agora detecta automaticamente o ambiente e usa o banco apropriado:

- 🌐 **Render.com/Heroku**: PostgreSQL (via `DATABASE_URL`)
- 💻 **Desenvolvimento Local**: SQL Server LocalDB
- 🔄 **Fallback Universal**: SQLite

---

## 📋 **Pré-requisitos**

- Conta no [Render.com](https://render.com)
- Repositório no GitHub com o código
- Projeto configurado com PostgreSQL support

---

## 🗄️ **1. Criar Banco PostgreSQL**

### **No Dashboard do Render:**

1. Acesse [Dashboard do Render](https://dashboard.render.com)
2. Clique em **"New +"** → **"PostgreSQL"**
3. Configure:
   - **Name**: `barbearia-saas-db`
   - **Database**: `barbearia_saas`
   - **User**: `barbearia_user`
   - **Region**: `Oregon (US West)` (ou mais próximo)
   - **PostgreSQL Version**: `16`
   - **Plan**: `Free` (para teste) ou `Starter` (produção)

4. Clique em **"Create Database"**
5. **Anote a URL interna** que será algo como:
   ```
   postgres://barbearia_user:password@dpg-xxxxx/barbearia_saas
   ```

---

## 🌐 **2. Criar Web Service**

### **No Dashboard do Render:**

1. Clique em **"New +"** → **"Web Service"**
2. Conecte seu repositório do GitHub
3. Configure:
   - **Name**: `barbearia-saas-api`
   - **Region**: `Oregon (US West)` (mesmo do banco)
   - **Branch**: `main`
   - **Root Directory**: `.` (raiz do projeto)
   - **Environment**: `Docker`
   - **Dockerfile Path**: `Dockerfile`

---

## ⚙️ **3. Configurar Variáveis de Ambiente**

### **Environment Variables Essenciais:**

```env
# Ambiente
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Banco de Dados (será preenchido automaticamente pelo Render)
DATABASE_URL=postgres://barbearia_user:password@dpg-xxxxx/barbearia_saas

# JWT Settings (OBRIGATÓRIO)
JWT_SECRET_KEY=SUA-CHAVE-SUPER-SECRETA-256-BITS-AQUI-PRODUCTION-2024
JWT_ISSUER=BarbeariaSaaS-Production
JWT_AUDIENCE=BarbeariaSaaS-Users-Production

# CORS (URL do seu frontend - OPCIONAL)
FRONTEND_URL=https://seu-frontend.vercel.app

# Logs (OPCIONAL)
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft=Warning
```

### **⚠️ Importante:**
- ✅ **DATABASE_URL** - Preenchido automaticamente pelo Render
- 🔑 **JWT_SECRET_KEY** - OBRIGATÓRIO gerar uma chave segura
- 🌐 **FRONTEND_URL** - Adicionar URL do seu frontend para CORS
- 📝 As demais são opcionais

---

## 🔑 **4. Gerar Chave JWT Segura**

**ESCOLHA UMA OPÇÃO:**

### **Opção 1 - PowerShell (Windows):**
```powershell
[System.Web.Security.Membership]::GeneratePassword(64, 0)
```

### **Opção 2 - Terminal (Linux/macOS):**
```bash
openssl rand -base64 64
```

### **Opção 3 - Node.js:**
```javascript
console.log(require('crypto').randomBytes(48).toString('base64'));
```

### **Opção 4 - Online (Rápido):**
- [Random.org - Password Generator](https://www.random.org/passwords/?num=1&len=64&format=plain)

---

## 🐳 **5. Verificar Dockerfile**

O Dockerfile já está configurado corretamente:

```dockerfile
# Porta do Render.com
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Health check configurado
HEALTHCHECK CMD curl -f http://localhost:10000/health || exit 1
```

---

## 🚀 **6. Fazer Deploy**

### **Primeira vez:**

1. ✅ Configure todas as variáveis de ambiente **OBRIGATÓRIAS**
2. ✅ Clique em **"Create Web Service"**
3. ⏳ Aguarde o build (5-10 minutos)
4. 🎉 Acesse a URL fornecida pelo Render

### **Próximos deploys:**

- Basta fazer push para a branch `main`
- Deploy automático será acionado

---

## 🔍 **7. Verificar Deploy**

### **A. Health Check:**
```bash
curl https://sua-api.onrender.com/health
```

**Resposta esperada:**
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-25T10:00:00Z",
  "environment": "Production",
  "database": "PostgreSQL (Cloud)",
  "databaseProvider": "Npgsql.EntityFrameworkCore.PostgreSQL",
  "hasDatabaseUrl": true
}
```

### **B. Swagger (Documentação):**
```
https://sua-api.onrender.com/docs
```

### **C. Teste de Login:**
```bash
curl -X POST https://sua-api.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@barbearianojoao.com",
    "password": "admin123"
  }'
```

---

## 🏠 **8. Desenvolvimento Local**

### **Opção 1 - SQL Server LocalDB (Padrão):**
```bash
# Usar configuração padrão
dotnet run --project src/BarbeariaSaaS.API
```

### **Opção 2 - SQLite (Alternativa):**
```bash
# Usar configuração SQLite
dotnet run --project src/BarbeariaSaaS.API --launch-profile "SQLite"
```

Ou copiar `appsettings.SQLite.json` para `appsettings.Development.json`.

---

## 🐛 **9. Troubleshooting**

### **❌ Build Failed:**
```bash
# Ver logs detalhados no dashboard do Render
# Verificar se todas as dependências estão no .csproj
```

### **❌ Database Connection Error:**
- ✅ Verificar se `DATABASE_URL` está configurada
- ✅ Confirmar se PostgreSQL está rodando no Render
- ✅ Verificar nos logs se está detectando PostgreSQL

### **❌ JWT/Auth Error:**
- ✅ Verificar se `JWT_SECRET_KEY` está configurada
- ✅ Confirmar se a chave tem pelo menos 32 caracteres
- ✅ Testar endpoint de login

### **❌ CORS Error:**
- ✅ Adicionar `FRONTEND_URL` com URL do seu frontend
- ✅ Verificar se o domínio está correto (com https://)

### **🔍 Ver Logs em Tempo Real:**
```
Dashboard → Services → sua-api → Logs
```

---

## 📊 **10. Monitoramento**

### **Logs:**
```
Dashboard → Services → sua-api → Logs
```

### **Métricas:**
```
Dashboard → Services → sua-api → Metrics
```

### **Database:**
```
Dashboard → PostgreSQL → barbearia-saas-db → Metrics
```

---

## 💰 **11. Custos (2025)**

### **Free Tier:**
- ✅ **Web Service**: 750 horas/mês gratuitas
- ✅ **PostgreSQL**: 1GB storage gratuito
- ⚠️ **Limitação**: Sleep após 15min inatividade

### **Paid Plans:**
- 💰 **Starter**: $7/mês - Sem sleep
- 💰 **Standard**: $25/mês - Mais recursos

---

## 🔗 **12. URLs de Exemplo**

```
API Base: https://barbearia-saas-api.onrender.com
Health: https://barbearia-saas-api.onrender.com/health
Docs: https://barbearia-saas-api.onrender.com/docs
Login: https://barbearia-saas-api.onrender.com/api/auth/login
```

---

## 📝 **13. Checklist Final**

### **Antes do Deploy:**
- [ ] ✅ Repo no GitHub atualizado
- [ ] ✅ Dockerfile na raiz do projeto
- [ ] ✅ Código compilando localmente

### **No Render.com:**
- [ ] ✅ PostgreSQL criado
- [ ] ✅ Web Service criado e conectado ao GitHub
- [ ] ✅ `DATABASE_URL` configurada automaticamente
- [ ] ✅ `JWT_SECRET_KEY` configurada manualmente
- [ ] ✅ `JWT_ISSUER` e `JWT_AUDIENCE` configuradas
- [ ] ✅ `FRONTEND_URL` configurada (se houver frontend)

### **Após Deploy:**
- [ ] ✅ Build completado sem erro
- [ ] ✅ Health check retornando `"PostgreSQL (Cloud)"`
- [ ] ✅ Swagger acessível em `/docs`
- [ ] ✅ Login funcionando
- [ ] ✅ CORS configurado para frontend

---

## 🎯 **Dicas Importantes**

### **🔧 Para Desenvolvedores:**
```bash
# Ver qual banco está sendo usado
curl localhost:5080/health

# Rodar com SQLite localmente
dotnet run --project src/BarbeariaSaaS.API --configuration SQLite
```

### **🌐 Para Deploy:**
```bash
# Testar antes do commit
dotnet build
dotnet test

# Commit e push
git add .
git commit -m "Deploy ready"
git push origin main
```

---

<div align="center">

**🎉 Deploy Automatizado Configurado!**

*Sua API BarbeariaSaaS agora detecta o ambiente automaticamente.*

**🔄 Local**: SQL Server → **🌐 Render.com**: PostgreSQL → **🔧 Fallback**: SQLite

</div> 