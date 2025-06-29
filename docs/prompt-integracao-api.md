# 🚀 **PROMPT PARA INTEGRAÇÃO DA API BARBEARIA SAAS**

## **📊 STATUS ATUAL**
✅ **API 100% FUNCIONAL** rodando em: `http://localhost:5080`  
✅ **Swagger UI** disponível em: `http://localhost:5080`  
✅ **Banco de dados criado** automaticamente com dados de teste  
✅ **JWT Authentication** configurado  
✅ **CORS habilitado** para `http://localhost:3000`  

---

## **🔑 DADOS DE TESTE CRIADOS**

### **🏢 Tenant (Barbearia):**
```json
{
  "name": "Barbearia do João",
  "subdomain": "joao",
  "id": "guid-gerado-automaticamente"
}
```

### **👨‍💼 Admin User:**
```json
{
  "email": "admin@barbearianojoao.com",
  "password": "admin123",
  "role": "TenantAdmin"
}
```

### **✂️ Serviços Criados:**
```json
[
  { "name": "Corte Masculino", "price": 35.00, "duration": 30 },
  { "name": "Barba", "price": 25.00, "duration": 20 },
  { "name": "Corte + Barba", "price": 50.00, "duration": 45 }
]
```

---

## **🌐 ENDPOINTS PARA FRONTEND**

### **📱 SITE PÚBLICO (Página de Agendamento):**

#### **1. Obter dados da barbearia:**
```typescript
GET /api/tenant/by-subdomain/{subdomain}

// Exemplo:
fetch('http://localhost:5080/api/tenant/by-subdomain/joao')
  .then(res => res.json())
  .then(data => {
    // data contém: name, branding, settings, etc.
  });
```

#### **2. Listar serviços disponíveis:**
```typescript
GET /api/service/public/{subdomain}

// Exemplo:
fetch('http://localhost:5080/api/service/public/joao')
  .then(res => res.json())
  .then(services => {
    // Array de serviços com: id, name, price, duration, description
  });
```

#### **3. Criar agendamento:**
```typescript
POST /api/booking/public/{subdomain}

const bookingData = {
  serviceId: "guid-do-servico",
  customerName: "João Silva",
  customerEmail: "joao@email.com", 
  customerPhone: "(11) 99999-9999",
  date: "2024-01-15",
  time: "14:00",
  notes: "Primeira vez na barbearia"
};

fetch('http://localhost:5080/api/booking/public/joao', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(bookingData)
})
.then(res => res.json())
.then(booking => {
  // Retorna: id, customerName, serviceName, date, time, status, servicePrice
});
```

---

### **🔐 DASHBOARD ADMIN (Requer JWT):**

#### **1. Login e obter token:**
```typescript
POST /api/auth/login

const loginData = {
  email: "admin@barbearianojoao.com",
  password: "admin123"
};

fetch('http://localhost:5080/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(loginData)
})
.then(res => res.json())
.then(data => {
  // data.token contém o JWT
  localStorage.setItem('token', data.token);
});
```

#### **2. Obter estatísticas do dashboard:**
```typescript
GET /api/dashboard/stats

const token = localStorage.getItem('token');

fetch('http://localhost:5080/api/dashboard/stats', {
  headers: { 
    'Authorization': `Bearer ${token}` 
  }
})
.then(res => res.json())
.then(stats => {
  // stats contém: totalBookings, todayRevenue, totalClients, etc.
});
```

