# Especificación Técnica - API Donativos
## Instrucciones para Agentes de IA y Desarrollo

---

## 📋 INFORMACIÓN DEL PROYECTO

### Stack Tecnológico
- **.NET Version**: .NET 10
- **C# Version**: 14.0
- **Framework**: ASP.NET Core Web API
- **ORM**: Entity Framework Core
- **Base de Datos**: SQL Server (Stored Procedures)
- **Logging**: Serilog (Console + File)
- **IDE**: Visual Studio 2022/2026

### Ubicación del Proyecto
- **Path**: `api-donativos/`
- **Git Branch**: `main`
- **Remote**: `origin`

---

## 🏗️ ARQUITECTURA Y PATRONES

### 1. Patrón de Capas
```
Controllers/        → API Endpoints (HTTP)
├─ Services/        → Lógica de negocio (Interfaces + Implementaciones)
├─ Models/          → Entidades de dominio
│  ├─ Dtos/         → Data Transfer Objects (Request/Response)
│  └─ Catalogos/    → Entidades de catálogos
└─ Data/            → Persistencia
   ├─ ApplicationDbContext.cs
   └─ Interceptors/ → Auditoría automática
```

### 2. Dependency Injection
**SIEMPRE usar inyección de dependencias:**
```csharp
public class MiController : ControllerBase
{
    private readonly IMiServicio _servicio;
    private readonly ILogger<MiController> _logger;

    public MiController(IMiServicio servicio, ILogger<MiController> logger)
    {
        _servicio = servicio;
        _logger = logger;
    }
}
```

### 3. Patrón Repository/Service
- **Interfaces**: Declarar en el mismo archivo del servicio (`Services/`)
- **Implementación**: Clase concreta que implementa la interfaz
- **Registro**: En `Startup.cs` → `services.AddScoped<IInterface, Implementation>()`

---

## 📝 CONVENCIONES DE CÓDIGO

### Nomenclatura

#### C# (Backend)
- **Clases/Interfaces**: `PascalCase` → `UsuarioService`, `IUsuario`
- **Métodos**: `PascalCase` → `RegistrarAsync()`
- **Parámetros/Variables locales**: `camelCase` → `usuarioId`, `request`
- **Constantes**: `UPPER_SNAKE_CASE` o `PascalCase`
- **Campos privados**: `_camelCase` → `_logger`, `_context`

#### DTOs y Propiedades de BD
- **Propiedades de DTOs**: `camelCase` (coincide con columnas SQL)
```csharp
public class RegistrarUsuarioRequest
{
    public string usuario { get; set; }
    public string password { get; set; }
    public int centroID { get; set; }
}
```

#### Base de Datos
- **Tablas**: `snake_case` → `donativos_detalle`, `salidas_archivos`
- **Stored Procedures**: `PascalCase` → `RegistrarUsuario`, `ActualizarPassword`
- **Parámetros SP**: `@camelCase` → `@usuario`, `@centroID`

### Estructura de Archivos
```
Controllers/
├─ UsuarioController.cs
├─ DonativoController.cs
└─ [Entidad]Controller.cs

Services/
├─ UsuarioService.cs        # Interface IUsuario + UsuarioService
└─ [Entidad]Service.cs

Models/
├─ Dtos/
│  ├─ RegistrarUsuarioRequest.cs
│  └─ [Entidad]Request.cs   # Request + Response en mismo archivo
└─ Catalogos/
   └─ [Entidad].cs          # Entidades de dominio
```

---

## 🔐 MANEJO DE ERRORES Y LOGGING

### Patrón Estándar de Controladores

```csharp
[HttpPost("accion")]
[AllowAnonymous] // o [Authorize] según corresponda
public async Task<ActionResult<Response>> Accion([FromBody] Request request)
{
    // 1. Validación de entrada
    if (request == null)
        return BadRequest(new { mensaje = "Datos no proporcionados" });

    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    try
    {
        // 2. Ejecutar lógica de negocio
        var resultado = await _servicio.MetodoAsync(request);

        // 3. Validar resultado
        if (!resultado.exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }
    catch (ArgumentException ex)
    {
        // 4. Errores de validación
        return BadRequest(new { mensaje = ex.Message });
    }
    catch (Exception ex)
    {
        // 5. Errores inesperados con Reference ID
        var referenceId = Guid.NewGuid().ToString();
        _logger.LogError(ex, "Error en [Operación]. ReferenceId: {ReferenceId}", referenceId);

        #if DEBUG
            return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
        #else
            return StatusCode(500, new { mensaje = "Error al procesar la solicitud", reference_id = referenceId });
        #endif
    }
}
```

