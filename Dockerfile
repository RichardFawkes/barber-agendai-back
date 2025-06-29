# ===================================
# DOCKERFILE - BARBEARIA SAAS API
# Multi-stage build para .NET 9
# ===================================

# ===================================
# STAGE 1: BASE
# Imagem base para runtime
# ===================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Criar usuário não-root para segurança
RUN addgroup --system --gid 1001 dotnetgroup
RUN adduser --system --uid 1001 --ingroup dotnetgroup dotnetuser

# ===================================
# STAGE 2: BUILD
# Imagem para compilação
# ===================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto primeiro (para cache layers)
COPY ["src/BarbeariaSaaS.API/BarbeariaSaaS.API.csproj", "src/BarbeariaSaaS.API/"]
COPY ["src/BarbeariaSaaS.Application/BarbeariaSaaS.Application.csproj", "src/BarbeariaSaaS.Application/"]
COPY ["src/BarbeariaSaaS.Infrastructure/BarbeariaSaaS.Infrastructure.csproj", "src/BarbeariaSaaS.Infrastructure/"]
COPY ["src/BarbeariaSaaS.Domain/BarbeariaSaaS.Domain.csproj", "src/BarbeariaSaaS.Domain/"]
COPY ["src/BarbeariaSaaS.Shared/BarbeariaSaaS.Shared.csproj", "src/BarbeariaSaaS.Shared/"]

# Restaurar dependências
RUN dotnet restore "src/BarbeariaSaaS.API/BarbeariaSaaS.API.csproj"

# Copiar todo o código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/src/BarbeariaSaaS.API"
RUN dotnet build "BarbeariaSaaS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===================================
# STAGE 3: PUBLISH
# Publicação da aplicação
# ===================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BarbeariaSaaS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===================================
# STAGE 4: FINAL
# Imagem final otimizada
# ===================================
FROM base AS final

# Instalar utilitários necessários
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Configurar timezone
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Configurações de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Definir diretório de trabalho
WORKDIR /app

# Copiar aplicação publicada
COPY --from=publish /app/publish .

# Criar diretórios necessários
RUN mkdir -p /app/logs /app/uploads /app/temp
RUN chown -R dotnetuser:dotnetgroup /app
RUN chmod -R 755 /app

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Usar usuário não-root
USER dotnetuser

# Comando de entrada
ENTRYPOINT ["dotnet", "BarbeariaSaaS.API.dll"] 