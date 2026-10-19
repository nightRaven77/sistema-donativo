# Sistema de Donativos (Monorepo)

Sistema integral de gestión de donaciones, control de existencias en almacenes, salidas a beneficiarios y traspasos logísticos entre centros de acopio en tiempo real.

---

## 🏛️ Arquitectura del Proyecto

El proyecto está organizado como un **Monorepo** moderno en **.NET 10** bajo una única solución (`SistemaDonativos.sln`), permitiendo compartir contratos, optimizar la compilación y gestionar los despliegues de forma unificada.

```
sistema-donativos/
├── SistemaDonativos.sln         # Solución principal que agrupa todos los proyectos
├── docker-compose.yml           # Orquestación de servicios (Base de Datos + API + Web)
├── .dockerignore                # Reglas de exclusión para imágenes Docker
├── respaldoDonativos.bak        # Respaldo limpio de la base de datos SQL Server
├── respaldo.sql                 # Script SQL de estructura/datos
├── src/
│   ├── SistemaDonativos.Shared/ # Biblioteca de clases común (.NET 10)
│   │   ├── Models/              # DTOs y contratos compartidos (ej. AuthDtos.cs)
│   │   └── SistemaDonativos.Shared.csproj
│   │
│   ├── SistemaDonativos.Api/    # Backend REST API (.NET 10)
│   │   ├── Controllers/         # Endpoints REST (Auth, Donativos, Inventario, etc.)
│   │   ├── Middleware/          # Manejador global de excepciones y correlación
│   │   ├── Services/            # Lógica de negocio y accesos asíncronos a BD
│   │   ├── Dockerfile           # Construcción multi-stage optimizada
│   │   └── SistemaDonativos.Api.csproj
│   │
│   └── SistemaDonativos.Web/    # Frontend MVC (.NET 10)
│       ├── Controllers/         # Controladores de presentación (Acceso, Registros, etc.)
│       ├── Views/               # Vistas Razor con diseño responsivo moderno
│       ├── wwwroot/             # Assets estáticos (CSS, JS, imágenes institucionales)
│       ├── Dockerfile           # Construcción multi-stage
│       └── SistemaDonativos.Web.csproj
```

---

## 🚀 Despliegue Rápido con Docker Compose

La forma recomendada y más rápida de ejecutar el ecosistema completo (Frontend, API y Base de Datos) es usando **Docker Compose**:

### 1. Iniciar todos los servicios:
```bash
docker compose up -d
```

### 2. Servicios disponibles:

| Componente | Servicio | URL / Puerto | Descripción |
| :--- | :--- | :--- | :--- |
| **Frontend Web** | `web` | [http://localhost:8080](http://localhost:8080) | Interfaz visual del sistema |
| **API REST** | `api` | [http://localhost:5001](http://localhost:5001) | Backend y servicios REST |
| **Health Check** | `api` | [http://localhost:5001/health](http://localhost:5001/health) | Diagnóstico de salud en vivo |
| **SQL Server** | `sqlserver`| `localhost:1433` | Motor de base de datos |

> [!NOTE]  
> El puerto de la API en el host está mapeado a `5001:8080` para evitar colisiones con el receptor AirPlay de macOS que reserva el puerto 5000.

---

## 🗄️ Base de Datos y Restauración

El entorno de base de datos utiliza **Microsoft SQL Server 2022** con emulación `linux/amd64` para compatibilidad nativa tanto en procesadores Intel/AMD como en **Apple Silicon (M1/M2/M3/M4)**.

### Credenciales predeterminadas de desarrollo:
- **Servidor**: `localhost,1433`
- **Usuario**: `sa`
- **Contraseña**: `Donativos2026!`
- **Base de Datos**: `DonativosDB`
- **Trust Server Certificate**: `True`

### Cómo restaurar la base de datos desde el archivo `.bak`:
Si creas un contenedor nuevo o necesitas restablecer la base de datos a su estado inicial, ejecuta:

```bash
docker exec -i donativos-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Donativos2026!' -C -Q \
"RESTORE DATABASE [DonativosDB] FROM DISK = '/var/opt/mssql/backup/respaldoDonativos.bak' \
WITH MOVE 'DonativosDB' TO '/var/opt/mssql/data/DonativosDB.mdf', \
MOVE 'DonativosDB_log' TO '/var/opt/mssql/data/DonativosDB_log.ldf', REPLACE;"
```

Para verificar que las 21 tablas se hayan restaurado:
```bash
docker exec -i donativos-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Donativos2026!' -C -d DonativosDB -Q \
"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
```

---

## 💻 Desarrollo Local (CLI de .NET)

Si deseas ejecutar o depurar los proyectos directamente en tu máquina anfitriona sin contenedores:

### Requisitos previos:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- Instancia de SQL Server activa con la base `DonativosDB`

### 1. Restaurar dependencias y compilar:
```bash
dotnet restore SistemaDonativos.sln
dotnet build SistemaDonativos.sln
```

### 2. Ejecutar la API:
```bash
dotnet run --project src/SistemaDonativos.Api
```

### 3. Ejecutar el Portal Web:
```bash
dotnet run --project src/SistemaDonativos.Web
```

---

## ⚙️ Variables de Entorno y Configuración

| Variable | Descripción | Valor por Defecto en Docker |
| :--- | :--- | :--- |
| `ConnectionStrings__donativosCn` | Cadena de conexión a SQL Server | `Server=sqlserver,1433;Database=DonativosDB;User Id=sa;...` |
| `apiUrl` | URL de la API consumida por la Web | `http://api:8080/` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ASP.NET Core | `Production` (Docker) / `Development` (Local) |
| `logPath__path` | Ruta para logs estructurados | `/app/logs/api-.txt` |

---

## ✨ Mejoras de Arquitectura y Rendimiento Implementadas

1. **Monorepo Unificado**:
   - Integración de proyectos antes dispersos en repositorios separados en una arquitectura limpia y desacoplada mediante `src/SistemaDonativos.Shared`.
2. **Acceso Asíncrono no Bloqueante**:
   - Migración de métodos sincrónicos a `async`/`await` (`OpenAsync()`, `ExecuteReaderAsync()`, `CancellationToken`) en servicios críticos de autenticación y datos, liberando hilos del ThreadPool de ASP.NET Core.
3. **Observabilidad y Manejo Global de Errores**:
   - `GlobalExceptionHandlerMiddleware`: Captura centralizada de excepciones que genera respuestas consistentes `{ message, reference_id, status }`.
   - Trazabilidad con cabecera `X-Correlation-Id` en cada petición.
   - Endpoint de salud nativo `/health` que responde `200 OK (Healthy)`.
   - Logging estructurado con `ILogger<T>` reemplazando impresiones directas en consola.
4. **Seguridad y Compatibilidad Linux en Contenedores**:
   - Estandarización de nombres de assets estáticos a minúsculas (`login.css`), resolviendo discrepancias de *case-sensitivity* en sistemas de archivos Linux.
   - Ejecución de contenedores bajo usuarios no privilegiados en producción.
5. **Renovación Visual y Accesibilidad**:
   - Rediseño de la interfaz de acceso utilizando **Plus Jakarta Sans**, efectos de profundidad ambiental, soporte para lectores de pantalla, estados `:focus-visible` accesibles y protección contra doble envío mediante feedback visual animado.
