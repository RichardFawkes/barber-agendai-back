# Script para testar os endpoints de disponibilidade do sistema de agendamento
Write-Host "🧪 Testando Endpoints de Disponibilidade - Sistema de Agendamento" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

$baseUrl = "http://localhost:5080"
$subdomain = "exemplo"
$serviceId = "550e8400-e29b-41d4-a716-446655440000"  # ID de exemplo
$currentDate = Get-Date -Format "yyyy-MM-dd"
$currentYear = Get-Date -Format "yyyy"
$currentMonth = Get-Date -Format "MM"

Write-Host ""
Write-Host "📅 Testando endpoint: Available Dates (Mês)" -ForegroundColor Green
Write-Host "URL: GET $baseUrl/api/booking/available-dates/$subdomain" -ForegroundColor Gray
Write-Host "Parâmetros: serviceId=$serviceId, year=$currentYear, month=$currentMonth" -ForegroundColor Gray

try {
    $url1 = "$baseUrl/api/booking/available-dates/$subdomain" + "?serviceId=$serviceId" + "&year=$currentYear" + "&month=$currentMonth"
    $response1 = Invoke-RestMethod -Uri $url1 -Method GET -ContentType "application/json"
    Write-Host "✅ Success! Response:" -ForegroundColor Green
    $response1 | ConvertTo-Json -Depth 5 | Write-Host
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "⏰ Testando endpoint: Available Times (Dia)" -ForegroundColor Green
Write-Host "URL: GET $baseUrl/api/booking/available-times/$subdomain" -ForegroundColor Gray
Write-Host "Parâmetros: serviceId=$serviceId, date=$currentDate" -ForegroundColor Gray

try {
    $url2 = "$baseUrl/api/booking/available-times/$subdomain" + "?serviceId=$serviceId" + "&date=$currentDate"
    $response2 = Invoke-RestMethod -Uri $url2 -Method GET -ContentType "application/json"
    Write-Host "✅ Success! Response:" -ForegroundColor Green
    $response2 | ConvertTo-Json -Depth 5 | Write-Host
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🏢 Testando endpoint: Business Hours (PUT)" -ForegroundColor Green
Write-Host "URL: PUT $baseUrl/api/tenant/$subdomain/business-hours" -ForegroundColor Gray

$businessHoursBody = @{
    schedule = @(
        @{
            dayOfWeek = 1
            dayName = "monday"
            isOpen = $true
            startTime = "08:00"
            endTime = "18:00"
        },
        @{
            dayOfWeek = 2
            dayName = "tuesday"
            isOpen = $true
            startTime = "08:00"
            endTime = "18:00"
        },
        @{
            dayOfWeek = 0
            dayName = "sunday"
            isOpen = $false
            startTime = $null
            endTime = $null
        }
    )
    breaks = @(
        @{
            startTime = "12:00"
            endTime = "13:00"
            name = "Almoço"
            appliesToAllDays = $true
        }
    )
    settings = @{
        slotDurationMinutes = 30
        advanceBookingDays = 30
        maxBookingsPerDay = 50
        bookingBufferMinutes = 0
        timezone = "America/Sao_Paulo"
        autoConfirmBookings = $true
    }
} | ConvertTo-Json -Depth 4

try {
    $url3 = "$baseUrl/api/tenant/$subdomain/business-hours"
    $response3 = Invoke-RestMethod -Uri $url3 -Method PUT -Body $businessHoursBody -ContentType "application/json"
    Write-Host "✅ Success! Response:" -ForegroundColor Green
    $response3 | ConvertTo-Json -Depth 3 | Write-Host
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "📊 Testando endpoint: Schedule Overview" -ForegroundColor Green
Write-Host "URL: GET $baseUrl/api/dashboard/$subdomain/schedule-overview" -ForegroundColor Gray
Write-Host "Parâmetros: date=$currentDate" -ForegroundColor Gray

try {
    $url4 = "$baseUrl/api/dashboard/$subdomain/schedule-overview" + "?date=$currentDate"
    $response4 = Invoke-RestMethod -Uri $url4 -Method GET -ContentType "application/json"
    Write-Host "✅ Success! Response:" -ForegroundColor Green
    $response4 | ConvertTo-Json -Depth 5 | Write-Host
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "ℹ️  Note: Este endpoint pode não estar implementado ainda" -ForegroundColor Yellow
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎉 Testando endpoint: Health Check" -ForegroundColor Green
Write-Host "URL: GET $baseUrl/api/booking/health" -ForegroundColor Gray

try {
    $url5 = "$baseUrl/api/booking/health"
    $response5 = Invoke-RestMethod -Uri $url5 -Method GET -ContentType "application/json"
    Write-Host "✅ Success! Response:" -ForegroundColor Green
    $response5 | ConvertTo-Json -Depth 2 | Write-Host
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "✨ Teste dos endpoints de disponibilidade concluído!" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Resumo dos endpoints testados:" -ForegroundColor White
Write-Host "   • GET  /api/booking/available-dates/{subdomain}" -ForegroundColor Gray
Write-Host "   • GET  /api/booking/available-times/{subdomain}" -ForegroundColor Gray
Write-Host "   • PUT  /api/tenant/{subdomain}/business-hours" -ForegroundColor Gray
Write-Host "   • GET  /api/dashboard/{subdomain}/schedule-overview" -ForegroundColor Gray
Write-Host "   • GET  /api/booking/health" -ForegroundColor Gray
Write-Host "" 