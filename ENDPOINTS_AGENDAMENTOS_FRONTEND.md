# 📅 ENDPOINTS DE AGENDAMENTOS - GUIA FRONTEND

## 🎯 OVERVIEW
Este documento explica como integrar os endpoints de agendamentos para funcionalidades administrativas (dashboard/admin) no frontend.

---

## 🔐 AUTENTICAÇÃO OBRIGATÓRIA
Todos os endpoints administrativos exigem autenticação JWT com header:
```http
Authorization: Bearer {seu_jwt_token}
```

O token deve conter a claim `tenant_id` para identificar o tenant.

---

## 📋 ENDPOINTS PRINCIPAIS

### 1. **BUSCAR AGENDAMENTOS (ADMIN)** ⭐
```http
GET /api/dashboard/bookings/{tenantId}
```

**Uso:** Lista completa de agendamentos com filtros avançados

**Parâmetros:**
- `tenantId` (path, obrigatório) - ID do tenant (GUID)
- `startDate` (query, opcional) - Data inicial (YYYY-MM-DD)
- `endDate` (query, opcional) - Data final (YYYY-MM-DD)
- `status` (query, opcional) - Status: "confirmed", "cancelled", "completed"

**Exemplos:**
```javascript
// Todos os agendamentos do tenant
GET /api/dashboard/bookings/123e4567-e89b-12d3-a456-426614174000

// Agendamentos de janeiro 2024
GET /api/dashboard/bookings/123e4567-e89b-12d3-a456-426614174000?startDate=2024-01-01&endDate=2024-01-31

// Apenas agendamentos confirmados
GET /api/dashboard/bookings/123e4567-e89b-12d3-a456-426614174000?status=confirmed

// Agendamentos da semana atual confirmados
GET /api/dashboard/bookings/123e4567-e89b-12d3-a456-426614174000?startDate=2024-01-15&endDate=2024-01-21&status=confirmed
```

### 2. **AGENDAMENTOS DE HOJE**
```http
GET /api/dashboard/bookings/today
```

**Uso:** Agendamentos do dia atual (usa tenant_id do JWT)

**Sem parâmetros** - usa automaticamente o tenant do usuário logado

---

## 📊 ESTRUTURA DE RESPOSTA

### BookingDto
```typescript
interface BookingDto {
  id: string;              // GUID do agendamento
  tenantId: string;        // GUID do tenant
  serviceId: string;       // GUID do serviço
  customerId: string;      // GUID do cliente
  bookingDate: string;     // Data no formato "YYYY-MM-DD"
  bookingTime: string;     // Horário no formato "HH:mm:ss"
  status: BookingStatus;   // Status do agendamento
  totalPrice: number;      // Preço total
  notes?: string;          // Observações opcionais
  createdAt: string;       // ISO timestamp
  updatedAt: string;       // ISO timestamp
  
  // Dados relacionados (populados)
  service: {
    id: string;
    name: string;
    duration: number;      // Duração em minutos
    price: number;
  };
  
  customer: {
    id: string;
    name: string;
    email: string;
    phone?: string;
  };
}

enum BookingStatus {
  Pending = "pending",
  Confirmed = "confirmed", 
  InProgress = "in_progress",
  Completed = "completed",
  Cancelled = "cancelled",
  NoShow = "no_show"
}
```

### Exemplo de Resposta
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "tenantId": "123e4567-e89b-12d3-a456-426614174000",
    "serviceId": "789e0123-e89b-12d3-a456-426614174000",
    "customerId": "456e7890-e89b-12d3-a456-426614174000",
    "bookingDate": "2024-01-20",
    "bookingTime": "14:30:00",
    "status": "confirmed",
    "totalPrice": 50.00,
    "notes": "Cliente preferiu horário da tarde",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "service": {
      "id": "789e0123-e89b-12d3-a456-426614174000",
      "name": "Corte + Barba",
      "duration": 60,
      "price": 50.00
    },
    "customer": {
      "id": "456e7890-e89b-12d3-a456-426614174000",
      "name": "João Silva",
      "email": "joao@email.com",
      "phone": "(11) 99999-9999"
    }
  }
]
```

---

## 🚀 EXEMPLOS DE INTEGRAÇÃO FRONTEND

### React Hook Personalizado
```typescript
import { useState, useEffect } from 'react';
import { apiClient } from './api';

interface UseBookingsParams {
  tenantId: string;
  startDate?: string;
  endDate?: string;
  status?: string;
}

