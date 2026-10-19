# Skills Recomendadas para Agentes de IA
## Proyecto: API Donativos

---

## 🎯 Skills Fundamentales

### 1. **Pattern Recognition** (Crítico)
**Capacidad**: Identificar y replicar patrones arquitectónicos existentes

**Aplicación**:
- Detectar estructura Controller → Service → DbContext
- Replicar patrones de error handling con Reference ID
- Mantener convenciones de nomenclatura (camelCase en DTOs, PascalCase en código)
- Seguir estructura de logging estructurado

**Ejemplo**:
```csharp
// El agente debe detectar que SIEMPRE se usa este patrón:
var referenceId = Guid.NewGuid().ToString();
_logger.LogError(ex, "Error en {Operacion}. ReferenceId: {ReferenceId}", operacion, referenceId);
```

---

### 2. **Code Completion** (Crítico)
**Capacidad**: Generar código completo sin placeholders

**Aplicación**:
- NO usar comentarios como `// código existente aquí...`
- Preservar código circundante completo
- Generar implementaciones completas (no esqueletos)

**Ejemplo Incorrecto**:
```csharp
// ❌ EVITAR
public async Task<Response> Metodo()
{
    // TODO: Implementar lógica
    // código existente...
}
```

**Ejemplo Correcto**:
```csharp
// ✅ CORRECTO
public async Task<Response> Metodo()
{
    if (request == null)
        return BadRequest(new { mensaje = "Datos no proporcionados" });

    try
    {
        var resultado = await _servicio.EjecutarAsync(request);
        return Ok(resultado);
    }
    catch (Exception ex)
    {
        var referenceId = Guid.NewGuid().ToString();
        _logger.LogError(ex, "Error. ReferenceId: {ReferenceId}", referenceId);
        return StatusCode(500, new { mensaje = "Error", reference_id = referenceId });
    }
}
```

---

### 3. **Contextual Naming** (Alto)
**Capacidad**: Usar nomenclatura correcta según el contexto

**Reglas**:
| Contexto | Convención | Ejemplo |
|----------|-----------|---------|
| Clases C# | PascalCase | `UsuarioService` |
| Métodos C# | PascalCase | `RegistrarAsync()` |
| Propiedades DTO | camelCase | `usuario`, `centroID` |
| Parámetros SP | @camelCase | `@usuario`, `@password` |
| Tablas SQL | snake_case | `donativos_detalle` |
| Campos privados | _camelCase | `_logger`, `_context` |

---

### 4. **Dependency Injection Awareness** (Crítico)
**Capacidad**: Implementar y registrar DI correctamente

**Checklist**:
- [ ] Declarar dependencias en constructor
- [ ] Almacenar en campos privados readonly
- [ ] Registrar en `Startup.cs`
- [ ] Usar interfaces para servicios

**Template**:
```csharp
// 1. Declarar interface
public interface IMiServicio
{
    Task<Response> MetodoAsync(Request request);
}

// 2. Implementar
public class MiServicio : IMiServicio
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MiServicio> _logger;

    public MiServicio(ApplicationDbContext context, ILogger<MiServicio> logger)
    {
        _context = context;
        _logger = logger;
    }
}

// 3. Registrar en Startup.cs
services.AddScoped<IMiServicio, MiServicio>();
```

---

### 5. **Structured Logging** (Alto)
**Capacidad**: Generar logs estructurados con contexto

**Niveles**:
- **Information**: Inicio y fin de operaciones
- **Warning**: Errores de negocio esperados
- **Error**: Excepciones inesperadas

**Template**:
```csharp
// Inicio (sin datos sensibles)
_logger.LogInformation(
    "Iniciando {Operacion}. Usuario: {Username}, ID: {Id}",
    "Registro",
    request.usuario,
    request.id
);

// Éxito
_logger.LogInformation(
    "{Operacion} exitosa. ID: {Id}",
    "Registro",
    resultado.id
);

// Advertencia
_logger.LogWarning(
    "{Operacion} fallida. Razón: {Razon}",
    "Registro",
    resultado.mensaje
);

// Error
_logger.LogError(
    ex,
    "Error en {Operacion}. Usuario: {Username}",
    "Registro",
    request.usuario
);
```

---

### 6. **Security Awareness** (Crítico)
**Capacidad**: Proteger datos sensibles

**Reglas de Oro**:
1. **NUNCA** loggear contraseñas
2. **NUNCA** retornar contraseñas en responses
3. **SIEMPRE** validar entrada en 3 niveles
4. Usar `#if DEBUG` para mensajes detallados de error
5. Incluir Reference ID en errores de producción

**Validación de 3 Niveles**:
```csharp
// 1. Null check
if (request == null)
    return BadRequest(new { mensaje = "Datos no proporcionados" });

// 2. ModelState
if (!ModelState.IsValid)
    return BadRequest(ModelState);

// 3. Lógica de negocio (en Service)
if (string.IsNullOrWhiteSpace(request.usuario))
    throw new ArgumentException("Usuario requerido");
```

---

### 7. **EF Core Stored Procedure Execution** (Alto)
**Capacidad**: Ejecutar SPs correctamente