### Niveles de Logging (Serilog)

```csharp
// Inicio de operación (sin datos sensibles)
_logger.LogInformation(
    "Iniciando [operación]. Usuario: {Username}, ID: {Id}",
    request.usuario,
    request.id
);

// Éxito
_logger.LogInformation(
    "Operación exitosa. ID: {Id}, Resultado: {Resultado}",
    resultado.id,
    resultado.mensaje
);

// Advertencia (error de negocio esperado)
_logger.LogWarning(
    "Operación fallida. Usuario: {Username}, Razón: {Razon}",
    request.usuario,
    resultado.mensaje
);

// Error (excepción)
_logger.LogError(
    ex,
    "Error en [operación]. Usuario: {Username}",
    request.usuario
);
```

**⚠️ NUNCA registrar contraseñas o datos sensibles en logs**

---

## 🔄 MANEJO DE RECURSOS DUPLICADOS

### Excepción Personalizada
Usar `DuplicateResourceException` para recursos duplicados en operaciones CREATE:

```csharp
// En el Servicio
if (existeRecurso != null)
{
    throw new DuplicateResourceException(
        message: $"Ya existe un {tipoRecurso} con {campo} '{valor}'",
        resourceId: existeRecurso.id,
        resourceCode: existeRecurso.codigo,  // Si aplica (null si no tiene)
        resourceType: "NombreRecurso",       // ej: "Beneficiario", "Donador"
        conflictingField: "nombre"            // ej: "nombre", "email", "codigo"
    );
}
```

### Captura en Controlador
```csharp
catch (DuplicateResourceException ex)
{
    // Log de advertencia (no es error de sistema)
    _logger.LogWarning(
        "Intento de crear recurso duplicado. Usuario: {UserId}, Tipo: {ResourceType}, ID Existente: {ExistingId}",
        id_usuario,
        ex.ResourceType,
        ex.ResourceId
    );

    // Retornar HTTP 409 Conflict con información estructurada
    return Conflict(new 
    { 
        message = ex.Message,
        resourceType = ex.ResourceType,
        conflictingField = ex.ConflictingField,
        existingResource = new 
        {
            id = ex.ResourceId,
            codigo = ex.ResourceCode  // Puede ser null
        }
    });
}
```

### HTTP Status Code
- **409 Conflict** para recursos duplicados (NO 500 Internal Server Error)
- **201 Created** para creación exitosa
- **400 Bad Request** para validación de entrada

### Ejemplo de Respuesta HTTP 409
```json
{
  "message": "Ya existe un beneficiario con el nombre 'Juan Pérez'",
  "resourceType": "Beneficiario",
  "conflictingField": "nombre",
  "existingResource": {
    "id": 12,
    "codigo": "BENE-000012"
  }
}
```

**⚠️ Servicios afectados**: `BeneficiarioService`, `DonadorService`, `ArticuloService`

---

## 🗄️ PERSISTENCIA Y BASE DE DATOS

### Ejecución de Stored Procedures

```csharp
// 1. Definir parámetros
var parameters = new[]
{
    new SqlParameter("@parametro1", valor1 ?? (object)DBNull.Value),
    new SqlParameter("@parametro2", valor2),
    new SqlParameter("@parametroInt", intValue)
};

// 2. Ejecutar SP
var resultado = await _context.ResultadoDbSet
    .FromSqlRaw("EXEC NombreSP @parametro1, @parametro2, @parametroInt", parameters)
    .AsNoTracking()
    .ToListAsync();

// 3. Procesar resultado
if (resultado == null || resultado.Count == 0)
{
    return new Response { exito = false, mensaje = "No se recibió respuesta" };
}

var item = resultado.FirstOrDefault();
```

### Configuración de DbContext

```csharp
// En ApplicationDbContext.cs

// DbSets para entidades normales
public DbSet<Entidad> Entidades { get; set; }

// DbSets para resultados de SPs (sin clave)
public DbSet<ResultadoSP> ResultadosSP { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configurar PK
    modelBuilder.Entity<Entidad>()
        .HasKey(e => e.id_entidad);

    // Resultados de SP sin clave
    modelBuilder.Entity<ResultadoSP>()
        .HasNoKey();

    // Relaciones FK
    modelBuilder.Entity<Detalle>()
        .HasOne(d => d.Maestro)
        .WithMany()
        .HasForeignKey(d => d.id_maestro)
        .OnDelete(DeleteBehavior.Restrict);
}
```

