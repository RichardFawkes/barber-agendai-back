# 🎨 BarbeariaSaaS - Exemplos Práticos Frontend

## 🔧 **CONFIGURAÇÃO POR FRAMEWORK**

### **Next.js (App Router)**

#### **API Service Setup**
```typescript
// lib/api.ts
const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'https://barber-agendai-back.onrender.com'

export class ApiService {
  private static getHeaders(authenticated = false): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    }
    
    if (authenticated && typeof window !== 'undefined') {
      const token = localStorage.getItem('jwt_token')
      if (token) {
        headers.Authorization = `Bearer ${token}`
      }
    }
    
    return headers
  }

  static async get<T>(endpoint: string, authenticated = false): Promise<T> {
    const response = await fetch(`${API_BASE}${endpoint}`, {
      headers: this.getHeaders(authenticated)
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return response.json()
  }

  static async post<T>(endpoint: string, data: any, authenticated = false): Promise<T> {
    const response = await fetch(`${API_BASE}${endpoint}`, {
      method: 'POST',
      headers: this.getHeaders(authenticated),
      body: JSON.stringify(data)
    })
    
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || `HTTP ${response.status}`)
    }
    
    return response.json()
  }
}
```

#### **Page Exemplo: Site de Agendamento**
```typescript
// app/[subdomain]/page.tsx
'use client'

import { useState, useEffect } from 'react'
import { useParams } from 'next/navigation'

interface Tenant {
  id: string
  name: string
  subdomain: string
  branding: {
    colors: {
      primary: string
      accent: string
    }
  }
}

interface Service {
  id: string
  name: string
  description: string
  price: number
  durationMinutes: number
}

export default function BookingPage() {
  const { subdomain } = useParams()
  const [tenant, setTenant] = useState<Tenant | null>(null)
  const [services, setServices] = useState<Service[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function loadData() {
      try {
        const [tenantData, servicesData] = await Promise.all([
          ApiService.get<Tenant>(`/api/tenant/by-subdomain/${subdomain}`),
          ApiService.get<Service[]>(`/api/service/public/${subdomain}`)
        ])
        
        setTenant(tenantData)
        setServices(servicesData)
      } catch (error) {
        console.error('Erro ao carregar dados:', error)
      } finally {
        setLoading(false)
      }
    }

    if (subdomain) {
      loadData()
    }
  }, [subdomain])

  if (loading) {
    return <div className="flex justify-center p-8">Carregando...</div>
  }

  if (!tenant) {
    return <div className="text-center p-8">Barbearia não encontrada</div>
  }

  return (
    <div 
      className="min-h-screen"
      style={{ backgroundColor: tenant.branding.colors.primary }}
    >
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-4xl font-bold text-white mb-8 text-center">
          {tenant.name}
        </h1>
        
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {services.map(service => (
            <ServiceCard key={service.id} service={service} tenant={tenant} />
          ))}
        </div>
      </div>
    </div>
  )
}

function ServiceCard({ service, tenant }: { service: Service, tenant: Tenant }) {
  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      <h3 className="text-xl font-semibold mb-2">{service.name}</h3>
      <p className="text-gray-600 mb-4">{service.description}</p>
      <div className="flex justify-between items-center mb-4">
        <span className="text-2xl font-bold">R$ {service.price}</span>
        <span className="text-gray-500">{service.durationMinutes} min</span>
      </div>
      <button 
        className="w-full py-2 rounded"
        style={{ backgroundColor: tenant.branding.colors.accent }}
        onClick={() => {/* Abrir modal de agendamento */}}
      >
        Agendar
      </button>
    </div>
  )
}
```

#### **Hook Personalizado para Agendamento**
```typescript
// hooks/useBooking.ts
import { useState, useCallback } from 'react'

interface BookingData {
  serviceId: string
  customerName: string
  customerEmail: string
  customerPhone: string
  date: string
  time: string
  notes?: string
}

export function useBooking(subdomain: string) {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const getAvailableTimes = useCallback(async (serviceId: string, date: string) => {
    try {
      setLoading(true)
      setError(null)
      
      const times = await ApiService.get<string[]>(
        `/api/booking/available-times/${subdomain}?serviceId=${serviceId}&date=${date}`
      )
      
      return times
    } catch (err) {
      setError('Erro ao buscar horários disponíveis')
      throw err
    } finally {
      setLoading(false)
    }
  }, [subdomain])

  const createBooking = useCallback(async (data: BookingData) => {
    try {
      setLoading(true)
      setError(null)
      
      const booking = await ApiService.post(
        `/api/booking/public/${subdomain}`,
        data
      )
      
      return booking
    } catch (err) {
      setError('Erro ao criar agendamento')
      throw err
    } finally {
      setLoading(false)
    }
  }, [subdomain])

  return {
    loading,
    error,
    getAvailableTimes,
    createBooking
  }
}
```

