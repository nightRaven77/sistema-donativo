# ====================================================================
# Etapa 1: Compilación y publicación de la API en el Monorepo
# ====================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copiar csproj para restaurar capas
COPY ["src/SistemaDonativos.Shared/SistemaDonativos.Shared.csproj", "src/SistemaDonativos.Shared/"]
COPY ["src/SistemaDonativos.Api/SistemaDonativos.Api.csproj", "src/SistemaDonativos.Api/"]
RUN dotnet restore "src/SistemaDonativos.Api/SistemaDonativos.Api.csproj"

# Copiar código fuente y compilar en Release
COPY ["src/SistemaDonativos.Shared/", "src/SistemaDonativos.Shared/"]
COPY ["src/SistemaDonativos.Api/", "src/SistemaDonativos.Api/"]
WORKDIR "/src/src/SistemaDonativos.Api"
RUN dotnet publish "SistemaDonativos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ====================================================================
# Etapa 2: Imagen de ejecución (Runtime ligera y segura)
# ====================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV logPath__path=/app/logs/api-.txt

# Crear carpeta de logs e instalar curl para healthcheck
RUN mkdir -p /app/logs && \
    apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/* && \
    chown -R $APP_UID /app/logs

USER $APP_UID

COPY --from=build /app/publish .

# Verificación de salud integrada en /health
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SistemaDonativos.Api.dll"]
