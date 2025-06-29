# 🚀 BarbeariaSaaS API - Guia de Integração Frontend

## 📋 **CONFIGURAÇÃO INICIAL**

### **Base URLs**
```javascript
const API_CONFIG = {
  development: 'https://localhost:7230',
  production: 'https://barber-agendai-back.onrender.com'
}

const BASE_URL = process.env.NODE_ENV === 'production' 
  ? API_CONFIG.production 
  : API_CONFIG.development
```

### **Headers Padrão**
```javascript
const defaultHeaders = {
  'Content-Type': 'application/json',
  'Accept': 'application/json'
}

// Para endpoints autenticados
const authHeaders = {
  ...defaultHeaders,
  'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
}
```

---

## 🌐 **ENDPOINTS PÚBLICOS (Site de Agendamento)**

### **1. Obter Informações da Barbearia**

```javascript
// GET /api/tenant/by-subdomain/{subdomain}
async function getTenantInfo(subdomain) {
  try {
    const response = await fetch(`${BASE_URL}/api/tenant/by-subdomain/${subdomain}`, {
      method: 'GET',
      headers: defaultHeaders
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao obter tenant:', error)
    throw error
  }
}

// Exemplo de resposta
const tenantResponse = {
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Barbearia do Zé",
  "subdomain": "barbeariadoze",
  "email": "contato@barbeariadoze.com",
  "phone": "+5511999999999",
  "address": "Rua das Flores, 123",
  "website": "https://www.barbeariadoze.com",
  "branding": {
    "colors": {
      "primary": "#0f172a",
      "accent": "#fbbf24"
    },
    "logo": {
      "url": "https://example.com/logo.png",
      "alt": "Logo Barbearia do Zé"
    }
  },
  "status": "active",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### **2. Listar Serviços Disponíveis**

```javascript
// GET /api/service/public/{subdomain}
async function getPublicServices(subdomain) {
  try {
    const response = await fetch(`${BASE_URL}/api/service/public/${subdomain}`, {
      method: 'GET',
      headers: defaultHeaders
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao obter serviços:', error)
    throw error
  }
}

// Exemplo de resposta
const servicesResponse = [
  {
    "id": "service-123",
    "name": "Corte Tradicional",
    "description": "Corte clássico masculino",
    "price": 25.00,
    "durationMinutes": 30,
    "color": "#3B82F6",
    "isActive": true
  },
  {
    "id": "service-456",
    "name": "Barba",
    "description": "Aparar e modelar barba",
    "price": 15.00,
    "durationMinutes": 20,
    "color": "#10B981",
    "isActive": true
  }
]
```

### **3. Obter Horários Disponíveis**

```javascript
// GET /api/booking/available-times/{subdomain}?serviceId={id}&date={date}
async function getAvailableTimes(subdomain, serviceId, date) {
  try {
    const params = new URLSearchParams({
      serviceId: serviceId,
      date: date // formato: YYYY-MM-DD
    })
    
    const response = await fetch(`${BASE_URL}/api/booking/available-times/${subdomain}?${params}`, {
      method: 'GET',
      headers: defaultHeaders
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao obter horários:', error)
    throw error
  }
}

// Exemplo de uso
const availableTimes = await getAvailableTimes('barbeariadoze', 'service-123', '2024-01-15')
// Resposta: ["08:00", "08:30", "09:00", "09:30", "10:00", "14:00", "14:30"]
```

### **4. Criar Agendamento Público**

```javascript
// POST /api/booking/public/{subdomain}
async function createPublicBooking(subdomain, bookingData) {
  try {
    const response = await fetch(`${BASE_URL}/api/booking/public/${subdomain}`, {
      method: 'POST',
      headers: defaultHeaders,
      body: JSON.stringify(bookingData)
    })
    
    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(`HTTP ${response.status}: ${JSON.stringify(errorData)}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao criar agendamento:', error)
    throw error
  }
}

// Exemplo de uso
const bookingData = {
  serviceId: "service-123",
  customerName: "João Silva",
  customerEmail: "joao@email.com",
  customerPhone: "+5511999999999",
  date: "2024-01-15",
  time: "10:00",
  notes: "Prefiro corte baixo nas laterais"
}

const booking = await createPublicBooking('barbeariadoze', bookingData)

// Exemplo de resposta
const bookingResponse = {
  "id": "booking-789",
  "date": "2024-01-15",
  "time": "10:00",
  "status": "confirmed",
  "customerName": "João Silva",
  "customerEmail": "joao@email.com",
  "customerPhone": "+5511999999999",
  "serviceName": "Corte Tradicional",
  "servicePrice": 25.00,
  "serviceDuration": 30,
  "notes": "Prefiro corte baixo nas laterais",
  "createdAt": "2024-01-10T15:30:00Z"
}
```

---

## 🔐 **ENDPOINTS DE DASHBOARD (Autenticação Obrigatória)**

### **1. Autenticação**

```javascript
// POST /api/auth/login
async function login(email, password) {
  try {
    const response = await fetch(`${BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: defaultHeaders,
      body: JSON.stringify({ email, password })
    })
    
    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(`Login failed: ${errorData.message || 'Invalid credentials'}`)
    }
    
    const data = await response.json()
    
    // Salvar token no localStorage
    localStorage.setItem('jwt_token', data.token)
    localStorage.setItem('user_data', JSON.stringify(data.user))
    
    return data
  } catch (error) {
    console.error('Erro no login:', error)
    throw error
  }
}

// Exemplo de resposta
const loginResponse = {
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "user-123",
    "name": "Admin Barbearia",
    "email": "admin@barbeariadoze.com",
    "role": "tenant_admin",
    "tenantId": "tenant-123"
  },
  "expiresIn": "24h"
}

// Logout
function logout() {
  localStorage.removeItem('jwt_token')
  localStorage.removeItem('user_data')
}

// Verificar se está autenticado
function isAuthenticated() {
  return !!localStorage.getItem('jwt_token')
}
```

### **2. Estatísticas do Dashboard**

```javascript
// GET /api/dashboard/stats
async function getDashboardStats() {
  try {
    const response = await fetch(`${BASE_URL}/api/dashboard/stats`, {
      method: 'GET',
      headers: authHeaders
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao obter estatísticas:', error)
    throw error
  }
}

// Exemplo de resposta
const statsResponse = {
  "totalBookings": 150,
  "todayBookings": 8,
  "totalRevenue": 3750.00,
  "totalCustomers": 95,
  "monthlyBookings": [
    { "month": "Jan", "count": 45 },
    { "month": "Feb", "count": 52 },
    { "month": "Mar", "count": 53 }
  ],
  "topServices": [
    { "name": "Corte Tradicional", "count": 80 },
    { "name": "Barba", "count": 45 }
  ]
}
```

### **3. Listar Agendamentos**

```javascript
// GET /api/dashboard/bookings/{tenantId}?startDate={date}&endDate={date}&status={status}
async function getBookings(tenantId, filters = {}) {
  try {
    const params = new URLSearchParams()
    if (filters.startDate) params.append('startDate', filters.startDate)
    if (filters.endDate) params.append('endDate', filters.endDate)
    if (filters.status) params.append('status', filters.status)
    
    const url = `${BASE_URL}/api/dashboard/bookings/${tenantId}?${params}`
    const response = await fetch(url, {
      method: 'GET',
      headers: authHeaders
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao obter agendamentos:', error)
    throw error
  }
}

// GET /api/dashboard/bookings/today
async function getTodayBookings() {
  try {
    const response = await fetch(`${BASE_URL}/api/dashboard/bookings/today`, {
      method: 'GET',
      headers: authHeaders
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao obter agendamentos de hoje:', error)
    throw error
  }
}

// Exemplo de uso
const allBookings = await getBookings('tenant-123', {
  startDate: '2024-01-01',
  endDate: '2024-01-31',
  status: 'confirmed'
})

const todayBookings = await getTodayBookings()
```

### **4. Gerenciamento de Serviços**

```javascript
// POST /api/service
async function createService(serviceData) {
  try {
    const response = await fetch(`${BASE_URL}/api/service`, {
      method: 'POST',
      headers: authHeaders,
      body: JSON.stringify(serviceData)
    })
    
    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(`HTTP ${response.status}: ${JSON.stringify(errorData)}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao criar serviço:', error)
    throw error
  }
}

// GET /api/service/tenant/{tenantId}
async function getAllServices(tenantId) {
  try {
    const response = await fetch(`${BASE_URL}/api/service/tenant/${tenantId}`, {
      method: 'GET',
      headers: authHeaders
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao obter serviços:', error)
    throw error
  }
}

// Exemplo de uso
const newService = await createService({
  tenantId: "tenant-123",
  name: "Corte + Barba",
  description: "Pacote completo",
  price: 45.00,
  durationMinutes: 60,
  color: "#3B82F6"
})

const allServices = await getAllServices('tenant-123')
```

### **5. Criar Nova Barbearia**

```javascript
// POST /api/tenant/create
async function createTenant(tenantData) {
  try {
    const response = await fetch(`${BASE_URL}/api/tenant/create`, {
      method: 'POST',
      headers: defaultHeaders,
      body: JSON.stringify(tenantData)
    })
    
    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(`HTTP ${response.status}: ${JSON.stringify(errorData)}`)
    }
    
    return await response.json()
  } catch (error) {
    console.error('Erro ao criar tenant:', error)
    throw error
  }
}

// Exemplo de uso
const tenantData = {
  name: "Barbearia Exemplo",
  subdomain: "exemplobarber",
  phone: "+5511999999999",
  email: "contato@exemplo.com",
  website: "www.exemplo.com",
  address: "Rua Exemplo, 123 - Centro",
  description: "A melhor barbearia da cidade",
  primaryColor: "#FF0000",
  secondaryColor: "#00FF00",
  adminEmail: "admin@exemplo.com",
  adminName: "Admin Exemplo",
  adminPassword: "MinhaSenh@123"
}

const newTenant = await createTenant(tenantData)
```

---

## 🔄 **INTERCEPTORS E MIDDLEWARE**

### **Axios Setup (Recomendado)**

```javascript
import axios from 'axios'

// Configuração do Axios
const api = axios.create({
  baseURL: BASE_URL,
  headers: defaultHeaders
})

// Interceptor para adicionar token automaticamente
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwt_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// Interceptor para tratar erros de autenticação
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expirado ou inválido
      logout()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// Exemplo de uso com Axios
const getTenantInfo = (subdomain) => 
  api.get(`/api/tenant/by-subdomain/${subdomain}`)

const getDashboardStats = () => 
  api.get('/api/dashboard/stats')

const createBooking = (subdomain, data) => 
  api.post(`/api/booking/public/${subdomain}`, data)
```

---

## ⚠️ **TRATAMENTO DE ERROS**

### **Códigos de Status HTTP**

```javascript
const handleApiError = (error) => {
  if (error.response) {
    switch (error.response.status) {
      case 400:
        return 'Dados inválidos enviados'
      case 401:
        return 'Não autorizado - faça login novamente'
      case 403:
        return 'Acesso negado'
      case 404:
        return 'Recurso não encontrado'
      case 409:
        return 'Conflito - dados já existem'
      case 422:
        return 'Dados não podem ser processados'
      case 500:
        return 'Erro interno do servidor'
      default:
        return 'Erro desconhecido'
    }
  } else if (error.request) {
    return 'Erro de conexão - verifique sua internet'
  } else {
    return 'Erro inesperado'
  }
}

// Uso em componentes
try {
  const booking = await createBooking(subdomain, bookingData)
  showSuccess('Agendamento criado com sucesso!')
} catch (error) {
  const errorMessage = handleApiError(error)
  showError(errorMessage)
}
```

### **Validação de Formulários**

```javascript
// Validações do lado cliente
const validateBookingForm = (data) => {
  const errors = {}
  
  if (!data.customerName || data.customerName.length < 3) {
    errors.customerName = 'Nome deve ter pelo menos 3 caracteres'
  }
  
  if (!data.customerEmail || !/\S+@\S+\.\S+/.test(data.customerEmail)) {
    errors.customerEmail = 'Email inválido'
  }
  
  if (!data.customerPhone || !/^\+?[\d\s\-\(\)]+$/.test(data.customerPhone)) {
    errors.customerPhone = 'Telefone inválido'
  }
  
  if (!data.serviceId) {
    errors.serviceId = 'Selecione um serviço'
  }
  
  if (!data.date) {
    errors.date = 'Selecione uma data'
  }
  
  if (!data.time) {
    errors.time = 'Selecione um horário'
  }
  
  return Object.keys(errors).length === 0 ? null : errors
}
```

---

## 🎨 **COMPONENTES REACT EXEMPLO**

### **Hook para Agendamento**

```javascript
import { useState, useEffect } from 'react'

export const useBooking = (subdomain) => {
  const [tenant, setTenant] = useState(null)
  const [services, setServices] = useState([])
  const [availableTimes, setAvailableTimes] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  const loadTenant = async () => {
    try {
      setLoading(true)
      const data = await getTenantInfo(subdomain)
      setTenant(data)
    } catch (err) {
      setError(handleApiError(err))
    } finally {
      setLoading(false)
    }
  }

  const loadServices = async () => {
    try {
      setLoading(true)
      const data = await getPublicServices(subdomain)
      setServices(data)
    } catch (err) {
      setError(handleApiError(err))
    } finally {
      setLoading(false)
    }
  }

  const loadAvailableTimes = async (serviceId, date) => {
    try {
      setLoading(true)
      const data = await getAvailableTimes(subdomain, serviceId, date)
      setAvailableTimes(data)
    } catch (err) {
      setError(handleApiError(err))
    } finally {
      setLoading(false)
    }
  }

  const createBooking = async (bookingData) => {
    try {
      setLoading(true)
      const data = await createPublicBooking(subdomain, bookingData)
      return data
    } catch (err) {
      setError(handleApiError(err))
      throw err
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (subdomain) {
      loadTenant()
      loadServices()
    }
  }, [subdomain])

  return {
    tenant,
    services,
    availableTimes,
    loading,
    error,
    loadAvailableTimes,
    createBooking
  }
}
```

### **Componente de Agendamento**

```javascript
import React, { useState } from 'react'
import { useBooking } from './useBooking'

export const BookingForm = ({ subdomain }) => {
  const { tenant, services, availableTimes, loading, error, loadAvailableTimes, createBooking } = useBooking(subdomain)
  const [formData, setFormData] = useState({
    serviceId: '',
    customerName: '',
    customerEmail: '',
    customerPhone: '',
    date: '',
    time: '',
    notes: ''
  })

  const handleServiceChange = (serviceId) => {
    setFormData({ ...formData, serviceId })
    if (formData.date) {
      loadAvailableTimes(serviceId, formData.date)
    }
  }

  const handleDateChange = (date) => {
    setFormData({ ...formData, date, time: '' })
    if (formData.serviceId) {
      loadAvailableTimes(formData.serviceId, date)
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const errors = validateBookingForm(formData)
    if (errors) {
      setError('Preencha todos os campos obrigatórios')
      return
    }

    try {
      await createBooking(formData)
      alert('Agendamento criado com sucesso!')
      // Reset form ou redirect
    } catch (err) {
      // Error já tratado no hook
    }
  }

  if (loading) return <div>Carregando...</div>
  if (error) return <div>Erro: {error}</div>
  if (!tenant) return <div>Barbearia não encontrada</div>

  return (
    <div>
      <h1>{tenant.name}</h1>
      <form onSubmit={handleSubmit}>
        <select 
          value={formData.serviceId} 
          onChange={(e) => handleServiceChange(e.target.value)}
        >
          <option value="">Selecione um serviço</option>
          {services.map(service => (
            <option key={service.id} value={service.id}>
              {service.name} - R$ {service.price}
            </option>
          ))}
        </select>

        <input
          type="date"
          value={formData.date}
          onChange={(e) => handleDateChange(e.target.value)}
          min={new Date().toISOString().split('T')[0]}
        />

        <select 
          value={formData.time} 
          onChange={(e) => setFormData({...formData, time: e.target.value})}
        >
          <option value="">Selecione um horário</option>
          {availableTimes.map(time => (
            <option key={time} value={time}>{time}</option>
          ))}
        </select>

        <input
          type="text"
          placeholder="Seu nome"
          value={formData.customerName}
          onChange={(e) => setFormData({...formData, customerName: e.target.value})}
        />

        <input
          type="email"
          placeholder="Seu email"
          value={formData.customerEmail}
          onChange={(e) => setFormData({...formData, customerEmail: e.target.value})}
        />

        <input
          type="tel"
          placeholder="Seu telefone"
          value={formData.customerPhone}
          onChange={(e) => setFormData({...formData, customerPhone: e.target.value})}
        />

        <textarea
          placeholder="Observações (opcional)"
          value={formData.notes}
          onChange={(e) => setFormData({...formData, notes: e.target.value})}
        />

        <button type="submit" disabled={loading}>
          {loading ? 'Agendando...' : 'Agendar'}
        </button>
      </form>
    </div>
  )
}
```

---

## 📱 **RESPONSIVIDADE E UX**

### **Estados de Loading**

```javascript
// Loading states para diferentes operações
const LoadingStates = {
  TENANT: 'Carregando informações da barbearia...',
  SERVICES: 'Carregando serviços disponíveis...',
  TIMES: 'Verificando horários disponíveis...',
  BOOKING: 'Criando seu agendamento...',
  LOGIN: 'Fazendo login...'
}

// Componente de loading
const LoadingSpinner = ({ message }) => (
  <div className="loading">
    <div className="spinner"></div>
    <p>{message}</p>
  </div>
)
```

### **Notificações**

```javascript
// Sistema de notificações
const showNotification = (type, message) => {
  // Implementar com sua lib de toast preferida
  // Ex: react-hot-toast, react-toastify, etc.
  
  const notifications = {
    success: (msg) => toast.success(msg),
    error: (msg) => toast.error(msg),
    info: (msg) => toast.info(msg),
    warning: (msg) => toast.warning(msg)
  }
  
  notifications[type]?.(message)
}

// Uso
showNotification('success', 'Agendamento criado com sucesso!')
showNotification('error', 'Erro ao criar agendamento')
```

---

## 🔒 **SEGURANÇA**

### **Validação de Subdomínio**

```javascript
const isValidSubdomain = (subdomain) => {
  // Aceitar apenas letras, números e hífens
  const pattern = /^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]$/
  return pattern.test(subdomain) && subdomain.length >= 3 && subdomain.length <= 50
}

// Sanitização
const sanitizeInput = (input) => {
  return input.trim().toLowerCase().replace(/[^a-zA-Z0-9-]/g, '')
}
```

### **Rate Limiting (Frontend)**

```javascript
// Implementar rate limiting básico no frontend
const rateLimiter = {
  attempts: new Map(),
  
  canMakeRequest(endpoint, maxAttempts = 5, windowMs = 60000) {
    const now = Date.now()
    const key = endpoint
    
    if (!this.attempts.has(key)) {
      this.attempts.set(key, [])
    }
    
    const attempts = this.attempts.get(key)
    
    // Remove tentativas antigas
    const validAttempts = attempts.filter(time => now - time < windowMs)
    this.attempts.set(key, validAttempts)
    
    if (validAttempts.length >= maxAttempts) {
      return false
    }
    
    validAttempts.push(now)
    return true
  }
}

// Uso
if (!rateLimiter.canMakeRequest('create-booking')) {
  showNotification('warning', 'Muitas tentativas. Aguarde um momento.')
  return
}
```

---

## 🚀 **DEPLOY E CONFIGURAÇÃO**

### **Variáveis de Ambiente**

```javascript
// .env.local (Next.js)
NEXT_PUBLIC_API_URL=https://barber-agendai-back.onrender.com
NEXT_PUBLIC_ENVIRONMENT=production

// .env.development
NEXT_PUBLIC_API_URL=https://localhost:7230
NEXT_PUBLIC_ENVIRONMENT=development
```

### **Build Scripts**

```json
// package.json
{
  "scripts": {
    "dev": "next dev",
    "build": "next build",
    "start": "next start",
    "test:api": "node scripts/test-api.js"
  }
}
```

---

## ✅ **CHECKLIST DE INTEGRAÇÃO**

### **Site de Agendamento Público**
- [ ] Buscar barbearia por subdomínio
- [ ] Listar serviços disponíveis
- [ ] Mostrar horários disponíveis
- [ ] Criar agendamento
- [ ] Validação de formulários
- [ ] Tratamento de erros
- [ ] Loading states
- [ ] Responsividade mobile

### **Dashboard Administrativo**
- [ ] Login/logout
- [ ] Proteção de rotas
- [ ] Estatísticas do dashboard
- [ ] Listar agendamentos
- [ ] Filtros de agendamentos
- [ ] Criar/editar serviços
- [ ] Gerenciar perfil
- [ ] Refresh token automático

### **Funcionalidades Extras**
- [ ] Dark mode
- [ ] Internacionalização (i18n)
- [ ] PWA (Progressive Web App)
- [ ] Notificações push
- [ ] Cache de dados
- [ ] Offline support

---

**🎉 PRONTO PARA INTEGRAR!**

Este guia contém tudo que você precisa para integrar o frontend com a API BarbeariaSaaS. Todos os endpoints estão funcionando e testados! 🚀 