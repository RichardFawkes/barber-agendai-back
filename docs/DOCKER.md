# 🐳 **Docker - BarbeariaSaaS**

Guia completo para desenvolvimento e deploy usando Docker.

---

## 📋 **Pré-requisitos**

- **Docker Desktop** instalado e rodando
- **Docker Compose** v2.0+
- **Git** para clonar o repositório

### **Instalação do Docker:**

#### **Windows:**
1. Baixe o [Docker Desktop for Windows](https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe)
2. Execute o instalador
3. Reinicie o computador se necessário

#### **macOS:**
```bash
# Via Homebrew
brew install --cask docker

# Ou baixe diretamente
# https://desktop.docker.com/mac/main/amd64/Docker.dmg
```

#### **Linux (Ubuntu/Debian):**
```bash
# Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Instalar Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

---

## 🚀 **Uso Rápido**

### **🔧 Scripts Auxiliares**

O projeto inclui scripts para facilitar o desenvolvimento:

#### **Linux/macOS:**
```bash
# Dar permissão (primeira vez)
chmod +x scripts/docker-dev.sh

# Iniciar ambiente de desenvolvimento
./scripts/docker-dev.sh dev

# Ver ajuda
./scripts/docker-dev.sh help
```

#### **Windows (PowerShell):**
```powershell
# Iniciar ambiente de desenvolvimento
.\scripts\docker-dev.ps1 dev

# Ver ajuda
.\scripts\docker-dev.ps1 help
```

### **📱 URLs Disponíveis:**

Após iniciar o ambiente:

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **API** | http://localhost:5080 | BarbeariaSaaS API |
| **Swagger** | http://localhost:5080 | Documentação interativa |
| **Adminer** | http://localhost:8081 | Interface do banco de dados |
| **Redis Commander** | http://localhost:8082 | Interface do Redis |
| **SQL Server** | localhost:1434 | Conexão direta ao banco |

---

## 🏗️ **Arquitetura Docker**

### **Containers:**

```
📦 barbearia-saas-api     → API Principal (.NET 9)
📦 barbearia-saas-db      → SQL Server 2022
📦 barbearia-saas-redis   → Redis 7 (Cache)
📦 barbearia-saas-adminer → Interface do BD
📦 barbearia-saas-redis-commander → Interface Redis
```

### **Volumes:**
```
🗄️ barbearia-saas-sqlserver-data → Dados do SQL Server
📋 barbearia-saas-logs           → Logs da aplicação  
📁 barbearia-saas-uploads        → Arquivos carregados
💾 barbearia-saas-redis-data     → Cache Redis
```

### **Rede:**
```
🌐 barbearia-saas-network → Rede interna Docker
```

---

## 📝 **Comandos Detalhados**

### **🔨 Build & Start**

```bash
# Build da imagem
docker-compose build

# Iniciar todos os serviços
docker-compose up -d

# Iniciar com logs em tempo real
docker-compose up

# Build e start em um comando
docker-compose up --build
```

### **🛑 Stop & Clean**

```bash
# Parar serviços
docker-compose down

# Parar e remover volumes
docker-compose down -v

# Limpeza completa (containers, volumes, imagens)
docker-compose down -v --rmi all
docker system prune -a
```

### **📋 Logs & Debug**

```bash
# Logs de todos os serviços
docker-compose logs

# Logs em tempo real
docker-compose logs -f

# Logs de um serviço específico
docker-compose logs -f barbearia-api

# Shell no container da API
docker-compose exec barbearia-api /bin/bash

# Shell no SQL Server
docker-compose exec sqlserver /bin/bash
```

### **🗄️ Banco de Dados**

```bash
# Conectar ao SQL Server
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!"

# Executar migrations
docker-compose exec barbearia-api dotnet ef database update