---

### **Vue.js 3 (Composition API)**

#### **Composable para API**
```typescript
// composables/useApi.ts
import { ref, computed } from 'vue'

const API_BASE = import.meta.env.VITE_API_URL || 'https://barber-agendai-back.onrender.com'

export function useApi() {
  const loading = ref(false)
  const error = ref<string | null>(null)

  const headers = computed(() => {
    const baseHeaders: Record<string, string> = {
      'Content-Type': 'application/json'
    }
    
    const token = localStorage.getItem('jwt_token')
    if (token) {
      baseHeaders.Authorization = `Bearer ${token}`
    }
    
    return baseHeaders
  })

  async function get<T>(endpoint: string): Promise<T> {
    try {
      loading.value = true
      error.value = null
      
      const response = await fetch(`${API_BASE}${endpoint}`, {
        headers: headers.value
      })
      
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`)
      }
      
      return await response.json()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro desconhecido'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function post<T>(endpoint: string, data: any): Promise<T> {
    try {
      loading.value = true
      error.value = null
      
      const response = await fetch(`${API_BASE}${endpoint}`, {
        method: 'POST',
        headers: headers.value,
        body: JSON.stringify(data)
      })
      
      if (!response.ok) {
        const errorData = await response.json()
        throw new Error(errorData.message || `HTTP ${response.status}`)
      }
      
      return await response.json()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro desconhecido'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    loading: readonly(loading),
    error: readonly(error),
    get,
    post
  }
}
```

#### **Componente de Agendamento**
```vue
<!-- components/BookingForm.vue -->
<template>
  <div class="booking-form">
    <h2>Agendar Serviço</h2>
    
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label>Serviço:</label>
        <select v-model="form.serviceId" @change="onServiceChange" required>
          <option value="">Selecione um serviço</option>
          <option 
            v-for="service in services" 
            :key="service.id" 
            :value="service.id"
          >
            {{ service.name }} - R$ {{ service.price }}
          </option>
        </select>
      </div>

      <div class="form-group">
        <label>Data:</label>
        <input 
          type="date" 
          v-model="form.date" 
          @change="onDateChange"
          :min="today"
          required 
        />
      </div>

      <div class="form-group" v-if="availableTimes.length">
        <label>Horário:</label>
        <div class="time-grid">
          <button
            v-for="time in availableTimes"
            :key="time"
            type="button"
            :class="['time-slot', { active: form.time === time }]"
            @click="form.time = time"
          >
            {{ time }}
          </button>
        </div>
      </div>

      <div class="form-group">
        <label>Nome:</label>
        <input 
          type="text" 
          v-model="form.customerName" 
          placeholder="Seu nome completo"
          required 
        />
      </div>

      <div class="form-group">
        <label>Email:</label>
        <input 
          type="email" 
          v-model="form.customerEmail" 
          placeholder="seu@email.com"
          required 
        />
      </div>

      <div class="form-group">
        <label>Telefone:</label>
        <input 
          type="tel" 
          v-model="form.customerPhone" 
          placeholder="+55 11 99999-9999"
          required 
        />
      </div>

      <div class="form-group">
        <label>Observações:</label>
        <textarea 
          v-model="form.notes" 
          placeholder="Alguma observação especial?"
        ></textarea>
      </div>

      <button 
        type="submit" 
        :disabled="loading || !canSubmit"
        class="submit-btn"
      >
        {{ loading ? 'Agendando...' : 'Confirmar Agendamento' }}
      </button>
    </form>

    <div v-if="error" class="error">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useApi } from '@/composables/useApi'

interface Props {
  subdomain: string
  services: Service[]
}

const props = defineProps<Props>()
const emit = defineEmits<{
  success: [booking: any]
  error: [message: string]
}>()

const { loading, error, get, post } = useApi()

const form = ref({
  serviceId: '',
  customerName: '',
  customerEmail: '',
  customerPhone: '',
  date: '',
  time: '',
  notes: ''
})

const availableTimes = ref<string[]>([])

