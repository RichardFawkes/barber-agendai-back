# 🏪 **BarbeariaSaaS - Sistema de Agendamento para Barbearias**

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-512BD4?style=for-the-badge&logo=microsoft)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=json-web-tokens)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker)
![Render](https://img.shields.io/badge/Render-46E3B7?style=for-the-badge&logo=render)

**API REST robusta e escalável para gestão completa de barbearias**

[📖 Documentação](#-documentação) • [🚀 Instalação](#-instalação) • [🔧 Configuração](#-configuração) • [📡 API](#-endpoints-da-api) • [🌐 Deploy](#-deploy) • [🤝 Contribuição](#-contribuição)

</div>

---

## 📋 **Descrição**

O **BarbeariaSaaS** é uma solução SaaS completa para gestão de barbearias que substitui sistemas baseados em Firebase por uma arquitetura .NET robusta e escalável. O sistema oferece **multitenancy**, permitindo que múltiplas barbearias operem de forma independente em uma única instância da aplicação.

### 🎯 **Principais Funcionalidades**

✂️ **Gestão de Serviços** - Cadastro e gerenciamento de serviços com preços e durações  
📅 **Sistema de Agendamentos** - Agendamentos online com validação de conflitos  
👥 **Gestão de Clientes** - Cadastro automático e histórico de atendimentos  
🏢 **Multitenancy** - Suporte a múltiplas barbearias em uma única instância  
🔐 **Autenticação JWT** - Sistema de autenticação seguro baseado em tokens  
📊 **Dashboard Analítico** - Relatórios e estatísticas em tempo real  
🎨 **Branding Personalizado** - Cada barbearia com sua identidade visual  
📱 **API REST Completa** - Endpoints para frontend web e mobile  

---

## 🏗️ **Arquitetura**

O projeto segue os princípios da **Clean Architecture** com as seguintes camadas:

```
├── 🎯 BarbeariaSaaS.API           # Controllers, configuração e entrada da aplicação
├── 🧠 BarbeariaSaaS.Application   # Casos de uso, CQRS, handlers e interfaces  
├── 🏢 BarbeariaSaaS.Domain        # Entidades, regras de negócio e validações
├── 🔧 BarbeariaSaaS.Infrastructure # Repositórios, EF, serviços externos
├── 📦 BarbeariaSaaS.Shared        # DTOs, contratos e objetos compartilhados
└── 🧪 Tests/                      # Testes unitários, integração e E2E
```

### 🛠️ **Tecnologias Utilizadas**

#### **Backend**
- **.NET 9** - Framework principal
- **ASP.NET Core** - API REST
- **Entity Framework Core** - ORM e acesso a dados
- **SQL Server** - Banco de dados (desenvolvimento)
- **PostgreSQL** - Banco de dados (produção)
- **SQLite** - Banco de dados (fallback)
- **AutoMapper** - Mapeamento objeto-objeto
- **MediatR** - Padrão CQRS e Mediator
- **FluentValidation** - Validação de dados

#### **Autenticação & Segurança**
- **JWT Bearer Authentication** - Autenticação baseada em tokens
- **BCrypt** - Hash de senhas seguro
- **CORS** - Configuração para frontend

#### **Deploy & DevOps**
- **Docker** - Containerização
- **Render.com** - Deploy em produção
- **Swagger/OpenAPI** - Documentação interativa da API

#### **Documentação & Testes**
- **xUnit** - Framework de testes
- **Serilog** - Sistema de logs estruturados

---

## 🚀 **Instalação**

### **Pré-requisitos**

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- **Banco de Dados** (uma das opções):
  - 🏆 **SQL Server LocalDB** (recomendado para Windows)
  - 🔄 **SQLite** (funciona em qualquer OS)
  - 🌐 **PostgreSQL** (automático no Render.com)

### **1. Clone o Repositório**

```bash
git clone https://github.com/seu-usuario/barbearia-saas-backend.git
cd barbearia-saas-backend
```

### **2. Restaurar Dependências**

```bash
dotnet restore
```

### **3. Configurar Banco de Dados**

A aplicação **detecta automaticamente** o ambiente e usa o banco apropriado:

#### **🖥️ Desenvolvimento Local:**

**Opção A - SQL Server LocalDB (Windows - Padrão):**
```bash
# Usar configuração padrão - nada a fazer!
dotnet run --project src/BarbeariaSaaS.API
```

**Opção B - SQLite (Multiplataforma):**
```bash
# Copiar configuração SQLite
cp src/BarbeariaSaaS.API/appsettings.SQLite.json src/BarbeariaSaaS.API/appsettings.Development.json

# Ou rodar diretamente
dotnet run --project src/BarbeariaSaaS.API
```

#### **🌐 Produção (Render.com):**
```bash
# PostgreSQL configurado automaticamente via DATABASE_URL
# Nenhuma configuração manual necessária!
```

### **4. Executar a Aplicação**

```bash
dotnet run --project src/BarbeariaSaaS.API
```

**URLs disponíveis:**
- 🌐 **API**: `http://localhost:5080`
- 📖 **Swagger**: `http://localhost:5080`
- ✅ **Health Check**: `http://localhost:5080/health`

---

## 🔧 **Configuração**

### **🔍 Detecção Automática de Banco**

A aplicação usa **configuração inteligente**:

```mermaid
graph TD
    A[Iniciar App] --> B{DATABASE_URL existe?}
    B -->|Sim| C[🌐 PostgreSQL Cloud]
    B -->|Não| D{appsettings.json tem localdb?}
    D -->|Sim| E[💻 SQL Server LocalDB]
    D -->|Não| F[🔄 SQLite Fallback]
    
    C --> G[✅ App Iniciada]
    E --> G
    F --> G
```

### **📝 Configurações por Ambiente**

#### **Desenvolvimento (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BarbeariaSaaS;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "BarbeariaSaaS-Super-Secret-Key-256-Bits-For-JWT-Token-Security-2024",
    "Issuer": "BarbeariaSaaS",
    "Audience": "BarbeariaSaaS-Users",
    "ExpirationHours": 24
  }
}
```

#### **Produção (Variáveis de Ambiente):**
```bash
# Render.com/Heroku - Configurado automaticamente
DATABASE_URL=postgres://user:pass@host:port/db
JWT_SECRET_KEY=sua-chave-secreta-production-256-bits
JWT_ISSUER=BarbeariaSaaS-Production
JWT_AUDIENCE=BarbeariaSaaS-Users-Production
```

### **🔄 Alternativa SQLite**

Para usar SQLite em desenvolvimento:

```bash
# Opção 1: Copiar configuração
cp src/BarbeariaSaaS.API/appsettings.SQLite.json src/BarbeariaSaaS.API/appsettings.Development.json