export const useBookings = ({ tenantId, startDate, endDate, status }: UseBookingsParams) => {
  const [bookings, setBookings] = useState<BookingDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchBookings = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const params = new URLSearchParams();
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        if (status) params.append('status', status);
        
        const url = `/api/dashboard/bookings/${tenantId}?${params.toString()}`;
        const response = await apiClient.get(url);
        
        setBookings(response.data);
      } catch (err) {
        setError('Erro ao carregar agendamentos');
        console.error('Booking fetch error:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchBookings();
  }, [tenantId, startDate, endDate, status]);

  return { bookings, loading, error, refetch: () => fetchBookings() };
};
```

### Hook para Agendamentos de Hoje
```typescript
export const useTodayBookings = () => {
  const [bookings, setBookings] = useState<BookingDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTodayBookings = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const response = await apiClient.get('/api/dashboard/bookings/today');
        setBookings(response.data);
      } catch (err) {
        setError('Erro ao carregar agendamentos de hoje');
        console.error('Today bookings error:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTodayBookings();
  }, []);

  return { bookings, loading, error, refetch: () => fetchTodayBookings() };
};
```

### Componente de Lista de Agendamentos
```tsx
import React from 'react';
import { useBookings } from './hooks/useBookings';
import { formatDate, formatTime } from './utils/dateUtils';

interface BookingsListProps {
  tenantId: string;
  startDate?: string;
  endDate?: string;
  status?: string;
}

