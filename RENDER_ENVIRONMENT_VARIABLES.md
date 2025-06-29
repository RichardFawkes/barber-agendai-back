# 🚀 **Variáveis de Ambiente - Render.com Deploy**

Configure estas variáveis **EXATAMENTE** assim no dashboard do Render.com:

---

## ✅ **OBRIGATÓRIAS**

### **Ambiente:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000
```

### **Banco PostgreSQL:**
```
DATABASE_URL=postgresql://barber_anotai_bd_user:BGsAZxnqFJ51wy389QcOlaLk91SgzuGy@dpg-d1g9q1nfte5s738d6pfg-a/barber_anotai_bd
```

### **JWT Authentication:**
```
JWT_SECRET_KEY=zcn4QGGlh+135xIe/mj5H+FpsBQryNlj6bbZ1e9Ev7FQhTBGm2w/HHZrFZAYMI/a
JWT_ISSUER=BarbeariaSaaS-Production
JWT_AUDIENCE=BarbeariaSaaS-Users-Production
```

---

## 🔧 **OPCIONAIS**

### **CORS (se tiver frontend):**
```
FRONTEND_URL=https://seu-frontend.vercel.app
```

### **Logs:**
```
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft=Warning
Logging__LogLevel__Microsoft.EntityFrameworkCore=Warning
```

---

## 📋 **Como Configurar no Render.com**

### **Passo a Passo:**

1. **Acesse:** Dashboard do Render.com
2. **Vá para:** Services → [Seu Web Service] → Settings → Environment Variables
3. **Para cada variável abaixo, clique em "Add Environment Variable":**

| Key | Value |
|-----|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:10000` |
| `DATABASE_URL` | `postgresql://barber_anotai_bd_user:BGsAZxnqFJ51wy389QcOlaLk91SgzuGy@dpg-d1g9q1nfte5s738d6pfg-a/barber_anotai_bd` |
| `JWT_SECRET_KEY` | `zcn4QGGlh+135xIe/mj5H+FpsBQryNlj6bbZ1e9Ev7FQhTBGm2w/HHZrFZAYMI/a` |
| `JWT_ISSUER` | `BarbeariaSaaS-Production` |
| `JWT_AUDIENCE` | `BarbeariaSaaS-Users-Production` |

4. **Clique em "Save Changes"** após adicionar todas

---

## 🔑 **Chave JWT Já Gerada**

✅ **Chave gerada com segurança:** `zcn4QGGlh+135xIe/mj5H+FpsBQryNlj6bbZ1e9Ev7FQhTBGm2w/HHZrFZAYMI/a`

**Para gerar uma nova (se quiser):**

### **PowerShell (Windows):**
```powershell
[System.Web.Security.Membership]::GeneratePassword(64, 0)
```

### **Node.js:**
```javascript
console.log(require('crypto').randomBytes(48).toString('base64'))
```

### **Online:**
- [Random.org - Password Generator](https://www.random.org/passwords/?num=1&len=64&format=plain)

---

## ✅ **Verificação de Deploy**

### **Logs Esperados:**
```
=== DATABASE CONFIGURATION ===
Environment: Production
IsProduction: True
DATABASE_URL present: True
DATABASE_URL host: dpg-d1g9q1nfte5s738d6pfg-a
✅ USING: PostgreSQL from DATABASE_URL (Cloud)
PostgreSQL Host: dpg-d1g9q1nfte5s738d6pfg-a:5432
PostgreSQL Database: barber_anotai_bd

=== STARTING DATABASE SETUP ===
Database Provider: Npgsql.EntityFrameworkCore.PostgreSQL
Applying migrations for Npgsql.EntityFrameworkCore.PostgreSQL...
✅ Database migration completed successfully
Starting database seeding...
✅ Database seeding completed successfully
```

### **Health Check:**
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

---

## 🚀 **Próximos Passos**

### **1. Commit e Push:**
```bash
git add .
git commit -m "Configure PostgreSQL for Render.com deployment"
git push origin main
```

### **2. Configure Variáveis no Render.com**
- Use a tabela acima para adicionar cada variável

### **3. Aguarde Deploy**
- O deploy será automático após o push
- Aguarde 5-10 minutos

### **4. Teste a API**
- Health check: `https://sua-api.onrender.com/health`
- Swagger: `https://sua-api.onrender.com/docs`
- Login: Endpoint `/api/auth/login`

---

## 🎯 **Dados de Teste**

**Usuário Admin Padrão:**
```json
{
  "email": "admin@barbearianojoao.com",
  "password": "admin123"
}
```

**Tenant de Teste:**
- **Subdomain:** `joao`
- **Nome:** `Barbearia do João` 