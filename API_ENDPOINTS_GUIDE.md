# 🔥 BarbeariaSaaS API - Guia Completo de Endpoints

## 🚀 **SISTEMA DE AGENDAMENTO COMPLETO**

### **📱 ENDPOINTS PÚBLICOS (Para Sites de Agendamento)**

#### **1. Obter informações do Tenant**
```http
GET /api/tenant/by-subdomain/{subdomain}
```
**Exemplo:** `GET /api/tenant/by-subdomain/barbeariadoze`

#### **2. Listar serviços disponíveis**
```http
GET /api/service/public/{subdomain}
```
**Exemplo:** `GET /api/service/public/barbeariadoze`

#### **3. Obter horários disponíveis** ⭐ **NOVO**
```http
GET /api/booking/available-times/{subdomain}?serviceId={id}&date=2024-01-15
```
**Exemplo:** `GET /api/booking/available-times/barbeariadoze?serviceId=abc123&date=2024-01-15`

#### **4. Criar agendamento público**
```http
POST /api/booking/public/{subdomain}
Content-Type: application/json

{
  "serviceId": "abc123",
  "customerName": "João Silva",
  "customerEmail": "joao@email.com", 
  "customerPhone": "+5511999999999",
  "date": "2024-01-15",
  "time": "10:00",
  "notes": "Observações opcionais"
}
```

---

### **🔐 ENDPOINTS DE DASHBOARD (Autenticação obrigatória)**

#### **🏢 Gerenciamento de Tenants**

##### **Criar nova barbearia**
```http
POST /api/tenant/create
Content-Type: application/json

{
  "name": "Barbearia Exemplo",
  "subdomain": "exemplobarber",
  "phone": "+5511999999999",
  "email": "contato@exemplo.com",
  "website": "www.exemplo.com",
  "address": "Rua Exemplo, 123 - Centro",
  "description": "A melhor barbearia da cidade",
  "primaryColor": "#FF0000",
  "secondaryColor": "#00FF00",
  "adminEmail": "admin@exemplo.com",
  "adminName": "Admin Exemplo",
  "adminPassword": "MinhaSenh@123"
}
```

#### **📊 Dashboard e Estatísticas**

##### **Obter estatísticas**
```http
GET /api/dashboard/stats
Authorization: Bearer {jwt_token}
```

##### **Listar agendamentos** ⭐ **NOVO**
```http
GET /api/dashboard/bookings/{tenantId}?startDate=2024-01-01&endDate=2024-01-31&status=confirmed
Authorization: Bearer {jwt_token}
```

##### **Agendamentos de hoje** ⭐ **NOVO**
```http
GET /api/dashboard/bookings/today
Authorization: Bearer {jwt_token}
```

#### **💇‍♂️ Gerenciamento de Serviços**

##### **Criar novo serviço** ⭐ **NOVO**
```http
POST /api/service
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "tenantId": "abc123",
  "name": "Corte + Barba",
  "description": "Corte completo com barba",
  "price": 45.00,
  "durationMinutes": 60,
  "color": "#3B82F6"
}
```

##### **Listar todos os serviços** ⭐ **NOVO**
```http
GET /api/service/tenant/{tenantId}
Authorization: Bearer {jwt_token}
```

#### **🔐 Autenticação**

##### **Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@exemplo.com",
  "password": "MinhaSenh@123"
}
```

---

## **🧪 COMO TESTAR**

### **1. Teste Básico (Endpoints Públicos)**
```powershell
# Execute o script de teste
.\test_complete_api.ps1
```

### **2. Teste Completo (Com Dashboard)**
```powershell
# 1. Criar tenant
$tenant = Invoke-RestMethod -Uri "https://localhost:7230/api/tenant/create" -Method POST -Body $tenantData -ContentType "application/json" -SkipCertificateCheck

# 2. Fazer login
$login = Invoke-RestMethod -Uri "https://localhost:7230/api/auth/login" -Method POST -Body $loginData -ContentType "application/json" -SkipCertificateCheck

# 3. Usar token nos headers
$headers = @{ Authorization = "Bearer $($login.token)" }

# 4. Testar endpoints autenticados
$stats = Invoke-RestMethod -Uri "https://localhost:7230/api/dashboard/stats" -Headers $headers -SkipCertificateCheck
```

---

## **📋 ENDPOINTS POR CATEGORIA**

### **✅ IMPLEMENTADOS E FUNCIONANDO**

**🌐 Públicos:**
- ✅ Obter tenant por subdomínio
- ✅ Listar serviços públicos  
- ✅ Horários disponíveis
- ✅ Criar agendamento público

**🔐 Dashboard:**
- ✅ Criar tenant com admin
- ✅ Login/autenticação
- ✅ Estatísticas do dashboard
- ✅ Listar agendamentos (com filtros)
- ✅ Agendamentos de hoje
- ✅ Criar serviços
- ✅ Listar todos os serviços

### **🔄 PRÓXIMAS FUNCIONALIDADES (Sugestões)**

**📝 CRUD Avançado:**
- [ ] Editar/deletar serviços
- [ ] Cancelar/reagendar agendamentos
- [ ] Gerenciar horários de funcionamento
- [ ] Upload de logo da barbearia

**📊 Dashboard Avançado:**
- [ ] Relatórios de faturamento
- [ ] Gráficos de agendamentos
- [ ] Gestão de clientes
- [ ] Notificações

**💳 Financeiro:**
- [ ] Integração com pagamentos
- [ ] Controle de comissões
- [ ] Relatórios financeiros

---

## **🚀 URL PARA PRODUÇÃO**

**Base URL:** `https://barber-agendai-back.onrender.com`

**Documentação:** `https://barber-agendai-back.onrender.com/docs`

**Health Check:** `https://barber-agendai-back.onrender.com/health`

---

**🎉 API COMPLETA E FUNCIONANDO!** 

O sistema agora possui todos os endpoints essenciais para:
- ✅ **Sites de agendamento público** funcionais
- ✅ **Dashboard administrativo** completo
- ✅ **Sistema de autenticação** robusto
- ✅ **CRUD de serviços** implementado
- ✅ **Listagem e filtros** de agendamentos 