#### **3. Listar serviços (admin):**
```typescript
GET /api/service/tenant/{tenantId}/active

fetch(`http://localhost:5080/api/service/tenant/${tenantId}/active`, {
  headers: { 
    'Authorization': `Bearer ${token}` 
  }
})
.then(res => res.json())
.then(services => {
  // Lista completa de serviços para gerenciamento
});
```

---

## **📋 ESTRUTURAS DE DADOS (DTOs)**

### **TenantDto:**
```typescript
interface TenantDto {
  id: string;
  name: string;
  subdomain: string;
  email?: string;
  phone?: string;
  address?: string;
  description?: string;
  branding: {
    colors: { primary: string; secondary?: string; accent?: string; };
    logo?: { url: string; };
    theme?: string;
  };
  settings: {
    businessHours: BusinessHour[];
    booking: { allowOnlineBooking: boolean; };
    notifications: { emailEnabled: boolean; smsEnabled: boolean; };
  };
  status: 'Active' | 'Inactive' | 'Suspended';
  plan: 'Free' | 'Basic' | 'Premium' | 'Enterprise';
}
```

### **ServiceDto:**
```typescript
interface ServiceDto {
  id: string;
  name: string;
  description?: string;
  price: number;
  duration: number; // em minutos
  color?: string;    // hex color
  imageUrl?: string;
  isActive: boolean;
  category?: {
    id: number;
    name: string;
    color?: string;
  };
}
```

### **BookingDto:**
```typescript
interface BookingDto {
  id: string;
  customerName: string;
  customerEmail?: string;
  customerPhone?: string;
  serviceName: string;
  servicePrice: number;
  date: string;        // YYYY-MM-DD
  time: string;        // HH:mm
  status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';
  notes?: string;
  createdAt: string;
}
```

### **CreateBookingDto:**
```typescript
interface CreateBookingDto {
  serviceId: string;
  customerName: string;
  customerEmail?: string;
  customerPhone?: string;
  date: string;        // YYYY-MM-DD
  time: string;        // HH:mm
  notes?: string;
}
```

### **DashboardStatsDto:**
```typescript
interface DashboardStatsDto {
  totalBookings: number;
  todayRevenue: number;
  totalClients: number;
  averageRating: number;
  pendingBookings: number;
  confirmedBookings: number;
}
```

### **LoginResponseDto:**
```typescript
interface LoginResponseDto {
  token: string;
  user: {
    id: string;
    name: string;
    email: string;
    role: 'SuperAdmin' | 'TenantAdmin' | 'TenantEmployee' | 'Customer';
    tenantId?: string;
  };
  expiresAt: string;
}
```

---

## **🔧 CONFIGURAÇÃO NO NEXT.JS**

### **1. Criar serviço de API:**
```typescript
// lib/api.ts
const API_BASE_URL = 'http://localhost:5080';

class ApiService {
  private getHeaders(includeAuth = false) {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };
    
    if (includeAuth) {
      const token = localStorage.getItem('token');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }
    
