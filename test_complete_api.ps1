# Teste completo dos endpoints da API BarbeariaSaaS

$baseUrl = "https://localhost:7230"
$subdomain = "barbeariadoze"

Write-Host "🧪 TESTANDO API COMPLETA DE AGENDAMENTO" -ForegroundColor Yellow
Write-Host "Base URL: $baseUrl" -ForegroundColor Cyan
Write-Host "Subdomain: $subdomain" -ForegroundColor Cyan

# 1. TESTE - Obter informações do tenant
Write-Host "`n1️⃣ OBTENDO INFORMAÇÕES DO TENANT..." -ForegroundColor Green
try {
    $tenant = Invoke-RestMethod -Uri "$baseUrl/api/tenant/by-subdomain/$subdomain" -Method GET -SkipCertificateCheck
    Write-Host "✅ Tenant encontrado: $($tenant.name)" -ForegroundColor Green
    $tenantId = $tenant.id
} catch {
    Write-Host "❌ Erro ao obter tenant: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# 2. TESTE - Listar serviços públicos
Write-Host "`n2️⃣ LISTANDO SERVIÇOS PÚBLICOS..." -ForegroundColor Green
try {
    $services = Invoke-RestMethod -Uri "$baseUrl/api/service/public/$subdomain" -Method GET -SkipCertificateCheck
    Write-Host "✅ Encontrados $($services.Count) serviços:" -ForegroundColor Green
    foreach ($service in $services) {
        Write-Host "  - $($service.name): R$ $($service.price) ($($service.durationMinutes) min)" -ForegroundColor Gray
    }
    $firstServiceId = $services[0].id
} catch {
    Write-Host "❌ Erro ao listar serviços: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. TESTE - Obter horários disponíveis
Write-Host "`n3️⃣ OBTENDO HORÁRIOS DISPONÍVEIS..." -ForegroundColor Green
$today = Get-Date -Format "yyyy-MM-dd"
try {
    $availableTimes = Invoke-RestMethod -Uri "$baseUrl/api/booking/available-times/$subdomain" -Method GET -Body @{
        serviceId = $firstServiceId
        date = $today
    } -SkipCertificateCheck
    
    Write-Host "✅ Horários disponíveis para hoje ($today):" -ForegroundColor Green
    foreach ($time in $availableTimes) {
        Write-Host "  - $time" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Erro ao obter horários: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

# 4. TESTE - Criar agendamento público
Write-Host "`n4️⃣ CRIANDO AGENDAMENTO PÚBLICO..." -ForegroundColor Green
$bookingData = @{
    tenantId = $tenantId
    serviceId = $firstServiceId
    customerName = "Cliente Teste"
    customerEmail = "teste@email.com"
    customerPhone = "+5511999999999"
    date = $today
    time = "10:00"
    notes = "Agendamento teste via API"
} | ConvertTo-Json

try {
    $booking = Invoke-RestMethod -Uri "$baseUrl/api/booking/public/$subdomain" -Method POST -Body $bookingData -ContentType "application/json" -SkipCertificateCheck
    Write-Host "✅ Agendamento criado: $($booking.id)" -ForegroundColor Green
    Write-Host "  Cliente: $($booking.customerName)" -ForegroundColor Gray
    Write-Host "  Data/Hora: $($booking.date) às $($booking.time)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Erro ao criar agendamento: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

Write-Host "`n🎯 PARA TESTAR ENDPOINTS DE DASHBOARD:" -ForegroundColor Cyan
Write-Host "1. Faça login: POST /api/auth/login" -ForegroundColor White
Write-Host "2. Use o token JWT nos headers: Authorization: Bearer <token>" -ForegroundColor White
Write-Host "3. Teste os endpoints:" -ForegroundColor White
Write-Host "   - GET /api/dashboard/stats" -ForegroundColor Gray
Write-Host "   - GET /api/dashboard/bookings/today" -ForegroundColor Gray
Write-Host "   - POST /api/service (criar novo serviço)" -ForegroundColor Gray
Write-Host "   - GET /api/service/tenant/$tenantId" -ForegroundColor Gray

Write-Host "`n✅ TESTE COMPLETO!" -ForegroundColor Green 