# Opção 2: Editar appsettings.json
# Alterar ConnectionString para:
# "DefaultConnection": "Data Source=barbearia_development.db"
```

---

## 📡 **Endpoints da API**

### **🔓 Públicos (Sem Autenticação)**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/health` | Status da API |
| `GET` | `/api/tenant/by-subdomain/{subdomain}` | Dados da barbearia |
| `GET` | `/api/service/public/{subdomain}` | Serviços disponíveis |
| `POST` | `/api/booking/public/{subdomain}` | Criar agendamento |
| `POST` | `/api/auth/login` | Login JWT |

### **🔐 Protegidos (Requer JWT)**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/dashboard/stats` | Estatísticas do dashboard |
| `GET` | `/api/service/tenant/{id}/active` | Serviços da barbearia |
| `POST` | `/api/booking` | Criar agendamento (admin) |

### **📖 Documentação Completa**

Acesse `http://localhost:5080` para ver a documentação interativa completa no Swagger UI.

---

## 💾 **Modelo de Dados**

### **Entidades Principais**

```mermaid
erDiagram
    Tenant ||--o{ User : has
    Tenant ||--o{ Service : offers
    Tenant ||--o{ Customer : serves
    Tenant ||--o{ Booking : manages
    
    Service ||--o{ Booking : books
    Customer ||--o{ Booking : makes
    ServiceCategory ||--o{ Service : categorizes
    
    Tenant {
        guid Id PK
        string Name
        string Subdomain UK
        string Email
        json BrandingJson
        json SettingsJson
        enum Status
        enum Plan
    }
    
    Service {
        guid Id PK
        guid TenantId FK
        string Name
        decimal Price
        int DurationMinutes
        bool IsActive
    }
    
    Booking {
        guid Id PK
        guid TenantId FK
        guid ServiceId FK
        guid CustomerId FK
        date BookingDate
        time BookingTime
        enum Status
        decimal ServicePrice
    }
```

---

## 🧪 **Testes**

### **Executar Todos os Testes**

```bash
dotnet test
```

### **Testes por Categoria**

```bash
# Testes Unitários
dotnet test tests/BarbeariaSaaS.UnitTests

# Testes de Integração  
dotnet test tests/BarbeariaSaaS.IntegrationTests

# Testes End-to-End
dotnet test tests/BarbeariaSaaS.EndToEndTests
```

### **Coverage Report**

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🔒 **Segurança**

### **Autenticação JWT**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@barbearia.com",
  "password": "senha123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "guid",
    "name": "Admin",
    "email": "admin@barbearia.com",
    "role": "TenantAdmin",
    "tenantId": "guid"
  },
  "expiresAt": "2024-12-25T10:00:00Z"
}
```

### **Usar Token nas Requisições**

```http
GET /api/dashboard/stats
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 🌐 **Deploy**