# Backup do banco
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!" -Q "BACKUP DATABASE [BarbeariaSaaS_Dev] TO DISK = '/tmp/backup.bak'"
```

---

## 🌍 **Ambientes**

### **🔧 Desenvolvimento (docker-compose.override.yml)**

Configurações para desenvolvimento local:

```yaml
# Configurações automáticas em desenvolvimento:
- Porta API: 5080 (em vez de 8080)
- Porta SQL: 1434 (em vez de 1433)  
- Banco: BarbeariaSaaS_Dev
- JWT expira em 2 horas
- Logs mais detalhados
- Adminer e Redis Commander incluídos
```

### **🚀 Produção (docker-compose.yml)**

Configurações para produção:

```yaml
# Configurações de produção:
- Porta API: 8080
- Porta SQL: 1433
- Banco: BarbeariaSaaS
- JWT expira em 24 horas
- Logs otimizados
- Apenas serviços essenciais
```

### **🔒 Produção com Docker Compose:**

```bash
# Deploy em produção
docker-compose -f docker-compose.yml up -d

# Com configurações customizadas
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## 🔧 **Configurações**

### **⚙️ Variáveis de Ambiente**

#### **Desenvolvimento (.env):**
```bash
# Criar arquivo .env na raiz do projeto
echo "ASPNETCORE_ENVIRONMENT=Development
SA_PASSWORD=DevPassword123!
JWT_SECRET=Dev-Secret-Key-256-Bits
REDIS_PASSWORD=" > .env
```

#### **Produção:**
```bash
# Configurar variáveis no servidor
export ASPNETCORE_ENVIRONMENT=Production
export SA_PASSWORD=ProducaoSenhaForte123!
export JWT_SECRET=Producao-Super-Secret-Key-256-Bits-Secure
export REDIS_PASSWORD=RedisPasswordForte123!
```

### **🏥 Health Checks**

Todos os containers têm health checks configurados:

```bash
# Verificar status
docker-compose ps

# Status detalhado
docker inspect $(docker-compose ps -q) | grep -A 5 '"Health"'
```

### **📊 Monitoramento**

```bash
# Stats em tempo real
docker stats

# Uso de recursos por container
docker-compose top

# Logs estruturados
docker-compose logs --since 1h | grep ERROR
```

---

## 🔍 **Troubleshooting**

### **❌ Problemas Comuns**

#### **1. Porta já em uso:**
```bash
# Verificar o que está usando a porta
netstat -an | grep :5080
# ou
lsof -i :5080

# Matar processo se necessário
sudo kill -9 $(lsof -t -i:5080)
```

#### **2. SQL Server não inicia:**
```bash
# Verificar logs
docker-compose logs sqlserver

# Verificar permissões de volume
sudo chown -R 10001:10001 data/sqlserver/
```

#### **3. API não conecta no banco:**
```bash
# Verificar se SQL Server está healthy
docker-compose ps

# Testar conexão manual
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!" -Q "SELECT 1"
```

#### **4. Erro de permissão (Linux):**
```bash
# Dar permissões aos scripts
chmod +x scripts/*.sh

# Adicionar usuário ao grupo docker
sudo usermod -aG docker $USER
newgrp docker
```

### **🧹 Reset Completo**

Se algo der muito errado:

```bash
# Parar tudo
docker-compose down -v

# Limpar containers órfãos
docker container prune -f

# Limpar volumes não usados
docker volume prune -f

# Limpar imagens não usadas
docker image prune -a

# Rebuild do zero
docker-compose build --no-cache
docker-compose up -d
```

---

## 📈 **Performance**

### **🎯 Otimizações**

#### **Build Cache:**
```bash
# Build com cache otimizado
docker-compose build --parallel

# Usar BuildKit para builds mais rápidos
export DOCKER_BUILDKIT=1
docker-compose build
```

#### **Recursos:**
```yaml
# Limitar recursos dos containers (docker-compose.yml)
services:
  barbearia-api:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
```