const today = computed(() => {
  return new Date().toISOString().split('T')[0]
})

const canSubmit = computed(() => {
  return form.value.serviceId && 
         form.value.date && 
         form.value.time && 
         form.value.customerName && 
         form.value.customerEmail && 
         form.value.customerPhone
})

async function loadAvailableTimes() {
  if (!form.value.serviceId || !form.value.date) return
  
  try {
    const times = await get<string[]>(
      `/api/booking/available-times/${props.subdomain}?serviceId=${form.value.serviceId}&date=${form.value.date}`
    )
    availableTimes.value = times
    form.value.time = '' // Reset selected time
  } catch (err) {
    availableTimes.value = []
  }
}

function onServiceChange() {
  form.value.time = ''
  if (form.value.date) {
    loadAvailableTimes()
  }
}

function onDateChange() {
  form.value.time = ''
  if (form.value.serviceId) {
    loadAvailableTimes()
  }
}

async function handleSubmit() {
  try {
    const booking = await post(`/api/booking/public/${props.subdomain}`, form.value)
    emit('success', booking)
    
    // Reset form
    form.value = {
      serviceId: '',
      customerName: '',
      customerEmail: '',
      customerPhone: '',
      date: '',
      time: '',
      notes: ''
    }
  } catch (err) {
    emit('error', 'Erro ao criar agendamento')
  }
}
</script>

<style scoped>
.booking-form {
  max-width: 500px;
  margin: 0 auto;
  padding: 2rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.time-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
  gap: 0.5rem;
}

.time-slot {
  padding: 0.5rem;
  border: 1px solid #ddd;
  background: white;
  cursor: pointer;
  border-radius: 4px;
}

.time-slot:hover,
.time-slot.active {
  background: #007bff;
  color: white;
}

.submit-btn {
  width: 100%;
  padding: 1rem;
  background: #28a745;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1.1rem;
  cursor: pointer;
}

.submit-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.error {
  color: #dc3545;
  margin-top: 1rem;
  padding: 0.5rem;
  background: #f8d7da;
  border-radius: 4px;
}
</style>
```

---

### **Angular (Standalone Components)**

#### **Service para API**
```typescript
// services/api.service.ts
import { Injectable } from '@angular/core'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Observable } from 'rxjs'
import { environment } from '../environments/environment'

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl

  constructor(private http: HttpClient) {}

  private getHeaders(authenticated = false): HttpHeaders {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    })

    if (authenticated) {
      const token = localStorage.getItem('jwt_token')
      if (token) {
        headers = headers.set('Authorization', `Bearer ${token}`)
      }
    }

    return headers
  }

  getTenant(subdomain: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/tenant/by-subdomain/${subdomain}`)
  }

  getPublicServices(subdomain: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/api/service/public/${subdomain}`)
  }

  getAvailableTimes(subdomain: string, serviceId: string, date: string): Observable<string[]> {
    const params = { serviceId, date }
    return this.http.get<string[]>(`${this.baseUrl}/api/booking/available-times/${subdomain}`, { params })
  }

  createBooking(subdomain: string, bookingData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/booking/public/${subdomain}`, bookingData)
  }

  // Dashboard endpoints
  login(credentials: { email: string, password: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/api/auth/login`, credentials)
  }

  getDashboardStats(): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/dashboard/stats`, {
      headers: this.getHeaders(true)
    })
  }

  getBookings(tenantId: string, filters?: any): Observable<any[]> {
    let params: any = {}
    if (filters) {
      Object.keys(filters).forEach(key => {
        if (filters[key]) params[key] = filters[key]
      })
    }

    return this.http.get<any[]>(`${this.baseUrl}/api/dashboard/bookings/${tenantId}`, {
      headers: this.getHeaders(true),
      params
    })
  }
}
```

