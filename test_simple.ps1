Write-Host "🧪 Testando Health Check" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5080/api/booking/health" -Method GET
    Write-Host "✅ Success! Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
} 