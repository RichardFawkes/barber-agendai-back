# ===================================
# DOCKER DEV SCRIPTS - BARBEARIA SAAS
# Scripts PowerShell para Windows
# ===================================

param(
    [Parameter(Position=0)]
    [string]$Command = "help",
    
    [Parameter(Position=1)]
    [string]$Service = ""
)

# Cores para output
$Green = "Green"
$Red = "Red" 
$Yellow = "Yellow"
$Blue = "Blue"

# Funções de log
function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Green
}

function Write-Error-Log {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Red
}

function Write-Warning-Log {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Blue
}

# Verificar se Docker está rodando
function Test-Docker {
    try {
        docker info | Out-Null
        return $true
    }
    catch {
        Write-Error-Log "Docker não está rodando!"
        exit 1
    }
}

# Build da aplicação
function Invoke-Build {
    Write-Log "🔨 Building BarbeariaSaaS..."
    docker-compose build
    Write-Log "✅ Build concluído!"
}

# Iniciar ambiente de desenvolvimento
function Start-Development {
    Write-Log "🚀 Iniciando ambiente de desenvolvimento..."
    Test-Docker
    
    # Criar diretórios necessários
    New-Item -ItemType Directory -Force -Path "logs", "uploads", "data\sqlserver" | Out-Null
    
    # Iniciar serviços
    docker-compose up -d
    
    Write-Log "✅ Ambiente iniciado!"
    Write-Info "📍 API: http://localhost:5080"
    Write-Info "📍 Swagger: http://localhost:5080" 
    Write-Info "📍 Adminer: http://localhost:8081"
    Write-Info "📍 Redis Commander: http://localhost:8082"
    Write-Info "📍 SQL Server: localhost:1434"
    
    # Mostrar logs
    docker-compose logs -f barbearia-api
}

# Parar serviços
function Stop-Services {
    Write-Log "🛑 Parando serviços..."
    docker-compose down
    Write-Log "✅ Serviços parados!"
}

# Reiniciar serviços
function Restart-Services {
    Write-Log "🔄 Reiniciando serviços..."
    Stop-Services
    Start-Development
}

# Mostrar logs
function Show-Logs {
    param([string]$ServiceName = "barbearia-api")
    Write-Log "📋 Mostrando logs do serviço: $ServiceName"
    docker-compose logs -f $ServiceName
}

# Limpar ambiente
function Clear-Environment {
    Write-Warning-Log "⚠️  Isso irá remover containers, volumes e imagens!"
    $response = Read-Host "Tem certeza? (y/N)"
    
    if ($response -eq "y" -or $response -eq "Y") {
        Write-Log "🧹 Limpando ambiente Docker..."
        docker-compose down -v --rmi all
        docker system prune -f
        Write-Log "✅ Limpeza concluída!"
    }
    else {
        Write-Info "Operação cancelada."
    }
}

# Executar migrations
function Invoke-Migrations {
    Write-Log "🗄️  Executando migrations..."
    docker-compose exec barbearia-api dotnet ef database update
    Write-Log "✅ Migrations aplicadas!"
}

# Backup do banco
function Invoke-Backup {
    $backupFile = "backup\db_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
    Write-Log "💾 Criando backup: $backupFile"
    
    New-Item -ItemType Directory -Force -Path "backup" | Out-Null
    
    docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!" -Q "BACKUP DATABASE [BarbeariaSaaS_Dev] TO DISK = '/tmp/backup.bak'"
    
    $containerId = docker-compose ps -q sqlserver
    docker cp "${containerId}:/tmp/backup.bak" $backupFile
    
    Write-Log "✅ Backup criado: $backupFile"
}

# Status dos serviços
function Show-Status {
    Write-Log "📊 Status dos serviços:"
    docker-compose ps
    Write-Host ""
    Write-Info "🔗 URLs:"
    Write-Host "  API: http://localhost:5080"
    Write-Host "  Swagger: http://localhost:5080"
    Write-Host "  Adminer: http://localhost:8081"  
    Write-Host "  Redis Commander: http://localhost:8082"
}

# Shell no container
function Enter-Shell {
    param([string]$ServiceName = "barbearia-api")
    Write-Log "🐚 Abrindo shell no container: $ServiceName"
    docker-compose exec $ServiceName /bin/bash
}

# Menu de ajuda
function Show-Help {
    Write-Host "=== BARBEARIA SAAS - Docker Development Helper ===" -ForegroundColor $Blue
    Write-Host ""
    Write-Host "Uso: .\docker-dev.ps1 [comando] [parâmetro]"
    Write-Host ""
    Write-Host "Comandos disponíveis:"
    Write-Host "  build     - Build da aplicação"
    Write-Host "  dev       - Iniciar ambiente de desenvolvimento" 
    Write-Host "  stop      - Parar todos os serviços"
    Write-Host "  restart   - Reiniciar serviços"
    Write-Host "  logs      - Mostrar logs (opcional: nome do serviço)"
    Write-Host "  clean     - Limpar containers, volumes e imagens"
    Write-Host "  migrate   - Executar migrations do banco"
    Write-Host "  backup    - Criar backup do banco de dados"
    Write-Host "  status    - Mostrar status dos serviços"
    Write-Host "  shell     - Abrir shell no container (opcional: nome do serviço)"
    Write-Host "  help      - Mostrar esta ajuda"
    Write-Host ""
    Write-Host "Exemplos:"
    Write-Host "  .\docker-dev.ps1 dev"
    Write-Host "  .\docker-dev.ps1 logs barbearia-api"
    Write-Host "  .\docker-dev.ps1 shell sqlserver"
}

# Main switch
switch ($Command.ToLower()) {
    "build" { Invoke-Build }
    "dev" { Start-Development }
    "start" { Start-Development }
    "stop" { Stop-Services }
    "restart" { Restart-Services }
    "logs" { 
        if ($Service) { Show-Logs -ServiceName $Service }
        else { Show-Logs }
    }
    "clean" { Clear-Environment }
    "migrate" { Invoke-Migrations }
    "backup" { Invoke-Backup }
    "status" { Show-Status }
    "shell" { 
        if ($Service) { Enter-Shell -ServiceName $Service }
        else { Enter-Shell }
    }
    default { Show-Help }
} 