const BookingsList: React.FC<BookingsListProps> = ({ 
  tenantId, 
  startDate, 
  endDate, 
  status 
}) => {
  const { bookings, loading, error } = useBookings({
    tenantId,
    startDate,
    endDate,
    status
  });

  if (loading) return <div>Carregando agendamentos...</div>;
  if (error) return <div className="error">Erro: {error}</div>;

  return (
    <div className="bookings-list">
      <h2>Agendamentos ({bookings.length})</h2>
      
      {bookings.length === 0 ? (
        <p>Nenhum agendamento encontrado.</p>
      ) : (
        <div className="bookings-grid">
          {bookings.map((booking) => (
            <div key={booking.id} className={`booking-card booking-${booking.status}`}>
              <div className="booking-header">
                <h3>{booking.customer.name}</h3>
                <span className={`status status-${booking.status}`}>
                  {getStatusLabel(booking.status)}
                </span>
              </div>
              
              <div className="booking-details">
                <p><strong>Serviço:</strong> {booking.service.name}</p>
                <p><strong>Data:</strong> {formatDate(booking.bookingDate)}</p>
                <p><strong>Horário:</strong> {formatTime(booking.bookingTime)}</p>
                <p><strong>Duração:</strong> {booking.service.duration} min</p>
                <p><strong>Valor:</strong> R$ {booking.totalPrice.toFixed(2)}</p>
              </div>
              
              <div className="booking-contact">
                <p>📧 {booking.customer.email}</p>
                {booking.customer.phone && (
                  <p>📱 {booking.customer.phone}</p>
                )}
              </div>
              
              {booking.notes && (
                <div className="booking-notes">
                  <p><strong>Observações:</strong> {booking.notes}</p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

const getStatusLabel = (status: string): string => {
  const labels = {
    pending: 'Pendente',
    confirmed: 'Confirmado',
    in_progress: 'Em Andamento',
    completed: 'Concluído',
    cancelled: 'Cancelado',
    no_show: 'Não Compareceu'
  };
  return labels[status] || status;
};
```

### Dashboard de Hoje
```tsx
import React from 'react';
import { useTodayBookings } from './hooks/useBookings';

const TodayDashboard: React.FC = () => {
  const { bookings, loading, error } = useTodayBookings();

  const confirmedBookings = bookings.filter(b => b.status === 'confirmed');
  const completedBookings = bookings.filter(b => b.status === 'completed');
  const totalRevenue = completedBookings.reduce((sum, b) => sum + b.totalPrice, 0);

  if (loading) return <div>Carregando...</div>;
  if (error) return <div>Erro: {error}</div>;

  return (
    <div className="today-dashboard">
      <h1>Dashboard - Hoje</h1>
      
      <div className="stats-grid">
        <div className="stat-card">
          <h3>Total de Agendamentos</h3>
          <p className="stat-number">{bookings.length}</p>
        </div>
        
        <div className="stat-card">
          <h3>Confirmados</h3>
          <p className="stat-number">{confirmedBookings.length}</p>
        </div>
        
        <div className="stat-card">
          <h3>Concluídos</h3>
          <p className="stat-number">{completedBookings.length}</p>
        </div>
        
        <div className="stat-card">
          <h3>Faturamento</h3>
          <p className="stat-number">R$ {totalRevenue.toFixed(2)}</p>
        </div>
      </div>

      <BookingsList 
        tenantId={getCurrentTenantId()} 
        startDate={getTodayString()}
        endDate={getTodayString()}
      />
    </div>
  );
};
```

---

## 🎨 ESTILOS CSS SUGERIDOS

```css
.bookings-list {
  padding: 20px;
}

.bookings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.booking-card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  background: white;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.booking-confirmed { border-left: 4px solid #10b981; }
.booking-pending { border-left: 4px solid #f59e0b; }
.booking-completed { border-left: 4px solid #3b82f6; }
.booking-cancelled { border-left: 4px solid #ef4444; }

.booking-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.status {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: bold;
}

.status-confirmed { background: #dcfce7; color: #166534; }
.status-pending { background: #fef3c7; color: #92400e; }
.status-completed { background: #dbeafe; color: #1e40af; }
.status-cancelled { background: #fee2e2; color: #dc2626; }

.today-dashboard {
  padding: 20px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 30px;
}

.stat-card {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  text-align: center;
}

.stat-number {
  font-size: 2em;
  font-weight: bold;
  color: #3b82f6;
  margin: 8px 0;
}
```

---

## 🛠️ CONFIGURAÇÃO DA API

```typescript
// api.ts
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: 'http://localhost:5080', // URL da sua API
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar o token automaticamente
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken'); // ou onde você armazena o token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para tratar erros
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expirado - redirecionar para login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

---

## 📱 FILTROS AVANÇADOS FRONTEND

```tsx
import React, { useState } from 'react';

interface BookingFiltersProps {
  onFiltersChange: (filters: BookingFilters) => void;
}

interface BookingFilters {
  startDate?: string;
  endDate?: string;
  status?: string;
}

const BookingFilters: React.FC<BookingFiltersProps> = ({ onFiltersChange }) => {
  const [filters, setFilters] = useState<BookingFilters>({});

  const handleFilterChange = (key: keyof BookingFilters, value: string) => {
    const newFilters = { ...filters, [key]: value || undefined };
    setFilters(newFilters);
    onFiltersChange(newFilters);
  };

  const getWeekRange = () => {
    const today = new Date();
    const startOfWeek = new Date(today.setDate(today.getDate() - today.getDay()));
    const endOfWeek = new Date(today.setDate(today.getDate() - today.getDay() + 6));
    
    return {
      start: startOfWeek.toISOString().split('T')[0],
      end: endOfWeek.toISOString().split('T')[0]
    };
  };

  const setQuickFilter = (type: 'today' | 'week' | 'month') => {
    const today = new Date();
    let startDate: string, endDate: string;

    switch (type) {
      case 'today':
        startDate = endDate = today.toISOString().split('T')[0];
        break;
      case 'week':
        const week = getWeekRange();
        startDate = week.start;
        endDate = week.end;
        break;
      case 'month':
        startDate = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0];
        endDate = new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString().split('T')[0];
        break;
    }

    const newFilters = { ...filters, startDate, endDate };
    setFilters(newFilters);
    onFiltersChange(newFilters);
  };

  return (
    <div className="booking-filters">
      <div className="filter-row">
        <div className="filter-group">
          <label>Data Inicial:</label>
          <input
            type="date"
            value={filters.startDate || ''}
            onChange={(e) => handleFilterChange('startDate', e.target.value)}
          />
        </div>

        <div className="filter-group">
          <label>Data Final:</label>
          <input
            type="date"
            value={filters.endDate || ''}
            onChange={(e) => handleFilterChange('endDate', e.target.value)}
          />
        </div>

        <div className="filter-group">
          <label>Status:</label>
          <select
            value={filters.status || ''}
            onChange={(e) => handleFilterChange('status', e.target.value)}
          >
            <option value="">Todos</option>
            <option value="confirmed">Confirmado</option>
            <option value="pending">Pendente</option>
            <option value="completed">Concluído</option>
            <option value="cancelled">Cancelado</option>
            <option value="no_show">Não Compareceu</option>
          </select>
        </div>
      </div>

      <div className="quick-filters">
        <button onClick={() => setQuickFilter('today')}>Hoje</button>
        <button onClick={() => setQuickFilter('week')}>Esta Semana</button>
        <button onClick={() => setQuickFilter('month')}>Este Mês</button>
        <button onClick={() => onFiltersChange({})}>Limpar</button>
      </div>
    </div>
  );
};
```

---

## ⚠️ TRATAMENTO DE ERROS

```typescript
// Tipos de erro comuns
interface ApiError {
  message: string;
  status: number;
  details?: any;
}

// Função para tratar erros
const handleApiError = (error: any): string => {
  if (error.response) {
    switch (error.response.status) {
      case 400:
        return 'Dados inválidos fornecidos';
      case 401:
        return 'Acesso não autorizado. Faça login novamente.';
      case 403:
        return 'Você não tem permissão para acessar estes dados';
      case 404:
        return 'Recurso não encontrado';
      case 500:
        return 'Erro interno do servidor. Tente novamente.';
      default:
        return error.response.data?.message || 'Erro desconhecido';
    }
  } else if (error.request) {
    return 'Erro de conexão. Verifique sua internet.';
  } else {
    return 'Erro inesperado. Tente novamente.';
  }
};
```

---

## 🎯 PRÓXIMOS PASSOS

1. **Implementar filtros de busca avançada**
2. **Adicionar paginação para grandes volumes**
3. **Implementar real-time updates com SignalR/WebSockets**
4. **Adicionar export para CSV/PDF**
5. **Implementar notificações push**

---

**🔗 URLs dos Endpoints:**
- Agendamentos Admin: `GET /api/dashboard/bookings/{tenantId}`
- Agendamentos Hoje: `GET /api/dashboard/bookings/today`
- Base URL: `http://localhost:5080` (desenvolvimento)

**📧 Suporte:** Para dúvidas sobre integração, consulte a documentação da API ou entre em contato com a equipe de desenvolvimento. 