#### **Componente de Agendamento**
```typescript
// components/booking-form.component.ts
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core'
import { CommonModule } from '@angular/common'
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms'
import { ApiService } from '../services/api.service'

@Component({
  selector: 'app-booking-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="booking-form">
      <h2>Agendar Serviço</h2>
      
      <form [formGroup]="bookingForm" (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label>Serviço:</label>
          <select formControlName="serviceId" (change)="onServiceChange()">
            <option value="">Selecione um serviço</option>
            <option *ngFor="let service of services" [value]="service.id">
              {{ service.name }} - R$ {{ service.price }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label>Data:</label>
          <input 
            type="date" 
            formControlName="date"
            [min]="today"
            (change)="onDateChange()"
          />
        </div>

        <div class="form-group" *ngIf="availableTimes.length">
          <label>Horário:</label>
          <div class="time-grid">
            <button
              *ngFor="let time of availableTimes"
              type="button"
              [class.active]="bookingForm.get('time')?.value === time"
              (click)="selectTime(time)"
            >
              {{ time }}
            </button>
          </div>
        </div>

        <div class="form-group">
          <label>Nome:</label>
          <input type="text" formControlName="customerName" placeholder="Seu nome completo" />
        </div>

        <div class="form-group">
          <label>Email:</label>
          <input type="email" formControlName="customerEmail" placeholder="seu@email.com" />
        </div>

        <div class="form-group">
          <label>Telefone:</label>
          <input type="tel" formControlName="customerPhone" placeholder="+55 11 99999-9999" />
        </div>

        <div class="form-group">
          <label>Observações:</label>
          <textarea formControlName="notes" placeholder="Alguma observação especial?"></textarea>
        </div>

        <button 
          type="submit" 
          [disabled]="bookingForm.invalid || loading"
          class="submit-btn"
        >
          {{ loading ? 'Agendando...' : 'Confirmar Agendamento' }}
        </button>
      </form>

      <div *ngIf="error" class="error">{{ error }}</div>
    </div>
  `,
  styleUrls: ['./booking-form.component.css']
})
export class BookingFormComponent implements OnInit {
  @Input() subdomain!: string
  @Input() services: any[] = []
  @Output() success = new EventEmitter<any>()
  @Output() errorEvent = new EventEmitter<string>()

  bookingForm: FormGroup
  availableTimes: string[] = []
  loading = false
  error: string | null = null
  today: string

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService
  ) {
    this.today = new Date().toISOString().split('T')[0]
    
    this.bookingForm = this.fb.group({
      serviceId: ['', Validators.required],
      customerName: ['', [Validators.required, Validators.minLength(3)]],
      customerEmail: ['', [Validators.required, Validators.email]],
      customerPhone: ['', Validators.required],
      date: ['', Validators.required],
      time: ['', Validators.required],
      notes: ['']
    })
  }

  ngOnInit() {}

  onServiceChange() {
    this.bookingForm.patchValue({ time: '' })
    this.loadAvailableTimes()
  }

  onDateChange() {
    this.bookingForm.patchValue({ time: '' })
    this.loadAvailableTimes()
  }

  selectTime(time: string) {
    this.bookingForm.patchValue({ time })
  }

  loadAvailableTimes() {
    const serviceId = this.bookingForm.get('serviceId')?.value
    const date = this.bookingForm.get('date')?.value

    if (!serviceId || !date) return

    this.apiService.getAvailableTimes(this.subdomain, serviceId, date)
      .subscribe({
        next: (times) => {
          this.availableTimes = times
        },
        error: (err) => {
          this.availableTimes = []
          console.error('Erro ao carregar horários:', err)
        }
      })
  }

  onSubmit() {
    if (this.bookingForm.invalid) return

    this.loading = true
    this.error = null

    this.apiService.createBooking(this.subdomain, this.bookingForm.value)
      .subscribe({
        next: (booking) => {
          this.success.emit(booking)
          this.bookingForm.reset()
          this.availableTimes = []
          this.loading = false
        },
        error: (err) => {
          this.error = 'Erro ao criar agendamento'
          this.errorEvent.emit(this.error)
          this.loading = false
        }
      })
  }
}
```

---

## 📱 **EXEMPLOS MOBILE (React Native)**

### **API Service**
```typescript
// services/api.ts
import AsyncStorage from '@react-native-async-storage/async-storage'

const API_BASE = __DEV__ 
  ? 'https://localhost:7230' 
  : 'https://barber-agendai-back.onrender.com'

class ApiService {
  static async getHeaders(authenticated = false): Promise<Record<string, string>> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    }
    
    if (authenticated) {
      const token = await AsyncStorage.getItem('jwt_token')
      if (token) {
        headers.Authorization = `Bearer ${token}`
      }
    }
    
    return headers
  }

  static async get<T>(endpoint: string, authenticated = false): Promise<T> {
    const headers = await this.getHeaders(authenticated)
    
    const response = await fetch(`${API_BASE}${endpoint}`, {
      headers
    })
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    
    return response.json()
  }

  static async post<T>(endpoint: string, data: any, authenticated = false): Promise<T> {
    const headers = await this.getHeaders(authenticated)
    
    const response = await fetch(`${API_BASE}${endpoint}`, {
      method: 'POST',
      headers,
      body: JSON.stringify(data)
    })
    
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || `HTTP ${response.status}`)
    }
    
    return response.json()
  }
}