### **🚀 Render.com (Recomendado)**

Para deploy em produção no Render.com:

```bash
# 1. Criar banco PostgreSQL no Render.com
# 2. Configurar variáveis de ambiente
# 3. Deploy automático via GitHub

# Ver guia completo:
```

📖 **[Guia Completo de Deploy no Render.com](docs/RENDER_DEPLOY.md)**

### **🐳 Docker Local**

```bash
# Build e run local
docker build -t barbearia-saas-api .
docker run -p 10000:10000 barbearia-saas-api

# Ou usar docker-compose
docker-compose up -d
```

📖 **[Guia Completo do Docker](docs/DOCKER.md)**

### **☁️ Outras Plataformas**

#### **Azure App Service:**
```bash
# Publicar para Azure
dotnet publish -c Release -o ./publish
# Deploy via Azure CLI ou Visual Studio
```

#### **AWS/Heroku:**
```bash
# Configurar para PostgreSQL e variáveis de ambiente
# Deploy via container ou buildpack
```

---

## 📊 **Monitoramento**

### **Health Checks**

- **API Status**: `GET /health`
- **Database**: Automático via EF Core
- **External Services**: Configurável

### **Logs**

Os logs são estruturados usando **Serilog** e podem ser configurados para:
- Console (desenvolvimento)
- Arquivo (produção)
- Application Insights (Azure)
- Elasticsearch (análise avançada)

---

## 🗂️ **Estrutura do Projeto**

```
📁 BarbeariaSaaS/
├── 📁 src/
│   ├── 🎯 BarbeariaSaaS.API/
│   │   ├── Controllers/          # Controllers da API
│   │   ├── Extensions/           # Extensions e configurações
│   │   └── Program.cs            # Configuração principal
│   │
│   ├── 🧠 BarbeariaSaaS.Application/
│   │   ├── Features/             # CQRS Commands/Queries
│   │   ├── Interfaces/           # Contratos de serviços
│   │   └── Mappings/             # Perfis do AutoMapper
│   │
│   ├── 🏢 BarbeariaSaaS.Domain/
│   │   └── Entities/             # Entidades de domínio
│   │
│   ├── 🔧 BarbeariaSaaS.Infrastructure/
│   │   ├── Data/                 # DbContext e configurações
│   │   ├── Repositories/         # Implementação dos repositórios
│   │   └── Services/             # Serviços de infraestrutura
│   │
│   └── 📦 BarbeariaSaaS.Shared/
│       └── DTOs/                 # Objetos de transferência
│
├── 📁 tests/                     # Projetos de teste
├── 📁 docs/                      # Documentação adicional
│   ├── 📄 RENDER_DEPLOY.md       # Guia de deploy Render.com
│   └── 📄 DOCKER.md              # Guia Docker completo
├── 🐳 Dockerfile                 # Configuração Docker
├── 🐳 docker-compose.yml         # Ambiente completo
├── ⚙️ render.yaml                # Config para Render.com
├── 📋 .gitignore                 # Arquivos ignorados pelo Git
└── 📖 README.md                  # Este arquivo
```

---

## 🤝 **Contribuição**

Contribuições são sempre bem-vindas! Para contribuir:

1. **Fork** o projeto
2. Crie sua **feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit** suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. **Push** para a branch (`git push origin feature/AmazingFeature`)
5. Abra um **Pull Request**

### **Padrões de Código**

- Seguir convenções do **.NET**
- Usar **Clean Code** princípios
- Escrever **testes** para novas funcionalidades
- Manter **documentação** atualizada

---

## 📜 **Licença**

Este projeto está sob a licença **MIT**. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 👨‍💻 **Autor**

**Seu Nome**
- 🐙 GitHub: [@seu-usuario](https://github.com/seu-usuario)
- 💼 LinkedIn: [Seu Perfil](https://linkedin.com/in/seu-perfil)
- 📧 Email: seu.email@exemplo.com

---

## 🙏 **Agradecimentos**

- Comunidade **.NET** pela excelente documentação
- Contribuidores do **Entity Framework Core**
- **Render.com** pela plataforma de deploy gratuita
- Mantenedores das bibliotecas open source utilizadas

---

<div align="center">

**⭐ Se este projeto te ajudou, deixe uma estrela!**

**🚀 Deploy fácil no Render.com → [docs/RENDER_DEPLOY.md](docs/RENDER_DEPLOY.md)**

Feito com ❤️ e ☕ por [Seu Nome](https://github.com/seu-usuario)

</div>
