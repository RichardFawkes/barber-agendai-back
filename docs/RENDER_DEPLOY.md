# 🚀 **Deploy no Render.com - BarbeariaSaaS**

Guia completo para fazer deploy da API BarbeariaSaaS no Render.com.

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

### **Environment Variables:**

```env
# Ambiente
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Banco de Dados (será preenchido automaticamente)
DATABASE_URL=postgres://barbearia_user:password@dpg-xxxxx/barbearia_saas

# JWT Settings
JWT_SECRET_KEY=SUA-CHAVE-SUPER-SECRETA-256-BITS-AQUI-PRODUCTION-2024
JWT_ISSUER=BarbeariaSaaS-Production
JWT_AUDIENCE=BarbeariaSaaS-Users-Production

# CORS (URL do seu frontend)
FRONTEND_URL=https://seu-frontend.vercel.app

# Logs
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft=Warning
```

### **⚠️ Importante:**
- Substitua `SUA-CHAVE-SUPER-SECRETA-256-BITS-AQUI-PRODUCTION-2024` por uma chave real
- Substitua `https://seu-frontend.vercel.app` pela URL real do seu frontend
- O `DATABASE_URL` será preenchido automaticamente pelo Render

---

## 🔑 **4. Gerar Chave JWT Segura**

Use um gerador de chaves ou PowerShell/Terminal:

### **PowerShell:**
```powershell
[System.Web.Security.Membership]::GeneratePassword(64, 0)
```

### **Terminal Linux/macOS:**
```bash
openssl rand -base64 64
```

### **Online:**
- [Random.org](https://www.random.org/passwords/?num=1&len=64&format=plain)

---

## 🐳 **5. Ajustar Dockerfile para Render**

O Dockerfile já está configurado, mas certifique-se de que a porta está correta:

```dockerfile
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
```

---

## 🚀 **6. Fazer Deploy**

### **Primeira vez:**

1. Configure todas as variáveis de ambiente
2. Clique em **"Create Web Service"**
3. Aguarde o build (pode levar 5-10 minutos)
4. Acesse a URL fornecida pelo Render

### **Próximos deploys:**

- Basta fazer push para a branch `main`
- Deploy automático será acionado

---

## 📡 **7. Testar a API**

### **Health Check:**
```bash
curl https://sua-api.onrender.com/health
```

**Resposta esperada:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-12-25T10:00:00Z",
  "environment": "Production",
  "database": "PostgreSQL"
}
```

### **Swagger:**
```
https://sua-api.onrender.com/docs
```

### **Login Test:**
```bash
curl -X POST https://sua-api.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@barbearianojoao.com",
    "password": "admin123"
  }'
```

---

## 🔧 **8. Configurações Avançadas**

### **Health Check (Opcional):**
```
Health Check Path: /health
```

### **Custom Domain:**
No dashboard do Render → Settings → Custom Domains

### **Auto-Deploy:**
Já configurado por padrão quando conecta o GitHub

---

## 🐛 **9. Troubleshooting**

### **Build falha:**
```bash
# Ver logs no dashboard do Render
# Ou verificar se todas as dependências estão no .csproj
```

### **Erro de conexão com banco:**
- Verifique se o `DATABASE_URL` está correto
- Confirme se o banco PostgreSQL está rodando
- Teste a conexão manualmente

### **Erro 500:**
```bash
# Ver logs da aplicação no dashboard
# Verificar se todas as variáveis de ambiente estão configuradas
```

### **CORS Error:**
- Verifique se `FRONTEND_URL` está configurada
- Confirme se o domínio do frontend está correto

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

## 💰 **11. Custos**

### **Free Tier:**
- **Web Service**: 750 horas/mês gratuitas
- **PostgreSQL**: 1GB storage gratuito
- **Limitações**: Sleep após 15min inatividade

### **Paid Plans:**
- **Starter**: $7/mês - Sem sleep
- **Standard**: $25/mês - Mais recursos

---

## 🔗 **12. URLs Importantes**

```
API Base URL: https://barbearia-saas-api.onrender.com
Swagger: https://barbearia-saas-api.onrender.com/docs
Health: https://barbearia-saas-api.onrender.com/health
```

---

## 📝 **13. Checklist de Deploy**

- [ ] ✅ Banco PostgreSQL criado
- [ ] ✅ Variáveis de ambiente configuradas
- [ ] ✅ JWT Secret Key gerada
- [ ] ✅ FRONTEND_URL configurada
- [ ] ✅ Web Service criado
- [ ] ✅ Build completado com sucesso
- [ ] ✅ Health check funcionando
- [ ] ✅ Swagger acessível
- [ ] ✅ Login funcionando
- [ ] ✅ CORS configurado

---

<div align="center">

**🎉 Deploy realizado com sucesso!**

*Sua API BarbeariaSaaS está rodando em produção.*

</div> 