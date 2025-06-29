#!/bin/bash

# ===================================
# DOCKER DEV SCRIPTS - BARBEARIA SAAS
# Scripts para facilitar desenvolvimento
# ===================================

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para log
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

error() {
    echo -e "${RED}[ERROR] $1${NC}"
}

warning() {
    echo -e "${YELLOW}[WARNING] $1${NC}"
}

info() {
    echo -e "${BLUE}[INFO] $1${NC}"
}

# Função para verificar se Docker está rodando
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        error "Docker não está rodando!"
        exit 1
    fi
}

# Função para build
build() {
    log "🔨 Building BarbeariaSaaS..."
    docker-compose build
    log "✅ Build concluído!"
}

# Função para iniciar em modo desenvolvimento
dev() {
    log "🚀 Iniciando ambiente de desenvolvimento..."
    check_docker
    
    # Criar diretórios necessários
    mkdir -p logs uploads data/sqlserver
    
    # Iniciar serviços
    docker-compose up -d
    
    log "✅ Ambiente iniciado!"
    info "📍 API: http://localhost:5080"
    info "📍 Swagger: http://localhost:5080"
    info "📍 Adminer: http://localhost:8081"
    info "📍 Redis Commander: http://localhost:8082"
    info "📍 SQL Server: localhost:1434"
    
    # Mostrar logs
    docker-compose logs -f barbearia-api
}

# Função para parar serviços
stop() {
    log "🛑 Parando serviços..."
    docker-compose down
    log "✅ Serviços parados!"
}

# Função para restart
restart() {
    log "🔄 Reiniciando serviços..."
    stop
    dev
}

# Função para logs
logs() {
    local service=${1:-barbearia-api}
    log "📋 Mostrando logs do serviço: $service"
    docker-compose logs -f $service
}

# Função para limpar tudo
clean() {
    warning "⚠️  Isso irá remover containers, volumes e imagens!"
    read -p "Tem certeza? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        log "🧹 Limpando ambiente Docker..."
        docker-compose down -v --rmi all
        docker system prune -f
        log "✅ Limpeza concluída!"
    else
        info "Operação cancelada."
    fi
}

# Função para executar migrations
migrate() {
    log "🗄️  Executando migrations..."
    docker-compose exec barbearia-api dotnet ef database update
    log "✅ Migrations aplicadas!"
}

# Função para backup do banco
backup() {
    local backup_file="backup/db_$(date +%Y%m%d_%H%M%S).bak"
    log "💾 Criando backup: $backup_file"
    
    mkdir -p backup
    
    docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "DevPassword123!" \
        -Q "BACKUP DATABASE [BarbeariaSaaS_Dev] TO DISK = '/tmp/backup.bak'"
    
    docker cp $(docker-compose ps -q sqlserver):/tmp/backup.bak $backup_file
    
    log "✅ Backup criado: $backup_file"
}

# Função para mostrar status
status() {
    log "📊 Status dos serviços:"
    docker-compose ps
    echo
    info "🔗 URLs:"
    echo "  API: http://localhost:5080"
    echo "  Swagger: http://localhost:5080"
    echo "  Adminer: http://localhost:8081"
    echo "  Redis Commander: http://localhost:8082"
}

# Função para shell no container
shell() {
    local service=${1:-barbearia-api}
    log "🐚 Abrindo shell no container: $service"
    docker-compose exec $service /bin/bash
}

# Menu de ajuda
help() {
    echo -e "${BLUE}=== BARBEARIA SAAS - Docker Development Helper ===${NC}"
    echo
    echo "Uso: ./docker-dev.sh [comando]"
    echo
    echo "Comandos disponíveis:"
    echo "  build     - Build da aplicação"
    echo "  dev       - Iniciar ambiente de desenvolvimento"
    echo "  stop      - Parar todos os serviços"
    echo "  restart   - Reiniciar serviços"
    echo "  logs      - Mostrar logs (opcional: nome do serviço)"
    echo "  clean     - Limpar containers, volumes e imagens"
    echo "  migrate   - Executar migrations do banco"
    echo "  backup    - Criar backup do banco de dados"
    echo "  status    - Mostrar status dos serviços"
    echo "  shell     - Abrir shell no container (opcional: nome do serviço)"
    echo "  help      - Mostrar esta ajuda"
    echo
    echo "Exemplos:"
    echo "  ./docker-dev.sh dev"
    echo "  ./docker-dev.sh logs barbearia-api"
    echo "  ./docker-dev.sh shell sqlserver"
}

# Main
case "${1:-help}" in
    build)
        build
        ;;
    dev|start)
        dev
        ;;
    stop)
        stop
        ;;
    restart)
        restart
        ;;
    logs)
        logs $2
        ;;
    clean)
        clean
        ;;
    migrate)
        migrate
        ;;
    backup)
        backup
        ;;
    status)
        status
        ;;
    shell)
        shell $2
        ;;
    help|*)
        help
        ;;
esac 