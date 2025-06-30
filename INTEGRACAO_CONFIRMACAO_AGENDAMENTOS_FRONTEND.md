# 🔄 CONFIRMAÇÃO DE AGENDAMENTOS - INTEGRAÇÃO FRONTEND

## 🎯 OVERVIEW
Endpoints para confirmar, cancelar e gerenciar status de agendamentos no sistema de barbearia.

---

## 🔐 AUTENTICAÇÃO
Todos os endpoints requerem autenticação JWT:
```http
Authorization: Bearer {jwt_token}
```

---

## 📋 ENDPOINTS DE CONFIRMAÇÃO

### 1. **CONFIRMAR AGENDAMENTO** ✅
```http
POST /api/booking/{bookingId}/confirm
```

**Descrição:** Confirma um agendamento pendente de forma rápida

**Parâmetros:**
- `bookingId` (path) - ID do agendamento (GUID)

**Exemplo:**
```javascript
const confirmBooking = async (bookingId) => {
  try {
    const response = await fetch(`/api/booking/${bookingId}/confirm`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (!response.ok) {
      throw new Error('Erro ao confirmar agendamento');
    }
    
    const booking = await response.json();
    return booking;
  } catch (error) {
    console.error('Erro:', error);
    throw error;
  }
};
```

### 2. **CANCELAR AGENDAMENTO** ❌
```http
POST /api/booking/{bookingId}/cancel?reason={motivo}
```

**Descrição:** Cancela um agendamento com motivo opcional

**Parâmetros:**
- `bookingId` (path) - ID do agendamento (GUID)
- `reason` (query, opcional) - Motivo do cancelamento

**Exemplo:**
```javascript
const cancelBooking = async (bookingId, reason = null) => {
  try {
    const url = reason 
      ? `/api/booking/${bookingId}/cancel?reason=${encodeURIComponent(reason)}`
      : `/api/booking/${bookingId}/cancel`;
      
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (!response.ok) {
      throw new Error('Erro ao cancelar agendamento');
    }
    
    const booking = await response.json();
    return booking;
  } catch (error) {
    console.error('Erro:', error);
    throw error;
  }
};
```

### 3. **ATUALIZAR STATUS GERAL** 🔄
```http
PUT /api/booking/{bookingId}/status
```

**Descrição:** Atualiza status do agendamento com controle total

**Body:**
```typescript
{
  status: string,     // obrigatório: "pending", "confirmed", "in_progress", "completed", "cancelled", "no_show"
  notes?: string,     // opcional: observações sobre a mudança
  reason?: string     // opcional: motivo da mudança
}
```

**Exemplo:**
```javascript
const updateBookingStatus = async (bookingId, statusData) => {
  try {
    const response = await fetch(`/api/booking/${bookingId}/status`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(statusData)
    });
    
    if (!response.ok) {
      throw new Error('Erro ao atualizar status');
    }
    
    const booking = await response.json();
    return booking;
  } catch (error) {
    console.error('Erro:', error);
    throw error;
  }
};

// Exemplos de uso:
await updateBookingStatus('123e4567-e89b-12d3-a456-426614174000', {
  status: 'in_progress',
  notes: 'Cliente chegou no horário'
});

await updateBookingStatus('123e4567-e89b-12d3-a456-426614174000', {
  status: 'no_show',
  reason: 'Cliente não compareceu',
  notes: 'Aguardamos 15 minutos'
});
```

---

## 📊 FLUXO DE STATUS

### Status Disponíveis:
- `pending` - Pendente (inicial)
- `confirmed` - Confirmado
- `in_progress` - Em andamento
- `completed` - Concluído
- `cancelled` - Cancelado
- `no_show` - Não compareceu

### Transições Válidas:
```
pending → confirmed, cancelled
confirmed → in_progress, cancelled, no_show
in_progress → completed, cancelled
completed → [FINAL - não pode mudar]
cancelled → [FINAL - não pode mudar]
no_show → [FINAL - não pode mudar]
```