### Interceptores de Auditoría

**El proyecto usa `AuditInterceptor` para gestionar automáticamente:**
- `fecha_creacion` (solo en INSERT)
- `fecha_actualizacion` (en INSERT y UPDATE)
- Soft Delete (convierte DELETE en UPDATE con `activo = false`)

**NO es necesario asignar manualmente estas propiedades en los servicios.**

---

## 📦 DTOs (Data Transfer Objects)

### Estructura Estándar

```csharp
namespace apiDonativos.Models.Dtos
{
    // Request (entrada)
    public class [Operacion]Request
    {
        public string campo1 { get; set; }
        public int campo2 { get; set; }
        // Usar [Required] si es necesario validación
    }

    // Response (salida)
    public class [Operacion]Response
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public int? id { get; set; }  // Opcional, solo si aplica
    }
}
```

**⚠️ Las propiedades de DTOs usan `camelCase` para coincidir con la base de datos.**

---

## 🔒 SEGURIDAD

### Authorization
- `[AllowAnonymous]` → Endpoints públicos (registro, login)
- `[Authorize]` → Endpoints protegidos (CRUD, operaciones)

### Validación de Entrada
```csharp
// 1. Null check
if (request == null)
    return BadRequest(new { mensaje = "Datos no proporcionados" });

// 2. ModelState (Data Annotations)
if (!ModelState.IsValid)
    return BadRequest(ModelState);

// 3. Validaciones de negocio en Service
if (string.IsNullOrWhiteSpace(request.usuario))
    throw new ArgumentException("Usuario es requerido");
```

### Protección de Datos Sensibles
- **Contraseñas**: Nunca loggear, nunca retornar
- **Tokens**: Solo en cookies HttpOnly o headers seguros
- **PII**: Minimizar en logs (usar ID en lugar de nombres completos)

---

## 📁 TEMPLATE DE NUEVOS ENDPOINTS

### 1. Crear DTO
```csharp
// Models/Dtos/[Entidad][Operacion].cs
namespace apiDonativos.Models.Dtos
{
    public class CrearArticuloRequest
    {
        public string nombre { get; set; }
        public int id_categoria { get; set; }
    }

    public class CrearArticuloResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public int? id_articulo { get; set; }
    }
}
```

### 2. Crear Servicio
```csharp
// Services/[Entidad]Service.cs
namespace apiDonativos.Services
{
    public interface IArticulo
    {
        Task<CrearArticuloResponse> CrearAsync(CrearArticuloRequest request);
    }

    public class ArticuloService : IArticulo
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArticuloService> _logger;

        public ArticuloService(ApplicationDbContext context, ILogger<ArticuloService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CrearArticuloResponse> CrearAsync(CrearArticuloRequest request)
        {
            _logger.LogInformation("Creando artículo: {Nombre}", request.nombre);

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@nombre", request.nombre),
                    new SqlParameter("@categoriaId", request.id_categoria)
                };

                var resultado = await _context.ResultadosSP
                    .FromSqlRaw("EXEC CrearArticulo @nombre, @categoriaId", parameters)
                    .AsNoTracking()
                    .ToListAsync();

                var item = resultado.FirstOrDefault();

                return new CrearArticuloResponse
                {
                    exito = item.Success,
                    mensaje = item.Message,
                    id_articulo = item.ID
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear artículo");
                throw;
            }
        }
    }
}
```

### 3. Crear Controlador
```csharp
// Controllers/ArticuloController.cs
using apiDonativos.Models.Dtos;
using apiDonativos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace apiDonativos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArticuloController : ControllerBase
    {
        private readonly IArticulo _articuloService;
        private readonly ILogger<ArticuloController> _logger;

        public ArticuloController(IArticulo articuloService, ILogger<ArticuloController> logger)
        {
            _articuloService = articuloService;
            _logger = logger;
        }

        /// <summary>
        /// Crea un nuevo artículo
        /// POST: articulo/crear
        /// </summary>
        [HttpPost("crear")]
        [Authorize]
        public async Task<ActionResult<CrearArticuloResponse>> Crear([FromBody] CrearArticuloRequest request)
        {
            if (request == null)
                return BadRequest(new { mensaje = "Datos no proporcionados" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _articuloService.CrearAsync(request);

                if (!resultado.exito)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                var referenceId = Guid.NewGuid().ToString();
                _logger.LogError(ex, "Error al crear artículo. ReferenceId: {ReferenceId}", referenceId);

                #if DEBUG
                    return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
                #else
                    return StatusCode(500, new { mensaje = "Error al procesar la solicitud", reference_id = referenceId });
                #endif
            }
        }
    }
}
```

