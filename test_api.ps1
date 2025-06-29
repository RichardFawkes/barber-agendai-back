$headers = @{
    "Content-Type" = "application/json"
}

$body = @{
    tenant = @{
        name = "Barbearia Teste PowerShell"
        description = "Barbearia para teste do endpoint com PowerShell"
        subdomain = "testeps"
        phone = "(11) 99999-9999"
        email = "teste@barbeariatest.com"
        address = "Rua de Teste PowerShell, 123 - Centro, São Paulo - SP"
        website = "www.barbeariatest.com.br"
    }
    admin = @{
        name = "Admin PowerShell"
        email = "admin@barbeariatest.com"
        phone = "(11) 98888-8888"
        password = "testeps123"
    }
} | ConvertTo-Json -Depth 3

Write-Host "Testando endpoint: POST http://localhost:5000/api/tenant/create"
Write-Host "Aguardando 5 segundos para aplicação inicializar..."
Start-Sleep 5

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/tenant/create" -Method POST -Headers $headers -Body $body
    Write-Host "✅ Sucesso!" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "❌ Erro:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody"
    }
} 