#### **Volume Performance:**
```yaml
# Para desenvolvimento mais rápido (macOS/Windows)
volumes:
  - ./src:/app/src:cached  # cached para read-heavy
  - ./logs:/app/logs:delegated  # delegated para write-heavy
```

---

## 🔐 **Segurança**

### **🛡️ Melhores Práticas**

#### **Secrets Management:**
```bash
# Usar Docker secrets em produção
echo "SuperSecretPassword" | docker secret create db_password -
```

#### **Usuários não-root:**
```dockerfile
# Dockerfile já configurado com usuário não-root
USER dotnetuser
```

#### **Rede isolada:**
```yaml
# Containers se comunicam apenas via rede interna
networks:
  barbearia-network:
    driver: bridge
    internal: true  # Para produção
```

---

## 📚 **Scripts Disponíveis**

### **🐧 Linux/macOS (docker-dev.sh)**

```bash
./scripts/docker-dev.sh build     # Build da aplicação
./scripts/docker-dev.sh dev       # Iniciar desenvolvimento
./scripts/docker-dev.sh stop      # Parar serviços
./scripts/docker-dev.sh restart   # Reiniciar
./scripts/docker-dev.sh logs      # Ver logs
./scripts/docker-dev.sh clean     # Limpeza completa
./scripts/docker-dev.sh migrate   # Executar migrations
./scripts/docker-dev.sh backup    # Backup do banco
./scripts/docker-dev.sh status    # Status dos serviços
./scripts/docker-dev.sh shell     # Shell no container
```

### **🪟 Windows (docker-dev.ps1)**

```powershell
.\scripts\docker-dev.ps1 build     # Build da aplicação
.\scripts\docker-dev.ps1 dev       # Iniciar desenvolvimento  
.\scripts\docker-dev.ps1 stop      # Parar serviços
.\scripts\docker-dev.ps1 restart   # Reiniciar
.\scripts\docker-dev.ps1 logs      # Ver logs
.\scripts\docker-dev.ps1 clean     # Limpeza completa
.\scripts\docker-dev.ps1 migrate   # Executar migrations
.\scripts\docker-dev.ps1 backup    # Backup do banco
.\scripts\docker-dev.ps1 status    # Status dos serviços
.\scripts\docker-dev.ps1 shell     # Shell no container
```

---

## 🎯 **Deploy em Produção**

### **☁️ Deploy na Nuvem**

#### **Azure Container Instances:**
```bash
# Login no Azure
az login

# Criar resource group
az group create --name BarbeariaSaaS --location eastus

# Deploy
az container create \
  --resource-group BarbeariaSaaS \
  --name barbearia-api \
  --image your-registry/barbearia-saas-api:latest \
  --dns-name-label barbearia-api \
  --ports 80
```

#### **AWS ECS:**
```bash
# Build e push para ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 123456789.dkr.ecr.us-east-1.amazonaws.com
docker build -t barbearia-saas-api .
docker tag barbearia-saas-api:latest 123456789.dkr.ecr.us-east-1.amazonaws.com/barbearia-saas-api:latest
docker push 123456789.dkr.ecr.us-east-1.amazonaws.com/barbearia-saas-api:latest
```

#### **VPS/Servidor Próprio:**
```bash
# Copiar arquivos para servidor
scp -r . user@servidor:/opt/barbearia-saas/

# SSH no servidor
ssh user@servidor

# Deploy
cd /opt/barbearia-saas
docker-compose -f docker-compose.yml up -d
```

---

## 📞 **Suporte**

Se encontrar problemas:

1. **Verifique os logs:** `docker-compose logs`
2. **Consulte este guia:** Procure na seção troubleshooting
3. **Issues do GitHub:** Abra uma issue com logs detalhados
4. **Discord/Slack:** Entre no canal de desenvolvimento

---

<div align="center">

**🐳 Docker configurado com sucesso!**

*Agora você pode desenvolver localmente com um ambiente completo e consistente.*

</div> 