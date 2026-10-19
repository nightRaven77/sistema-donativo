# ====================================================================
# Etapa 1: Compilación y publicación del Frontend Web MVC en el Monorepo
# ====================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copiar csproj para restaurar capas
COPY ["src/SistemaDonativos.Shared/SistemaDonativos.Shared.csproj", "src/SistemaDonativos.Shared/"]
COPY ["src/SistemaDonativos.Web/SistemaDonativos.Web.csproj", "src/SistemaDonativos.Web/"]
RUN dotnet restore "src/SistemaDonativos.Web/SistemaDonativos.Web.csproj"

# Copiar código fuente y compilar en Release
COPY ["src/SistemaDonativos.Shared/", "src/SistemaDonativos.Shared/"]
COPY ["src/SistemaDonativos.Web/", "src/SistemaDonativos.Web/"]
WORKDIR "/src/src/SistemaDonativos.Web"
RUN dotnet publish "SistemaDonativos.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ====================================================================
# Etapa 2: Imagen de ejecución (Runtime ligera y segura)
# ====================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SistemaDonativos.Web.dll"]