### 4. Registrar en Startup.cs
```csharp
// Startup.cs → ConfigureServices
services.AddScoped<IArticulo, ArticuloService>();
```

---

## 🧪 TESTING

### Estructura de Pruebas (Recomendado)
```
Tests/
├─ Unit/
│  ├─ Services/
│  │  └─ UsuarioServiceTests.cs
│  └─ Controllers/
│     └─ UsuarioControllerTests.cs
└─ Integration/
   └─ UsuarioEndpointTests.cs
```

### Nombrado de Pruebas
```csharp
[Fact]
public async Task MetodoAProbar_Escenario_ResultadoEsperado()
{
    // Arrange
    // Act
    // Assert
}
```

---

## 📚 DOCUMENTACIÓN REQUERIDA

### XML Comments en Controllers
```csharp
/// <summary>
/// Descripción breve de la operación
/// POST: controlador/accion
/// </summary>
/// <param name="request">Descripción del parámetro</param>
/// <returns>Descripción del retorno</returns>
[HttpPost("accion")]
public async Task<ActionResult<Response>> Accion([FromBody] Request request)
```

---

## 🚀 DEPLOYMENT

### Configuración de Logs
- **Desarrollo**: `logs/api-YYYYMMDD.txt` (relativo al ejecutable)
- **Producción**: `D:\Logs\ApiDonativos\api-YYYYMMDD.txt` (centralizado)
- **Retención**: 30 días
- **Tamaño máximo**: 10 MB por archivo

### Variables de Entorno
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## ✅ CHECKLIST PARA NUEVAS FUNCIONALIDADES

- [ ] DTO creado con `camelCase` en propiedades
- [ ] Interface de servicio declarada (`IEntidad`)
- [ ] Servicio implementado con logging
- [ ] Controlador con manejo de errores completo
- [ ] Inyección de dependencias registrada en `Startup.cs`
- [ ] Stored Procedure creado (si aplica)
- [ ] DbSet agregado a `ApplicationDbContext` (si aplica)
- [ ] XML comments en endpoints públicos
- [ ] Authorization configurada (`[Authorize]` o `[AllowAnonymous]`)
- [ ] No se registran datos sensibles en logs
- [ ] Validación de entrada implementada
- [ ] Reference ID en logs de error

---

## 🔄 CONTROL DE VERSIONES

### Branches
- `main` → Producción
- `dev` → Desarrollo activo
- `feature/[nombre]` → Nuevas funcionalidades
- `hotfix/[nombre]` → Correcciones urgentes

### Commits
Formato: `[TIPO] Descripción breve`

Tipos:
- `[FEAT]` → Nueva funcionalidad
- `[FIX]` → Corrección de bug
- `[REFACTOR]` → Refactorización
- `[DOCS]` → Documentación
- `[STYLE]` → Formato de código
- `[TEST]` → Pruebas

Ejemplo: `[FEAT] Agregar endpoint de registro de usuarios`

---

## 🛠️ HERRAMIENTAS Y EXTENSIONES RECOMENDADAS

### Visual Studio
- **C# Dev Kit**
- **GitHub Copilot** (agente de código)
- **SonarLint** (análisis de calidad)
- **Entity Framework Power Tools**

### NuGet Packages Core
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Serilog.AspNetCore`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File`
- `Microsoft.AspNetCore.Authentication.JwtBearer` (si aplica)

---

## 🎯 SKILLS RECOMENDADAS PARA AGENTES DE IA

Al interactuar con este proyecto, los agentes de IA deben:

1. **Seguir estrictamente las convenciones de nomenclatura** (PascalCase/camelCase según contexto)
2. **Implementar el patrón completo de error handling** con Reference ID
3. **Incluir logging estructurado** en cada operación
4. **Validar entrada** en tres niveles (null, ModelState, negocio)
5. **Usar inyección de dependencias** en todos los componentes
6. **Preservar la estructura de carpetas** existente
7. **No exponer datos sensibles** en logs o respuestas
8. **Generar código completo** sin placeholders (`// código existente...`)
9. **Mantener consistencia** con el código existente
10. **Documentar con XML comments** los endpoints públicos

---

**Versión**: 1.0  
**Última actualización**: Marzo 2026  
**Mantenedor**: Equipo de Desarrollo - API Donativos
