# ====================================================================
# Etapa 1: Compilación y publicación del proyecto Web MVC
# ====================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY ["donativos.csproj", "./"]
RUN dotnet restore "donativos.csproj"

# Copiar todo el código y publicar
COPY . .
RUN dotnet publish "donativos.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ====================================================================
# Etapa 2: Imagen de ejecución (Runtime ligera y segura)
# ====================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "donativos.dll"]