export default ApiService
```

### **Hook para Agendamento Mobile**
```typescript
// hooks/useBooking.ts
import { useState, useCallback } from 'react'
import ApiService from '../services/api'

export function useBooking(subdomain: string) {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const getTenant = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      return await ApiService.get(`/api/tenant/by-subdomain/${subdomain}`)
    } catch (err) {
      setError('Erro ao buscar barbearia')
      throw err
    } finally {
      setLoading(false)
    }
  }, [subdomain])

  const getServices = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      return await ApiService.get(`/api/service/public/${subdomain}`)
    } catch (err) {
      setError('Erro ao buscar serviços')
      throw err
    } finally {
      setLoading(false)
    }
  }, [subdomain])

  const getAvailableTimes = useCallback(async (serviceId: string, date: string) => {
    try {
      setLoading(true)
      setError(null)
      return await ApiService.get(`/api/booking/available-times/${subdomain}?serviceId=${serviceId}&date=${date}`)
    } catch (err) {
      setError('Erro ao buscar horários')
      throw err
    } finally {
      setLoading(false)
    }
  }, [subdomain])

  const createBooking = useCallback(async (bookingData: any) => {
    try {
      setLoading(true)
      setError(null)
      return await ApiService.post(`/api/booking/public/${subdomain}`, bookingData)
    } catch (err) {
      setError('Erro ao criar agendamento')
      throw err
    } finally {
      setLoading(false)
    }
  }, [subdomain])

  return {
    loading,
    error,
    getTenant,
    getServices,
    getAvailableTimes,
    createBooking
  }
}
```

---

## 🔄 **PATTERNS AVANÇADOS**

### **Context para Estado Global (React)**
```typescript
// context/AppContext.tsx
import React, { createContext, useContext, useReducer, ReactNode } from 'react'

interface AppState {
  user: User | null
  tenant: Tenant | null
  loading: boolean
  error: string | null
}

type AppAction = 
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'SET_USER'; payload: User | null }
  | { type: 'SET_TENANT'; payload: Tenant | null }

const initialState: AppState = {
  user: null,
  tenant: null,
  loading: false,
  error: null
}

function appReducer(state: AppState, action: AppAction): AppState {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, loading: action.payload }
    case 'SET_ERROR':
      return { ...state, error: action.payload }
    case 'SET_USER':
      return { ...state, user: action.payload }
    case 'SET_TENANT':
      return { ...state, tenant: action.payload }
    default:
      return state
  }
}

const AppContext = createContext<{
  state: AppState
  dispatch: React.Dispatch<AppAction>
} | null>(null)

export function AppProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(appReducer, initialState)

  return (
    <AppContext.Provider value={{ state, dispatch }}>
      {children}
    </AppContext.Provider>
  )
}

export function useAppContext() {
  const context = useContext(AppContext)
  if (!context) {
    throw new Error('useAppContext must be used within AppProvider')
  }
  return context
}
```

### **Cache com React Query**
```typescript
// hooks/useBookingQueries.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import ApiService from '../services/api'

export function useTenant(subdomain: string) {
  return useQuery({
    queryKey: ['tenant', subdomain],
    queryFn: () => ApiService.get(`/api/tenant/by-subdomain/${subdomain}`),
    staleTime: 5 * 60 * 1000, // 5 minutos
    enabled: !!subdomain
  })
}

export function useServices(subdomain: string) {
  return useQuery({
    queryKey: ['services', subdomain],
    queryFn: () => ApiService.get(`/api/service/public/${subdomain}`),
    staleTime: 10 * 60 * 1000, // 10 minutos
    enabled: !!subdomain
  })
}

export function useAvailableTimes(subdomain: string, serviceId: string, date: string) {
  return useQuery({
    queryKey: ['available-times', subdomain, serviceId, date],
    queryFn: () => ApiService.get(`/api/booking/available-times/${subdomain}?serviceId=${serviceId}&date=${date}`),
    enabled: !!(subdomain && serviceId && date),
    staleTime: 1 * 60 * 1000, // 1 minuto
  })
}