---

## 🎨 COMPONENTE REACT COMPLETO

### Hook para Confirmação:
```typescript
// hooks/useBookingConfirmation.ts
import { useState } from 'react';

interface BookingConfirmationHook {
  confirmBooking: (bookingId: string) => Promise<void>;
  cancelBooking: (bookingId: string, reason?: string) => Promise<void>;
  updateStatus: (bookingId: string, status: string, notes?: string) => Promise<void>;
  isLoading: boolean;
  error: string | null;
}

export const useBookingConfirmation = (): BookingConfirmationHook => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getAuthHeaders = () => {
    const token = localStorage.getItem('authToken');
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  };

  const confirmBooking = async (bookingId: string) => {
    setIsLoading(true);
    setError(null);
    
    try {
      const response = await fetch(`/api/booking/${bookingId}/confirm`, {
        method: 'POST',
        headers: getAuthHeaders()
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Erro ao confirmar agendamento');
      }
      
      // Sucesso - você pode disparar um refresh da lista aqui
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const cancelBooking = async (bookingId: string, reason?: string) => {
    setIsLoading(true);
    setError(null);
    
    try {
      const url = reason 
        ? `/api/booking/${bookingId}/cancel?reason=${encodeURIComponent(reason)}`
        : `/api/booking/${bookingId}/cancel`;
        
      const response = await fetch(url, {
        method: 'POST',
        headers: getAuthHeaders()
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Erro ao cancelar agendamento');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const updateStatus = async (bookingId: string, status: string, notes?: string) => {
    setIsLoading(true);
    setError(null);
    
    try {
      const response = await fetch(`/api/booking/${bookingId}/status`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify({ status, notes })
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Erro ao atualizar status');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    confirmBooking,
    cancelBooking,
    updateStatus,
    isLoading,
    error
  };
};
```

### Componente de Ações:
```typescript
// components/BookingActions.tsx
import React, { useState } from 'react';
import { useBookingConfirmation } from '../hooks/useBookingConfirmation';

interface BookingActionsProps {
  booking: {
    id: string;
    status: string;
    customerName: string;
  };
  onStatusChange?: () => void;
}

export const BookingActions: React.FC<BookingActionsProps> = ({ 
  booking, 
  onStatusChange 
}) => {
  const { confirmBooking, cancelBooking, updateStatus, isLoading, error } = useBookingConfirmation();
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  const handleConfirm = async () => {
    try {
      await confirmBooking(booking.id);
      onStatusChange?.();
    } catch (error) {
      // Erro já tratado no hook
    }
  };

  const handleCancel = async () => {
    try {
      await cancelBooking(booking.id, cancelReason || undefined);
      setShowCancelModal(false);
      setCancelReason('');
      onStatusChange?.();
    } catch (error) {
      // Erro já tratado no hook
    }
  };

  const handleStatusChange = async (newStatus: string) => {
    try {
      await updateStatus(booking.id, newStatus);
      onStatusChange?.();
    } catch (error) {
      // Erro já tratado no hook
    }
  };

  const canConfirm = booking.status === 'pending';
  const canCancel = ['pending', 'confirmed'].includes(booking.status);
  const canProgress = booking.status === 'confirmed';
  const canComplete = booking.status === 'in_progress';

  return (
    <div className="booking-actions">
      {error && (
        <div className="error-message">
          {error}
        </div>
      )}

      <div className="action-buttons">
        {canConfirm && (
          <button 
            onClick={handleConfirm}
            disabled={isLoading}
            className="btn btn-success"
          >
            ✅ Confirmar
          </button>
        )}

        {canProgress && (
          <button 
            onClick={() => handleStatusChange('in_progress')}
            disabled={isLoading}
            className="btn btn-primary"
          >
            ▶️ Iniciar
          </button>
        )}

        {canComplete && (
          <button 
            onClick={() => handleStatusChange('completed')}
            disabled={isLoading}
            className="btn btn-success"
          >
            ✅ Concluir
          </button>
        )}

        {canCancel && (
          <button 
            onClick={() => setShowCancelModal(true)}
            disabled={isLoading}
            className="btn btn-danger"
          >
            ❌ Cancelar
          </button>
        )}

        {booking.status === 'confirmed' && (
          <button 
            onClick={() => handleStatusChange('no_show')}
            disabled={isLoading}
            className="btn btn-warning"
          >
            👻 Não Compareceu
          </button>
        )}
      </div>

      {showCancelModal && (
        <div className="cancel-modal">
          <div className="modal-content">
            <h3>Cancelar Agendamento</h3>
            <p>Tem certeza que deseja cancelar o agendamento de <strong>{booking.customerName}</strong>?</p>
            
            <textarea
              placeholder="Motivo do cancelamento (opcional)"
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              className="cancel-reason"
            />

            <div className="modal-actions">
              <button 
                onClick={handleCancel}
                disabled={isLoading}
                className="btn btn-danger"
              >
                Sim, Cancelar
              </button>
              <button 
                onClick={() => setShowCancelModal(false)}
                className="btn btn-secondary"
              >
                Voltar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
```