**Pattern**:
```csharp
// 1. Parámetros (manejar nulls)
var parameters = new[]
{
    new SqlParameter("@parametro1", valor1 ?? (object)DBNull.Value),
    new SqlParameter("@parametro2", valor2)
};

// 2. Ejecución
var resultado = await _context.ResultadoDbSet
    .FromSqlRaw("EXEC NombreSP @parametro1, @parametro2", parameters)
    .AsNoTracking()
    .ToListAsync();

// 3. Validación
if (resultado == null || resultado.Count == 0)
{
    return new Response { exito = false, mensaje = "Sin respuesta" };
}

var item = resultado.FirstOrDefault();
```

---

### 8. **Error Handling with Reference ID** (Crítico)
**Capacidad**: Implementar manejo de errores trazable

**Pattern Completo**:
```csharp
try
{
    var resultado = await _servicio.MetodoAsync(request);

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
    _logger.LogError(ex, "Error en {Operacion}. ReferenceId: {ReferenceId}", "Operacion", referenceId);

    #if DEBUG
        return StatusCode(500, new { mensaje = ex.Message, reference_id = referenceId });
    #else
        return StatusCode(500, new { mensaje = "Error al procesar la solicitud", reference_id = referenceId });
    #endif
}
```

---

### 9. **DTO Design** (Alto)
**Capacidad**: Diseñar DTOs consistentes

**Estructura Estándar**:
```csharp
namespace apiDonativos.Models.Dtos
{
    // Request
    public class [Operacion]Request
    {
        public string campo1 { get; set; }  // camelCase
        public int campo2 { get; set; }
    }

    // Response
    public class [Operacion]Response
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public int? id { get; set; }  // Opcional
    }
}
```

---

### 10. **Code Documentation** (Medio)
**Capacidad**: Documentar endpoints con XML comments

**Template**:
```csharp
/// <summary>
/// [Descripción de la operación]
/// POST: [controlador]/[accion]
/// </summary>
/// <param name="request">Datos de la solicitud</param>
/// <returns>Resultado de la operación</returns>
[HttpPost("accion")]
public async Task<ActionResult<Response>> Accion([FromBody] Request request)
```

---

## 📊 Matriz de Prioridad

| Skill | Prioridad | Impacto | Frecuencia |
|-------|-----------|---------|------------|
| Pattern Recognition | ⭐⭐⭐⭐⭐ | Alto | Constante |
| Code Completion | ⭐⭐⭐⭐⭐ | Alto | Constante |
| DI Awareness | ⭐⭐⭐⭐⭐ | Alto | Constante |
| Error Handling | ⭐⭐⭐⭐⭐ | Crítico | Constante |
| Security Awareness | ⭐⭐⭐⭐⭐ | Crítico | Constante |
| Structured Logging | ⭐⭐⭐⭐ | Alto | Frecuente |
| Contextual Naming | ⭐⭐⭐⭐ | Alto | Constante |
| EF Core SP Execution | ⭐⭐⭐⭐ | Alto | Frecuente |
| DTO Design | ⭐⭐⭐ | Medio | Frecuente |
| Documentation | ⭐⭐⭐ | Medio | Moderado |

---

## 🚨 Anti-Patterns a Evitar

### ❌ NO hacer:
```csharp
// 1. Placeholder comments
public void Metodo()
{
    // TODO: Implementar
    // código existente aquí...
}

// 2. Inconsistencia en naming
public string Usuario { get; set; }  // En DTO debería ser 'usuario'

// 3. Logging de contraseñas
_logger.LogInformation("Usuario: {User}, Password: {Pass}", user, pass);

// 4. Sin validación
public async Task<Response> Metodo(Request request)
{
    return await _servicio.EjecutarAsync(request);
}

// 5. Errores sin Reference ID
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    return StatusCode(500, "Error");
}
```

### ✅ SÍ hacer:
```csharp
// 1. Código completo
public async Task<Response> Metodo(Request request)
{
    if (request == null)
        return BadRequest(new { mensaje = "Datos no proporcionados" });

    // Implementación completa...
}

// 2. Naming consistente
public string usuario { get; set; }  // camelCase en DTOs

// 3. Logging seguro
_logger.LogInformation("Usuario: {Username}", request.usuario);

// 4. Validación completa
if (request == null)
    return BadRequest();
if (!ModelState.IsValid)
    return BadRequest(ModelState);

// 5. Errores con Reference ID
var refId = Guid.NewGuid().ToString();
_logger.LogError(ex, "Error. RefId: {RefId}", refId);
return StatusCode(500, new { reference_id = refId });
```

---

## 🎓 Entrenamiento Sugerido

### Fase 1: Reconocimiento (1-2 días)
- Analizar 5 controladores existentes
- Identificar patrones comunes
- Documentar convenciones

### Fase 2: Replicación (3-5 días)
- Crear 3 endpoints nuevos siguiendo el template
- Validar con checklist de especificación
- Recibir feedback

### Fase 3: Maestría (1 semana)
- Refactorizar código antiguo a nuevos estándares
- Optimizar logs y error handling
- Documentar mejoras

---

**Nota**: Este documento debe actualizarse conforme evoluciona el proyecto.

**Versión**: 1.0  
**Fecha**: Marzo 2026