    return headers;
  }

  // SITE PÚBLICO
  async getTenantBySubdomain(subdomain: string): Promise<TenantDto> {
    const response = await fetch(`${API_BASE_URL}/api/tenant/by-subdomain/${subdomain}`);
    return response.json();
  }

  async getPublicServices(subdomain: string): Promise<ServiceDto[]> {
    const response = await fetch(`${API_BASE_URL}/api/service/public/${subdomain}`);
    return response.json();
  }

  async createPublicBooking(subdomain: string, booking: CreateBookingDto): Promise<BookingDto> {
    const response = await fetch(`${API_BASE_URL}/api/booking/public/${subdomain}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(booking)
    });
    return response.json();
  }

  // DASHBOARD ADMIN
  async login(email: string, password: string): Promise<LoginResponseDto> {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify({ email, password })
    });
    return response.json();
  }

  async getDashboardStats(): Promise<DashboardStatsDto> {
    const response = await fetch(`${API_BASE_URL}/api/dashboard/stats`, {
      headers: this.getHeaders(true)
    });
    return response.json();
  }

  async getTenantServices(tenantId: string): Promise<ServiceDto[]> {
    const response = await fetch(`${API_BASE_URL}/api/service/tenant/${tenantId}/active`, {
      headers: this.getHeaders(true)
    });
    return response.json();
  }
}

export const apiService = new ApiService();
```

### **2. Hook para autenticação:**
```typescript
// hooks/useAuth.ts
import { useState, useEffect } from 'react';
import { apiService } from '@/lib/api';

export const useAuth = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');
    
    if (token && userData) {
      setUser(JSON.parse(userData));
    }
    setLoading(false);
  }, []);

  const login = async (email: string, password: string) => {
    try {
      const response = await apiService.login(email, password);
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
      setUser(response.user);
      return response;
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  return { user, login, logout, loading };
};
```

### **3. Exemplo de componente de agendamento:**
```typescript
// components/BookingForm.tsx
'use client';

import { useState, useEffect } from 'react';
import { apiService } from '@/lib/api';

interface BookingFormProps {
  subdomain: string;
}

export default function BookingForm({ subdomain }: BookingFormProps) {
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [tenant, setTenant] = useState<TenantDto | null>(null);
  const [formData, setFormData] = useState<CreateBookingDto>({
    serviceId: '',
    customerName: '',
    customerEmail: '',
    customerPhone: '',
    date: '',
    time: '',
    notes: ''
  });

  useEffect(() => {
    loadData();
  }, [subdomain]);

  const loadData = async () => {
    try {
      const [tenantData, servicesData] = await Promise.all([
        apiService.getTenantBySubdomain(subdomain),
        apiService.getPublicServices(subdomain)
      ]);
      
      setTenant(tenantData);
      setServices(servicesData);
    } catch (error) {
      console.error('Erro ao carregar dados:', error);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const booking = await apiService.createPublicBooking(subdomain, formData);
      alert(`Agendamento criado com sucesso! ID: ${booking.id}`);
      
      // Reset form
      setFormData({
        serviceId: '',
        customerName: '',
        customerEmail: '',
        customerPhone: '',
        date: '',
        time: '',
        notes: ''
      });
    } catch (error) {
      alert('Erro ao criar agendamento. Tente novamente.');
    }
  };

  if (!tenant) return <div>Carregando...</div>;

  return (
    <div className="max-w-md mx-auto bg-white p-6 rounded-lg shadow-lg">
      <h2 className="text-2xl font-bold mb-4" style={{ color: tenant.branding.colors.primary }}>
        {tenant.name}
      </h2>
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">Serviço</label>
          <select
            value={formData.serviceId}
            onChange={(e) => setFormData({...formData, serviceId: e.target.value})}
            className="w-full p-2 border rounded-md"
            required
          >
            <option value="">Selecione um serviço</option>
            {services.map(service => (
              <option key={service.id} value={service.id}>
                {service.name} - R$ {service.price.toFixed(2)} ({service.duration}min)
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Nome</label>
          <input
            type="text"
            value={formData.customerName}
            onChange={(e) => setFormData({...formData, customerName: e.target.value})}
            className="w-full p-2 border rounded-md"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Email</label>
          <input
            type="email"
            value={formData.customerEmail}
            onChange={(e) => setFormData({...formData, customerEmail: e.target.value})}
            className="w-full p-2 border rounded-md"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Telefone</label>
          <input
            type="tel"
            value={formData.customerPhone}
            onChange={(e) => setFormData({...formData, customerPhone: e.target.value})}
            className="w-full p-2 border rounded-md"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">Data</label>
            <input
              type="date"
              value={formData.date}
              onChange={(e) => setFormData({...formData, date: e.target.value})}
              className="w-full p-2 border rounded-md"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Horário</label>
            <input
              type="time"
              value={formData.time}
              onChange={(e) => setFormData({...formData, time: e.target.value})}
              className="w-full p-2 border rounded-md"
              required
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Observações</label>
          <textarea
            value={formData.notes}
            onChange={(e) => setFormData({...formData, notes: e.target.value})}
            className="w-full p-2 border rounded-md"
            rows={3}
          />
        </div>

        <button
          type="submit"
          className="w-full py-2 px-4 rounded-md text-white font-medium"
          style={{ backgroundColor: tenant.branding.colors.primary }}
        >
          Agendar Serviço
        </button>
      </form>
    </div>
  );
}
```

### **4. Exemplo de dashboard admin:**
```typescript
// components/AdminDashboard.tsx
'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { apiService } from '@/lib/api';

export default function AdminDashboard() {
  const { user } = useAuth();
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);
  const [services, setServices] = useState<ServiceDto[]>([]);

  useEffect(() => {
    if (user) {
      loadDashboardData();
    }
  }, [user]);

  const loadDashboardData = async () => {
    try {
      const [statsData, servicesData] = await Promise.all([
        apiService.getDashboardStats(),
        user?.tenantId ? apiService.getTenantServices(user.tenantId) : Promise.resolve([])
      ]);
      
      setStats(statsData);
      setServices(servicesData);
    } catch (error) {
      console.error('Erro ao carregar dashboard:', error);
    }
  };

  if (!stats) return <div>Carregando dashboard...</div>;

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold mb-6">Dashboard</h1>
      
      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-lg font-semibold text-gray-600">Total de Agendamentos</h3>
          <p className="text-3xl font-bold text-blue-600">{stats.totalBookings}</p>
        </div>
        
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-lg font-semibold text-gray-600">Receita Hoje</h3>
          <p className="text-3xl font-bold text-green-600">R$ {stats.todayRevenue.toFixed(2)}</p>
        </div>
        
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-lg font-semibold text-gray-600">Total de Clientes</h3>
          <p className="text-3xl font-bold text-purple-600">{stats.totalClients}</p>
        </div>
      </div>

      {/* Services */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h2 className="text-xl font-bold mb-4">Serviços Ativos</h2>
        <div className="grid gap-4">
          {services.map(service => (
            <div key={service.id} className="flex justify-between items-center p-4 border rounded">
              <div>
                <h3 className="font-semibold">{service.name}</h3>
                <p className="text-gray-600">{service.description}</p>
              </div>
              <div className="text-right">
                <p className="font-bold">R$ {service.price.toFixed(2)}</p>
                <p className="text-sm text-gray-500">{service.duration} min</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
```

---

## **🧪 TESTES RÁPIDOS**

### **Testar endpoints no navegador:**

1. **Health Check:** `http://localhost:5080/health`
2. **Swagger UI:** `http://localhost:5080`
3. **Tenant Info:** `http://localhost:5080/api/tenant/by-subdomain/joao`
4. **Serviços Públicos:** `http://localhost:5080/api/service/public/joao`

### **Testar login via curl:**
```bash
curl -X POST "http://localhost:5080/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@barbearianojoao.com","password":"admin123"}'
```

---

## **📝 PRÓXIMOS PASSOS**

1. **Implementar a classe ApiService** no seu projeto Next.js
2. **Criar componentes de agendamento** usando os endpoints públicos
3. **Implementar dashboard admin** com autenticação JWT
4. **Configurar rotas** para multitenancy (ex: `/barbearia/[subdomain]`)
5. **Adicionar tratamento de erros** e loading states
6. **Customizar UI** usando o branding do tenant

---

## **🔗 ENDPOINTS COMPLETOS DISPONÍVEIS**

### **✅ Públicos (Sem Auth):**
- `GET /health` - Status da API
- `GET /api/tenant/by-subdomain/{subdomain}` - Info da barbearia
- `GET /api/service/public/{subdomain}` - Serviços públicos
- `POST /api/booking/public/{subdomain}` - Agendamento público
- `POST /api/auth/login` - Login JWT

### **🔐 Protegidos (Requer JWT):**
- `GET /api/dashboard/stats` - Estatísticas
- `GET /api/service/tenant/{id}/active` - Serviços do admin
- `POST /api/booking` - Agendamento admin

---

## **🎉 API PRONTA PARA INTEGRAÇÃO!**

A API está **100% funcional** e pronta para receber requisições do seu frontend Next.js. Use este prompt para integrar rapidamente todos os endpoints necessários! 