### CSS para Ações:
```css
/* styles/BookingActions.css */
.booking-actions {
  padding: 1rem;
  border-top: 1px solid #eee;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 500;
  transition: all 0.2s;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-success {
  background-color: #28a745;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background-color: #218838;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #0056b3;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background-color: #c82333;
}

.btn-warning {
  background-color: #ffc107;
  color: #212529;
}

.btn-warning:hover:not(:disabled) {
  background-color: #e0a800;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #545b62;
}

.cancel-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.cancel-reason {
  width: 100%;
  min-height: 100px;
  padding: 0.5rem;
  margin: 1rem 0;
  border: 1px solid #ddd;
  border-radius: 4px;
  resize: vertical;
}

.modal-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
  margin-top: 1rem;
}

.error-message {
  color: #dc3545;
  background-color: #f8d7da;
  padding: 0.5rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  border: 1px solid #f5c6cb;
}
```

---

## 🎯 EXEMPLO DE USO COMPLETO

```typescript
// pages/BookingsPage.tsx
import React, { useState, useEffect } from 'react';
import { BookingActions } from '../components/BookingActions';

interface Booking {
  id: string;
  customerName: string;
  status: string;
  // ... outras propriedades
}

export const BookingsPage: React.FC = () => {
  const [bookings, setBookings] = useState<Booking[]>([]);

  const refreshBookings = async () => {
    // Buscar agendamentos atualizados
    const response = await fetch('/api/dashboard/bookings/today', {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('authToken')}`
      }
    });
    const data = await response.json();
    setBookings(data);
  };

  useEffect(() => {
    refreshBookings();
  }, []);

  return (
    <div className="bookings-page">
      <h1>Agendamentos de Hoje</h1>
      
      {bookings.map((booking) => (
        <div key={booking.id} className="booking-card">
          <div className="booking-info">
            <h3>{booking.customerName}</h3>
            <span className={`status status-${booking.status}`}>
              {getStatusLabel(booking.status)}
            </span>
          </div>
          
          <BookingActions 
            booking={booking}
            onStatusChange={refreshBookings}
          />
        </div>
      ))}
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

---

## 🔥 FUNCIONALIDADES IMPLEMENTADAS

✅ **Confirmação rápida** - Um clique para confirmar  
✅ **Cancelamento com motivo** - Modal para inserir razão  
✅ **Progressão de status** - Fluxo completo do agendamento  
✅ **Validação de transições** - Apenas mudanças válidas  
✅ **Feedback visual** - Estados de loading e erro  
✅ **Responsivo** - Funciona em mobile e desktop  

---

## 📱 BASE URL
```
http://localhost:5080/api/booking
```

Substitua pela URL do seu ambiente de produção quando necessário. 