export function useCreateBooking(subdomain: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: any) => ApiService.post(`/api/booking/public/${subdomain}`, data),
    onSuccess: () => {
      // Invalidar cache de horários disponíveis
      queryClient.invalidateQueries({ queryKey: ['available-times', subdomain] })
    }
  })
}
```

---

## 🎯 **OTIMIZAÇÕES DE PERFORMANCE**

### **Lazy Loading de Componentes**
```typescript
// Lazy loading no React
const BookingForm = lazy(() => import('./components/BookingForm'))
const DashboardStats = lazy(() => import('./components/DashboardStats'))

function App() {
  return (
    <Suspense fallback={<div>Carregando...</div>}>
      <Routes>
        <Route path="/:subdomain" element={<BookingForm />} />
        <Route path="/dashboard" element={<DashboardStats />} />
      </Routes>
    </Suspense>
  )
}

// Lazy loading no Vue
const BookingForm = defineAsyncComponent(() => import('./components/BookingForm.vue'))
const DashboardStats = defineAsyncComponent(() => import('./components/DashboardStats.vue'))
```

### **Debounce para Busca**
```typescript
// Hook de debounce
function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value)

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value)
    }, delay)

    return () => {
      clearTimeout(handler)
    }
  }, [value, delay])

  return debouncedValue
}

// Uso em busca de horários
function BookingForm() {
  const [selectedDate, setSelectedDate] = useState('')
  const debouncedDate = useDebounce(selectedDate, 300)

  useEffect(() => {
    if (debouncedDate && selectedService) {
      loadAvailableTimes(selectedService, debouncedDate)
    }
  }, [debouncedDate, selectedService])
}
```

---

## 📊 **MONITORAMENTO E ANALYTICS**

### **Tracking de Eventos**
```typescript
// Analytics service
class AnalyticsService {
  static trackEvent(event: string, properties?: Record<string, any>) {
    // Google Analytics
    if (typeof gtag !== 'undefined') {
      gtag('event', event, properties)
    }

    // Mixpanel
    if (typeof mixpanel !== 'undefined') {
      mixpanel.track(event, properties)
    }

    console.log('Analytics Event:', event, properties)
  }

  static trackBookingAttempt(subdomain: string, serviceId: string) {
    this.trackEvent('booking_attempt', {
      subdomain,
      service_id: serviceId,
      timestamp: new Date().toISOString()
    })
  }

  static trackBookingSuccess(subdomain: string, bookingId: string) {
    this.trackEvent('booking_success', {
      subdomain,
      booking_id: bookingId,
      timestamp: new Date().toISOString()
    })
  }
}

// Uso nos componentes
function BookingForm() {
  const handleSubmit = async (data) => {
    AnalyticsService.trackBookingAttempt(subdomain, data.serviceId)
    
    try {
      const booking = await createBooking(data)
      AnalyticsService.trackBookingSuccess(subdomain, booking.id)
    } catch (error) {
      AnalyticsService.trackEvent('booking_error', {
        subdomain,
        error: error.message
      })
    }
  }
}
```

---

## ✅ **CHECKLIST FINAL DE INTEGRAÇÃO**

### **Funcionalidades Básicas**
- [ ] Buscar barbearia por subdomínio
- [ ] Listar serviços disponíveis
- [ ] Mostrar horários disponíveis
- [ ] Criar agendamento público
- [ ] Sistema de autenticação
- [ ] Dashboard com estatísticas
- [ ] Listar/filtrar agendamentos
- [ ] CRUD de serviços

### **UX/UI**
- [ ] Loading states
- [ ] Tratamento de erros
- [ ] Validação de formulários
- [ ] Responsividade mobile
- [ ] Feedback visual (toasts/alerts)
- [ ] Estados vazios (empty states)

### **Performance**
- [ ] Cache de dados
- [ ] Lazy loading
- [ ] Debounce em buscas
- [ ] Otimização de imagens
- [ ] Minificação de código

### **Segurança**
- [ ] Validação de subdomínio
- [ ] Sanitização de inputs
- [ ] Rate limiting
- [ ] Headers de segurança
- [ ] Validação de JWT

### **Monitoramento**
- [ ] Analytics de eventos
- [ ] Logs de erro
- [ ] Métricas de performance
- [ ] Monitoramento de uptime

---

**🎉 PRONTO PARA QUALQUER FRAMEWORK!**

Com estes exemplos você pode integrar a API BarbeariaSaaS em qualquer tecnologia frontend! 🚀 