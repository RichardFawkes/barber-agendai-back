$headers = @{
    "Content-Type" = "application/json"
}

$body = @{
    tenant = @{
        name = "Barbearia Teste 5080"
        description = "Barbearia para teste do endpoint na porta 5080"
        subdomain = "teste5080"
        phone = "(11) 99999-9999"
        email = "teste@barbearia5080.com"
        address = "Rua de Teste 5080, 123 - Centro, São Paulo - SP"
        website = "www.barbearia5080.com.br"
    }
    admin = @{
        name = "Admin Teste 5080"
        email = "admin@barbearia5080.com"
        phone = "(11) 98888-8888"
        password = "teste5080"
    }
} | ConvertTo-Json -Depth 3

Write-Host "🚀 Testando endpoint: POST http://localhost:5080/api/tenant/create" -ForegroundColor Cyan
Write-Host "📦 Enviando dados..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5080/api/tenant/create" -Method POST -Headers $headers -Body $body
    Write-Host "✅ SUCESSO!" -ForegroundColor Green
    Write-Host "📋 Resposta:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "❌ ERRO:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "📊 Status Code: $statusCode" -ForegroundColor Yellow
        
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "📄 Response Body:" -ForegroundColor Yellow
            Write-Host $responseBody -ForegroundColor White
        } catch {
            Write-Host "Não foi possível ler o response body" -ForegroundColor Red
        }
    }
} 