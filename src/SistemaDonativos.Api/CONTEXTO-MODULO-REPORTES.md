# 📋 Archivo de Contexto - Módulo de Reportes

> **Fecha de Creación:** 2025-01-XX  
> **Versión:** 1.0  
> **Proyecto:** Sistema de Donativos - Módulo de Reportes  
> **Repositorio:** sistema-donativos  
> **Rama:** main  

---

## 🎯 Resumen Ejecutivo

**Objetivo:** Implementar 5 reportes dinámicos con exportación a Excel usando EPPlus y consultas directas a Stored Procedures de SQL Server.

**Arquitectura Elegida:** API-First con endpoints REST que retornan archivos Excel generados dinámicamente.

**Stack Tecnológico:**
- **.NET 10** (API separada del proyecto Razor Pages)
- **EPPlus 7.5.2** (Generación de Excel)
- **ADO.NET** (Ejecución de SPs con múltiples resultsets)
- **JavaScript/jQuery** (Frontend consumidor de API)

---

## 📊 Stored Procedures y Parámetros

### **1. sp_Reporte_Entradas**

**Descripción:** Reporte de todas las entradas (donaciones y compras) al inventario con filtros dinámicos.

**Parámetros:**
```sql
@fecha_inicio DATETIME = NULL
@fecha_fin DATETIME = NULL
@id_almacen INT = NULL
@id_donador INT = NULL
@id_articulo INT = NULL
@id_categoria INT = NULL
@tipo NVARCHAR(50) = NULL
@estatus NVARCHAR(50) = NULL
@agrupar_por NVARCHAR(50) = NULL
@pagina INT = 1
@registros_por_pagina INT = 50
@ordenar_por NVARCHAR(50) = 'fecha_recepcion'
@orden NVARCHAR(4) = 'DESC'
```

**Retorna:**
- **ResultSet 1:** Resumen/Totales (agregaciones, métricas generales)
- **ResultSet 2:** Detalle paginado (listado completo de entradas)

---

### **2. sp_Reporte_Salidas**

**Descripción:** Reporte de todas las salidas (donaciones entregadas y mermas) del inventario.

**Parámetros:**
```sql
@fecha_inicio DATETIME = NULL
@fecha_fin DATETIME = NULL
@id_almacen INT = NULL
@id_beneficiario INT = NULL
@id_articulo INT = NULL
@id_categoria INT = NULL
@tipo_salida NVARCHAR(50) = NULL
@estatus NVARCHAR(50) = NULL
@agrupar_por NVARCHAR(50) = NULL
@pagina INT = 1
@registros_por_pagina INT = 50
```

**Retorna:**
- **ResultSet 1:** Resumen/Totales
- **ResultSet 2:** Detalle paginado

---

### **3. sp_Reporte_Traspasos**

**Descripción:** Reporte de traspasos entre almacenes.

**Parámetros:**
```sql
@fecha_inicio DATETIME = NULL
@fecha_fin DATETIME = NULL
@id_almacen_origen INT = NULL
@id_almacen_destino INT = NULL
@id_articulo INT = NULL
@id_categoria INT = NULL
@estatus NVARCHAR(50) = NULL
@agrupar_por NVARCHAR(50) = NULL
@pagina INT = 1
@registros_por_pagina INT = 50
```

**Retorna:**
- **ResultSet 1:** Resumen/Totales
- **ResultSet 2:** Detalle paginado

---

### **4. sp_Reporte_Existencias**

**Descripción:** Reporte de existencias actuales por almacén con filtros de stock mínimo/máximo.

**Parámetros:**
```sql
@id_almacen INT = NULL
@id_articulo INT = NULL
@id_categoria INT = NULL
@cantidad_minima DECIMAL(18, 2) = NULL
@cantidad_maxima DECIMAL(18, 2) = NULL
@solo_sin_existencia BIT = 0
@agrupar_por NVARCHAR(50) = NULL
@ordenar_por NVARCHAR(50) = 'nombre_articulo'
@orden NVARCHAR(4) = 'ASC'
@fecha_corte DATETIME = NULL
@pagina INT = 1
@registros_por_pagina INT = 100
```

**Retorna:**
- **ResultSet 1:** Resumen/Totales
- **ResultSet 2:** Detalle paginado

---

### **5. sp_Kardex_Inventario**

**Descripción:** Kardex de movimientos de un artículo específico en un almacén.

**Parámetros:**
```sql
@id_almacen INT (REQUERIDO)
@id_articulo INT (REQUERIDO)
@fecha_inicio DATETIME = NULL
@fecha_fin DATETIME = NULL
```

**Retorna:**
- **ResultSet 1:** Información del artículo (datos maestros)
- **ResultSet 2:** Movimientos detallados (entradas, salidas, traspasos)

---

## 🏗️ Estructura de Archivos Propuesta (API)

```
API/                                          // Proyecto API separado
├── Controllers/
│   └── ReportesController.cs                 // 5 endpoints POST
│
├── Services/
│   ├── Interfaces/
│   │   ├── IReportesService.cs
│   │   └── IExcelExportService.cs
│   │
│   └── Implementation/
│       ├── ReportesService.cs                // Ejecución de SPs (ADO.NET)
│       └── ExcelExportService.cs             // Generación Excel (EPPlus)
│
├── DTOs/
│   ├── Reportes/
│   │   ├── ReporteEntradasDto.cs
│   │   ├── ReporteSalidasDto.cs
│   │   ├── ReporteTraspasosDto.cs
│   │   ├── ReporteExistenciasDto.cs
│   │   └── ReporteKardexDto.cs
│   │
│   └── Common/
│       └── ReporteResultDto.cs               // Wrapper para DataSet
│
└── Configuration/
    └── DependencyInjection.cs                // Registro de servicios
```

---

## 📦 Dependencias NuGet Necesarias

```xml
<!-- API.csproj -->
<ItemGroup>
  <!-- Generación de Excel -->
  <PackageReference Include="EPPlus" Version="7.5.2" />
  
  <!-- Acceso a datos SQL Server -->
  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
  
  <!-- Librerías base de .NET 10 (ya incluidas) -->
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
</ItemGroup>
```

**Configuración de licencia EPPlus:**
```csharp
// En el constructor de ExcelExportService
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```

---

## 🔧 DTOs Completos

### **ReporteEntradasDto.cs**
```csharp
namespace API.DTOs.Reportes;

/// <summary>
/// Parámetros para el reporte de entradas al inventario
/// </summary>
public class ReporteEntradasDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? IdAlmacen { get; set; }
    public int? IdDonador { get; set; }
    public int? IdArticulo { get; set; }
    public int? IdCategoria { get; set; }
    public string? Tipo { get; set; }              // "donacion", "compra", etc.
    public string? Estatus { get; set; }            // "registrada", "recibida", etc.
    public string? AgruparPor { get; set; }         // "almacen", "categoria", "donador", etc.
    public int Pagina { get; set; } = 1;
    public int RegistrosPorPagina { get; set; } = 50;
    public string OrdenarPor { get; set; } = "fecha_recepcion";
    public string Orden { get; set; } = "DESC";     // "ASC" o "DESC"
}
```

### **ReporteSalidasDto.cs**
```csharp
namespace API.DTOs.Reportes;

/// <summary>
/// Parámetros para el reporte de salidas del inventario
/// </summary>
public class ReporteSalidasDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? IdAlmacen { get; set; }
    public int? IdBeneficiario { get; set; }
    public int? IdArticulo { get; set; }
    public int? IdCategoria { get; set; }
    public string? TipoSalida { get; set; }         // "donacion", "merma"
    public string? Estatus { get; set; }            // "registrada", "entregada"
    public string? AgruparPor { get; set; }         // "almacen", "categoria", "beneficiario"
    public int Pagina { get; set; } = 1;
    public int RegistrosPorPagina { get; set; } = 50;
}
```

### **ReporteTraspasosDto.cs**
```csharp
namespace API.DTOs.Reportes;

/// <summary>
/// Parámetros para el reporte de traspasos entre almacenes
/// </summary>
public class ReporteTraspasosDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? IdAlmacenOrigen { get; set; }
    public int? IdAlmacenDestino { get; set; }
    public int? IdArticulo { get; set; }
    public int? IdCategoria { get; set; }
    public string? Estatus { get; set; }            // "registrada", "enviada", "recibida"
    public string? AgruparPor { get; set; }         // "almacen_origen", "almacen_destino", "categoria"
    public int Pagina { get; set; } = 1;
    public int RegistrosPorPagina { get; set; } = 50;
}
```

### **ReporteExistenciasDto.cs**
```csharp
namespace API.DTOs.Reportes;

/// <summary>
/// Parámetros para el reporte de existencias actuales
/// </summary>
public class ReporteExistenciasDto
{
    public int? IdAlmacen { get; set; }
    public int? IdArticulo { get; set; }
    public int? IdCategoria { get; set; }
    public decimal? CantidadMinima { get; set; }
    public decimal? CantidadMaxima { get; set; }
    public bool SoloSinExistencia { get; set; } = false;  // Filtrar artículos con stock = 0
    public string? AgruparPor { get; set; }               // "almacen", "categoria"
    public string OrdenarPor { get; set; } = "nombre_articulo";
    public string Orden { get; set; } = "ASC";
    public DateTime? FechaCorte { get; set; }             // Para ver existencias a una fecha específica
    public int Pagina { get; set; } = 1;
    public int RegistrosPorPagina { get; set; } = 100;
}
```

### **ReporteKardexDto.cs**
```csharp
namespace API.DTOs.Reportes;

/// <summary>
/// Parámetros para el reporte Kardex de un artículo
/// </summary>
public class ReporteKardexDto
{
    public int IdAlmacen { get; set; }          // REQUERIDO
    public int IdArticulo { get; set; }         // REQUERIDO
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}
```

---

## 🎯 Endpoints del Controller

### **ReportesController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Reportes;
using API.Services.Interfaces;

namespace API.Controllers;

/// <summary>
/// Controller para generación de reportes Excel del sistema de inventario
/// </summary>
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;
    private readonly IExcelExportService _excelService;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(
        IReportesService reportesService,
        IExcelExportService excelService,
        ILogger<ReportesController> logger)
    {
        _reportesService = reportesService;
        _excelService = excelService;
        _logger = logger;
    }

    /// <summary>
    /// Genera reporte de entradas al inventario en formato Excel
    /// </summary>
    [HttpPost("entradas/generar")]
    public async Task<IActionResult> GenerarReporteEntradas(
        [FromBody] ReporteEntradasDto parametros)
    {
        try
        {
            _logger.LogInformation("Generando reporte de entradas con filtros: {@Parametros}", parametros);

            // 1. Ejecutar SP y obtener DataSet
            var dataSet = await _reportesService.EjecutarReporteEntradas(parametros);

            // 2. Generar archivo Excel
            var excelBytes = _excelService.GenerarExcelEntradas(dataSet, parametros);

            // 3. Retornar archivo
            var nombreArchivo = $"Reporte_Entradas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de entradas");
            return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
        }
    }

    /// <summary>
    /// Genera reporte de salidas del inventario en formato Excel
    /// </summary>
    [HttpPost("salidas/generar")]
    public async Task<IActionResult> GenerarReporteSalidas(
        [FromBody] ReporteSalidasDto parametros)
    {
        try
        {
            _logger.LogInformation("Generando reporte de salidas");

            var dataSet = await _reportesService.EjecutarReporteSalidas(parametros);
            var excelBytes = _excelService.GenerarExcelSalidas(dataSet, parametros);
            var nombreArchivo = $"Reporte_Salidas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de salidas");
            return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
        }
    }

    /// <summary>
    /// Genera reporte de traspasos entre almacenes en formato Excel
    /// </summary>
    [HttpPost("traspasos/generar")]
    public async Task<IActionResult> GenerarReporteTraspasos(
        [FromBody] ReporteTraspasosDto parametros)
    {
        try
        {
            _logger.LogInformation("Generando reporte de traspasos");

            var dataSet = await _reportesService.EjecutarReporteTraspasos(parametros);
            var excelBytes = _excelService.GenerarExcelTraspasos(dataSet, parametros);
            var nombreArchivo = $"Reporte_Traspasos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de traspasos");
            return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
        }
    }

    /// <summary>
    /// Genera reporte de existencias actuales en formato Excel
    /// </summary>
    [HttpPost("existencias/generar")]
    public async Task<IActionResult> GenerarReporteExistencias(
        [FromBody] ReporteExistenciasDto parametros)
    {
        try
        {
            _logger.LogInformation("Generando reporte de existencias");

            var dataSet = await _reportesService.EjecutarReporteExistencias(parametros);
            var excelBytes = _excelService.GenerarExcelExistencias(dataSet, parametros);
            var nombreArchivo = $"Reporte_Existencias_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de existencias");
            return StatusCode(500, new { mensaje = "Error al generar el reporte", error = ex.Message });
        }
    }

    /// <summary>
    /// Genera reporte Kardex de movimientos de un artículo en formato Excel
    /// </summary>
    [HttpPost("kardex/generar")]
    public async Task<IActionResult> GenerarReporteKardex(
        [FromBody] ReporteKardexDto parametros)
    {
        try
        {
            // Validar parámetros requeridos
            if (parametros.IdAlmacen == 0 || parametros.IdArticulo == 0)
            {
                return BadRequest(new { mensaje = "IdAlmacen e IdArticulo son requeridos" });
            }

            _logger.LogInformation("Generando kardex para artículo {IdArticulo} en almacén {IdAlmacen}", 
                parametros.IdArticulo, parametros.IdAlmacen);

            var dataSet = await _reportesService.EjecutarReporteKardex(parametros);
            var excelBytes = _excelService.GenerarExcelKardex(dataSet, parametros);
            var nombreArchivo = $"Kardex_Art{parametros.IdArticulo}_Alm{parametros.IdAlmacen}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar kardex");
            return StatusCode(500, new { mensaje = "Error al generar el kardex", error = ex.Message });
        }
    }
}
```

---

## 🔨 Interfaces de Servicios

### **IReportesService.cs**
```csharp
using System.Data;
using API.DTOs.Reportes;

namespace API.Services.Interfaces;

/// <summary>
/// Servicio para ejecución de Stored Procedures de reportes
/// </summary>
public interface IReportesService
{
    /// <summary>
    /// Ejecuta el SP de reporte de entradas y retorna un DataSet con 2 tablas (resumen + detalle)
    /// </summary>
    Task<DataSet> EjecutarReporteEntradas(ReporteEntradasDto parametros);

    /// <summary>
    /// Ejecuta el SP de reporte de salidas y retorna un DataSet con 2 tablas
    /// </summary>
    Task<DataSet> EjecutarReporteSalidas(ReporteSalidasDto parametros);

    /// <summary>
    /// Ejecuta el SP de reporte de traspasos y retorna un DataSet con 2 tablas
    /// </summary>
    Task<DataSet> EjecutarReporteTraspasos(ReporteTraspasosDto parametros);

    /// <summary>
    /// Ejecuta el SP de reporte de existencias y retorna un DataSet con 2 tablas
    /// </summary>
    Task<DataSet> EjecutarReporteExistencias(ReporteExistenciasDto parametros);

    /// <summary>
    /// Ejecuta el SP de kardex y retorna un DataSet con 2 tablas (info artículo + movimientos)
    /// </summary>
    Task<DataSet> EjecutarReporteKardex(ReporteKardexDto parametros);
}
```

### **IExcelExportService.cs**
```csharp
using System.Data;
using API.DTOs.Reportes;

namespace API.Services.Interfaces;

/// <summary>
/// Servicio para generación de archivos Excel con EPPlus
/// </summary>
public interface IExcelExportService
{
    /// <summary>
    /// Genera un archivo Excel del reporte de entradas
    /// </summary>
    byte[] GenerarExcelEntradas(DataSet datos, ReporteEntradasDto parametros);

    /// <summary>
    /// Genera un archivo Excel del reporte de salidas
    /// </summary>
    byte[] GenerarExcelSalidas(DataSet datos, ReporteSalidasDto parametros);

    /// <summary>
    /// Genera un archivo Excel del reporte de traspasos
    /// </summary>
    byte[] GenerarExcelTraspasos(DataSet datos, ReporteTraspasosDto parametros);

    /// <summary>
    /// Genera un archivo Excel del reporte de existencias
    /// </summary>
    byte[] GenerarExcelExistencias(DataSet datos, ReporteExistenciasDto parametros);

    /// <summary>
    /// Genera un archivo Excel del reporte kardex
    /// </summary>
    byte[] GenerarExcelKardex(DataSet datos, ReporteKardexDto parametros);
}
```

---

## 💡 Implementación Base de ReportesService

### **ReportesService.cs**
```csharp
using System.Data;
using Microsoft.Data.SqlClient;
using API.DTOs.Reportes;
using API.Services.Interfaces;

namespace API.Services.Implementation;

/// <summary>
/// Servicio que ejecuta Stored Procedures de reportes usando ADO.NET
/// </summary>
public class ReportesService : IReportesService
{
    private readonly string _connectionString;
    private readonly ILogger<ReportesService> _logger;

    public ReportesService(
        IConfiguration configuration, 
        ILogger<ReportesService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' no encontrada");
        _logger = logger;
    }

    public async Task<DataSet> EjecutarReporteEntradas(ReporteEntradasDto parametros)
    {
        var dataSet = new DataSet();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("sp_Reporte_Entradas", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 120; // 2 minutos para reportes grandes

                // Agregar parámetros con valores null seguros
                command.Parameters.AddWithValue("@fecha_inicio", parametros.FechaInicio ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@fecha_fin", parametros.FechaFin ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_almacen", parametros.IdAlmacen ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_donador", parametros.IdDonador ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_articulo", parametros.IdArticulo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_categoria", parametros.IdCategoria ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@tipo", parametros.Tipo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@estatus", parametros.Estatus ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@agrupar_por", parametros.AgruparPor ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@pagina", parametros.Pagina);
                command.Parameters.AddWithValue("@registros_por_pagina", parametros.RegistrosPorPagina);
                command.Parameters.AddWithValue("@ordenar_por", parametros.OrdenarPor);
                command.Parameters.AddWithValue("@orden", parametros.Orden);

                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }
            }
        }

        _logger.LogInformation(
            "Reporte Entradas ejecutado: {TablesCount} tabla(s) obtenida(s), Tabla 1: {Rows1} filas, Tabla 2: {Rows2} filas", 
            dataSet.Tables.Count,
            dataSet.Tables.Count > 0 ? dataSet.Tables[0].Rows.Count : 0,
            dataSet.Tables.Count > 1 ? dataSet.Tables[1].Rows.Count : 0
        );

        return dataSet;
    }

    public async Task<DataSet> EjecutarReporteSalidas(ReporteSalidasDto parametros)
    {
        var dataSet = new DataSet();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("sp_Reporte_Salidas", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 120;

                command.Parameters.AddWithValue("@fecha_inicio", parametros.FechaInicio ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@fecha_fin", parametros.FechaFin ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_almacen", parametros.IdAlmacen ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_beneficiario", parametros.IdBeneficiario ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_articulo", parametros.IdArticulo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_categoria", parametros.IdCategoria ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@tipo_salida", parametros.TipoSalida ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@estatus", parametros.Estatus ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@agrupar_por", parametros.AgruparPor ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@pagina", parametros.Pagina);
                command.Parameters.AddWithValue("@registros_por_pagina", parametros.RegistrosPorPagina);

                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }
            }
        }

        _logger.LogInformation("Reporte Salidas ejecutado: {TablesCount} tabla(s)", dataSet.Tables.Count);
        return dataSet;
    }

    public async Task<DataSet> EjecutarReporteTraspasos(ReporteTraspasosDto parametros)
    {
        var dataSet = new DataSet();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("sp_Reporte_Traspasos", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 120;

                command.Parameters.AddWithValue("@fecha_inicio", parametros.FechaInicio ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@fecha_fin", parametros.FechaFin ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_almacen_origen", parametros.IdAlmacenOrigen ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_almacen_destino", parametros.IdAlmacenDestino ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_articulo", parametros.IdArticulo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_categoria", parametros.IdCategoria ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@estatus", parametros.Estatus ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@agrupar_por", parametros.AgruparPor ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@pagina", parametros.Pagina);
                command.Parameters.AddWithValue("@registros_por_pagina", parametros.RegistrosPorPagina);

                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }
            }
        }

        _logger.LogInformation("Reporte Traspasos ejecutado: {TablesCount} tabla(s)", dataSet.Tables.Count);
        return dataSet;
    }

    public async Task<DataSet> EjecutarReporteExistencias(ReporteExistenciasDto parametros)
    {
        var dataSet = new DataSet();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("sp_Reporte_Existencias", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 120;

                command.Parameters.AddWithValue("@id_almacen", parametros.IdAlmacen ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_articulo", parametros.IdArticulo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@id_categoria", parametros.IdCategoria ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@cantidad_minima", parametros.CantidadMinima ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@cantidad_maxima", parametros.CantidadMaxima ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@solo_sin_existencia", parametros.SoloSinExistencia);
                command.Parameters.AddWithValue("@agrupar_por", parametros.AgruparPor ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ordenar_por", parametros.OrdenarPor);
                command.Parameters.AddWithValue("@orden", parametros.Orden);
                command.Parameters.AddWithValue("@fecha_corte", parametros.FechaCorte ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@pagina", parametros.Pagina);
                command.Parameters.AddWithValue("@registros_por_pagina", parametros.RegistrosPorPagina);

                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }
            }
        }

        _logger.LogInformation("Reporte Existencias ejecutado: {TablesCount} tabla(s)", dataSet.Tables.Count);
        return dataSet;
    }

    public async Task<DataSet> EjecutarReporteKardex(ReporteKardexDto parametros)
    {
        var dataSet = new DataSet();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("sp_Kardex_Inventario", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 120;

                // Parámetros requeridos
                command.Parameters.AddWithValue("@id_almacen", parametros.IdAlmacen);
                command.Parameters.AddWithValue("@id_articulo", parametros.IdArticulo);
                
                // Parámetros opcionales
                command.Parameters.AddWithValue("@fecha_inicio", parametros.FechaInicio ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@fecha_fin", parametros.FechaFin ?? (object)DBNull.Value);

                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }
            }
        }

        _logger.LogInformation("Kardex ejecutado para artículo {IdArticulo}: {TablesCount} tabla(s)", 
            parametros.IdArticulo, dataSet.Tables.Count);
        return dataSet;
    }
}
```

---

## 📊 Implementación Base de ExcelExportService

### **ExcelExportService.cs**
```csharp
using System.Data;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using API.DTOs.Reportes;
using API.Services.Interfaces;

namespace API.Services.Implementation;

/// <summary>
/// Servicio para generación de archivos Excel usando EPPlus
/// </summary>
public class ExcelExportService : IExcelExportService
{
    private readonly ILogger<ExcelExportService> _logger;

    public ExcelExportService(ILogger<ExcelExportService> logger)
    {
        _logger = logger;
        
        // ⭐ IMPORTANTE: Configurar licencia EPPlus
        // NonCommercial = Uso gratuito para proyectos no comerciales
        // Commercial = Requiere licencia pagada
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public byte[] GenerarExcelEntradas(DataSet datos, ReporteEntradasDto parametros)
    {
        using (var package = new ExcelPackage())
        {
            // Hoja 1: Resumen
            if (datos.Tables.Count > 0 && datos.Tables[0].Rows.Count > 0)
            {
                var hojaResumen = package.Workbook.Worksheets.Add("Resumen");
                CargarDatosTabla(hojaResumen, datos.Tables[0], "RESUMEN DE ENTRADAS AL INVENTARIO");
                AplicarEstilosEncabezado(hojaResumen, Color.FromArgb(79, 129, 189)); // Azul
            }

            // Hoja 2: Detalle
            if (datos.Tables.Count > 1 && datos.Tables[1].Rows.Count > 0)
            {
                var hojaDetalle = package.Workbook.Worksheets.Add("Detalle");
                CargarDatosTabla(hojaDetalle, datos.Tables[1], "DETALLE DE ENTRADAS");
                AplicarEstilosEncabezado(hojaDetalle, Color.FromArgb(79, 129, 189));
                AplicarFormatoFechas(hojaDetalle, datos.Tables[1]);
            }

            _logger.LogInformation("Excel de Entradas generado: {Size} bytes", package.GetAsByteArray().Length);
            return package.GetAsByteArray();
        }
    }

    public byte[] GenerarExcelSalidas(DataSet datos, ReporteSalidasDto parametros)
    {
        using (var package = new ExcelPackage())
        {
            if (datos.Tables.Count > 0 && datos.Tables[0].Rows.Count > 0)
            {
                var hojaResumen = package.Workbook.Worksheets.Add("Resumen");
                CargarDatosTabla(hojaResumen, datos.Tables[0], "RESUMEN DE SALIDAS DEL INVENTARIO");
                AplicarEstilosEncabezado(hojaResumen, Color.FromArgb(192, 80, 77)); // Rojo
            }

            if (datos.Tables.Count > 1 && datos.Tables[1].Rows.Count > 0)
            {
                var hojaDetalle = package.Workbook.Worksheets.Add("Detalle");
                CargarDatosTabla(hojaDetalle, datos.Tables[1], "DETALLE DE SALIDAS");
                AplicarEstilosEncabezado(hojaDetalle, Color.FromArgb(192, 80, 77));
                AplicarFormatoFechas(hojaDetalle, datos.Tables[1]);
            }

            return package.GetAsByteArray();
        }
    }

    public byte[] GenerarExcelTraspasos(DataSet datos, ReporteTraspasosDto parametros)
    {
        using (var package = new ExcelPackage())
        {
            if (datos.Tables.Count > 0 && datos.Tables[0].Rows.Count > 0)
            {
                var hojaResumen = package.Workbook.Worksheets.Add("Resumen");
                CargarDatosTabla(hojaResumen, datos.Tables[0], "RESUMEN DE TRASPASOS ENTRE ALMACENES");
                AplicarEstilosEncabezado(hojaResumen, Color.FromArgb(155, 187, 89)); // Verde
            }

            if (datos.Tables.Count > 1 && datos.Tables[1].Rows.Count > 0)
            {
                var hojaDetalle = package.Workbook.Worksheets.Add("Detalle");
                CargarDatosTabla(hojaDetalle, datos.Tables[1], "DETALLE DE TRASPASOS");
                AplicarEstilosEncabezado(hojaDetalle, Color.FromArgb(155, 187, 89));
                AplicarFormatoFechas(hojaDetalle, datos.Tables[1]);
            }

            return package.GetAsByteArray();
        }
    }

    public byte[] GenerarExcelExistencias(DataSet datos, ReporteExistenciasDto parametros)
    {
        using (var package = new ExcelPackage())
        {
            if (datos.Tables.Count > 0 && datos.Tables[0].Rows.Count > 0)
            {
                var hojaResumen = package.Workbook.Worksheets.Add("Resumen");
                CargarDatosTabla(hojaResumen, datos.Tables[0], "RESUMEN DE EXISTENCIAS ACTUALES");
                AplicarEstilosEncabezado(hojaResumen, Color.FromArgb(128, 100, 162)); // Morado
            }

            if (datos.Tables.Count > 1 && datos.Tables[1].Rows.Count > 0)
            {
                var hojaDetalle = package.Workbook.Worksheets.Add("Detalle");
                CargarDatosTabla(hojaDetalle, datos.Tables[1], "DETALLE DE EXISTENCIAS");
                AplicarEstilosEncabezado(hojaDetalle, Color.FromArgb(128, 100, 162));
            }

            return package.GetAsByteArray();
        }
    }

    public byte[] GenerarExcelKardex(DataSet datos, ReporteKardexDto parametros)
    {
        using (var package = new ExcelPackage())
        {
            // Hoja 1: Información del artículo
            if (datos.Tables.Count > 0 && datos.Tables[0].Rows.Count > 0)
            {
                var hojaInfo = package.Workbook.Worksheets.Add("Información");
                CargarDatosTabla(hojaInfo, datos.Tables[0], "INFORMACIÓN DEL ARTÍCULO");
                AplicarEstilosEncabezado(hojaInfo, Color.FromArgb(247, 150, 70)); // Naranja
            }

            // Hoja 2: Movimientos (Kardex)
            if (datos.Tables.Count > 1 && datos.Tables[1].Rows.Count > 0)
            {
                var hojaKardex = package.Workbook.Worksheets.Add("Movimientos");
                CargarDatosTabla(hojaKardex, datos.Tables[1], "KARDEX DE MOVIMIENTOS");
                AplicarEstilosEncabezado(hojaKardex, Color.FromArgb(247, 150, 70));
                AplicarFormatoFechas(hojaKardex, datos.Tables[1]);
            }

            return package.GetAsByteArray();
        }
    }

    // ===================================================================
    // MÉTODOS PRIVADOS DE UTILIDAD
    // ===================================================================

    /// <summary>
    /// Carga los datos de una DataTable en una hoja Excel
    /// </summary>
    private void CargarDatosTabla(ExcelWorksheet worksheet, DataTable tabla, string titulo)
    {
        // Título del reporte (fila 1)
        worksheet.Cells[1, 1].Value = titulo;
        worksheet.Cells[1, 1, 1, tabla.Columns.Count].Merge = true;
        worksheet.Cells[1, 1].Style.Font.Size = 16;
        worksheet.Cells[1, 1].Style.Font.Bold = true;
        worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        worksheet.Row(1).Height = 30;

        // Fecha de generación (fila 2)
        worksheet.Cells[2, 1].Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        worksheet.Cells[2, 1, 2, tabla.Columns.Count].Merge = true;
        worksheet.Cells[2, 1].Style.Font.Italic = true;
        worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Encabezados de columnas (fila 4)
        int headerRow = 4;
        for (int col = 0; col < tabla.Columns.Count; col++)
        {
            worksheet.Cells[headerRow, col + 1].Value = tabla.Columns[col].ColumnName;
        }

        // Datos (desde fila 5)
        int dataStartRow = 5;
        for (int row = 0; row < tabla.Rows.Count; row++)
        {
            for (int col = 0; col < tabla.Columns.Count; col++)
            {
                worksheet.Cells[row + dataStartRow, col + 1].Value = tabla.Rows[row][col];
            }
        }

        // Autoajustar columnas
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Aplicar bordes a toda la tabla de datos
        var rangeData = worksheet.Cells[headerRow, 1, dataStartRow + tabla.Rows.Count - 1, tabla.Columns.Count];
        rangeData.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        rangeData.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        rangeData.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        rangeData.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }

    /// <summary>
    /// Aplica estilos al encabezado de la tabla
    /// </summary>
    private void AplicarEstilosEncabezado(ExcelWorksheet worksheet, Color colorEncabezado)
    {
        if (worksheet.Dimension == null) return;

        int headerRow = 4;
        int numColumnas = worksheet.Dimension.End.Column;

        var rangeHeader = worksheet.Cells[headerRow, 1, headerRow, numColumnas];
        rangeHeader.Style.Font.Bold = true;
        rangeHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
        rangeHeader.Style.Fill.BackgroundColor.SetColor(colorEncabezado);
        rangeHeader.Style.Font.Color.SetColor(Color.White);
        rangeHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        rangeHeader.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    }

    /// <summary>
    /// Aplica formato de fecha a las columnas que contienen fechas
    /// </summary>
    private void AplicarFormatoFechas(ExcelWorksheet worksheet, DataTable tabla)
    {
        if (worksheet.Dimension == null) return;

        int dataStartRow = 5;

        for (int col = 0; col < tabla.Columns.Count; col++)
        {
            // Detectar columnas de tipo DateTime
            if (tabla.Columns[col].DataType == typeof(DateTime))
            {
                var rangeColumna = worksheet.Cells[dataStartRow, col + 1, dataStartRow + tabla.Rows.Count - 1, col + 1];
                rangeColumna.Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";
            }
        }
    }
}
```

---

## 🔌 Registro de Dependencias

### **Program.cs**
```csharp
// ... código existente ...

// ⭐ Registrar servicios de reportes
builder.Services.AddScoped<IReportesService, ReportesService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// ⭐ Configuración de CORS (si la API está en otro dominio/puerto)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7001",  // Ajustar según tu puerto de Razor Pages
            "http://localhost:5001"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Content-Disposition"); // Importante para descargas
    });
});

// ... resto del código ...

// En el pipeline de middleware
app.UseCors("AllowFrontend");
```

---

## 🎨 Consumo desde Frontend (JavaScript)

### **Extensión de AjaxService.js**
```javascript
// ===================================================================
// MÓDULO DE REPORTES
// ===================================================================

/**
 * Descarga un archivo blob con el nombre especificado
 */
function descargarArchivo(blob, nombreArchivo) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = nombreArchivo;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}

// Agregar al objeto AjaxService existente
AjaxService.reportes = {
    /**
     * Genera reporte de entradas al inventario
     */
    generarEntradas(parametros) {
        const url = AjaxService.buildUrl('/api/reportes/entradas/generar');
        
        return $.ajax({
            url: url,
            method: 'POST',
            data: JSON.stringify(parametros),
            contentType: 'application/json',
            xhrFields: {
                responseType: 'blob' // ⭐ IMPORTANTE: Recibir como binario
            },
            beforeSend: function() {
                notificaciones.info('Generando reporte de entradas...', 'Procesando');
            }
        }).done(function(blob) {
            const nombreArchivo = `Reporte_Entradas_${new Date().getTime()}.xlsx`;
            descargarArchivo(blob, nombreArchivo);
            notificaciones.success('Reporte de entradas generado correctamente', 'Éxito');
        }).fail(function(xhr, status, error) {
            console.error('Error al generar reporte de entradas:', error);
            notificaciones.error('Error al generar el reporte de entradas', 'Error');
        });
    },

    /**
     * Genera reporte de salidas del inventario
     */
    generarSalidas(parametros) {
        const url = AjaxService.buildUrl('/api/reportes/salidas/generar');
        
        return $.ajax({
            url: url,
            method: 'POST',
            data: JSON.stringify(parametros),
            contentType: 'application/json',
            xhrFields: { responseType: 'blob' },
            beforeSend: function() {
                notificaciones.info('Generando reporte de salidas...', 'Procesando');
            }
        }).done(function(blob) {
            descargarArchivo(blob, `Reporte_Salidas_${new Date().getTime()}.xlsx`);
            notificaciones.success('Reporte de salidas generado correctamente', 'Éxito');
        }).fail(function(xhr, status, error) {
            console.error('Error al generar reporte de salidas:', error);
            notificaciones.error('Error al generar el reporte de salidas', 'Error');
        });
    },

    /**
     * Genera reporte de traspasos entre almacenes
     */
    generarTraspasos(parametros) {
        const url = AjaxService.buildUrl('/api/reportes/traspasos/generar');
        
        return $.ajax({
            url: url,
            method: 'POST',
            data: JSON.stringify(parametros),
            contentType: 'application/json',
            xhrFields: { responseType: 'blob' },
            beforeSend: function() {
                notificaciones.info('Generando reporte de traspasos...', 'Procesando');
            }
        }).done(function(blob) {
            descargarArchivo(blob, `Reporte_Traspasos_${new Date().getTime()}.xlsx`);
            notificaciones.success('Reporte de traspasos generado correctamente', 'Éxito');
        }).fail(function(xhr, status, error) {
            console.error('Error al generar reporte de traspasos:', error);
            notificaciones.error('Error al generar el reporte de traspasos', 'Error');
        });
    },

    /**
     * Genera reporte de existencias actuales
     */
    generarExistencias(parametros) {
        const url = AjaxService.buildUrl('/api/reportes/existencias/generar');
        
        return $.ajax({
            url: url,
            method: 'POST',
            data: JSON.stringify(parametros),
            contentType: 'application/json',
            xhrFields: { responseType: 'blob' },
            beforeSend: function() {
                notificaciones.info('Generando reporte de existencias...', 'Procesando');
            }
        }).done(function(blob) {
            descargarArchivo(blob, `Reporte_Existencias_${new Date().getTime()}.xlsx`);
            notificaciones.success('Reporte de existencias generado correctamente', 'Éxito');
        }).fail(function(xhr, status, error) {
            console.error('Error al generar reporte de existencias:', error);
            notificaciones.error('Error al generar el reporte de existencias', 'Error');
        });
    },

    /**
     * Genera reporte Kardex de un artículo
     */
    generarKardex(parametros) {
        const url = AjaxService.buildUrl('/api/reportes/kardex/generar');
        
        // Validar parámetros requeridos
        if (!parametros.IdAlmacen || !parametros.IdArticulo) {
            notificaciones.warning('Debe seleccionar un almacén y un artículo', 'Validación');
            return Promise.reject('Parámetros incompletos');
        }
        
        return $.ajax({
            url: url,
            method: 'POST',
            data: JSON.stringify(parametros),
            contentType: 'application/json',
            xhrFields: { responseType: 'blob' },
            beforeSend: function() {
                notificaciones.info('Generando kardex...', 'Procesando');
            }
        }).done(function(blob) {
            descargarArchivo(blob, `Kardex_${parametros.IdArticulo}_${new Date().getTime()}.xlsx`);
            notificaciones.success('Kardex generado correctamente', 'Éxito');
        }).fail(function(xhr, status, error) {
            console.error('Error al generar kardex:', error);
            notificaciones.error('Error al generar el kardex', 'Error');
        });
    }
};
```

---

## 📝 Ejemplo de Uso Completo desde Frontend

### **reportes.js (nuevo archivo)**
```javascript
/**
 * ====================================================================
 * REPORTES.JS - Módulo de Generación de Reportes
 * ====================================================================
 */

// ===================================================================
// VARIABLES GLOBALES
// ===================================================================
let catalogosReportes = {
    almacenes: [],
    categorias: [],
    donadores: [],
    beneficiarios: [],
    articulos: []
};

// ===================================================================
// INICIALIZACIÓN
// ===================================================================

function inicializarReportes() {
    console.log('✓ Inicializando módulo Reportes');
    
    cargarCatalogosReportes();
    configurarEventosReportes();
}

function cargarCatalogosReportes() {
    // Cargar catálogos necesarios para los filtros
    AjaxService.almacenes.obtenerTodos().then(data => {
        catalogosReportes.almacenes = Array.isArray(data) ? data : [];
        llenarSelectAlmacenesReportes();
    });
    
    AjaxService.categorias.obtenerTodas().then(data => {
        catalogosReportes.categorias = Array.isArray(data) ? data : [];
        llenarSelectCategoriasReportes();
    });
}

function llenarSelectAlmacenesReportes() {
    const $select = $('#selectAlmacenReporte');
    $select.empty().append('<option value="">-- Todos --</option>');
    
    catalogosReportes.almacenes.forEach(alm => {
        const id = alm.id_almacen || alm.idAlmacen || alm.id;
        const nombre = alm.nombre || `Almacén ${id}`;
        $select.append(`<option value="${id}">${nombre}</option>`);
    });
}

function llenarSelectCategoriasReportes() {
    const $select = $('#selectCategoriaReporte');
    $select.empty().append('<option value="">-- Todas --</option>');
    
    catalogosReportes.categorias.forEach(cat => {
        const id = cat.id_categoria || cat.idCategoria || cat.id;
        const nombre = cat.nombre || `Categoría ${id}`;
        $select.append(`<option value="${id}">${nombre}</option>`);
    });
}

// ===================================================================
// GENERACIÓN DE REPORTES
// ===================================================================

/**
 * Genera reporte de entradas
 */
function generarReporteEntradas() {
    const parametros = {
        fechaInicio: $('#fechaInicioEntradas').val() || null,
        fechaFin: $('#fechaFinEntradas').val() || null,
        idAlmacen: $('#selectAlmacenReporte').val() ? parseInt($('#selectAlmacenReporte').val()) : null,
        idCategoria: $('#selectCategoriaReporte').val() ? parseInt($('#selectCategoriaReporte').val()) : null,
        tipo: $('#selectTipoEntrada').val() || null,
        pagina: 1,
        registrosPorPagina: 1000
    };

    console.log('📊 Generando reporte de entradas:', parametros);
    AjaxService.reportes.generarEntradas(parametros);
}

/**
 * Genera reporte de salidas
 */
function generarReporteSalidas() {
    const parametros = {
        fechaInicio: $('#fechaInicioSalidas').val() || null,
        fechaFin: $('#fechaFinSalidas').val() || null,
        idAlmacen: $('#selectAlmacenReporte').val() ? parseInt($('#selectAlmacenReporte').val()) : null,
        idCategoria: $('#selectCategoriaReporte').val() ? parseInt($('#selectCategoriaReporte').val()) : null,
        tipoSalida: $('#selectTipoSalida').val() || null,
        pagina: 1,
        registrosPorPagina: 1000
    };

    console.log('📊 Generando reporte de salidas:', parametros);
    AjaxService.reportes.generarSalidas(parametros);
}

/**
 * Genera reporte de traspasos
 */
function generarReporteTraspasos() {
    const parametros = {
        fechaInicio: $('#fechaInicioTraspasos').val() || null,
        fechaFin: $('#fechaFinTraspasos').val() || null,
        idAlmacenOrigen: $('#selectAlmacenOrigen').val() ? parseInt($('#selectAlmacenOrigen').val()) : null,
        idAlmacenDestino: $('#selectAlmacenDestino').val() ? parseInt($('#selectAlmacenDestino').val()) : null,
        pagina: 1,
        registrosPorPagina: 1000
    };

    console.log('📊 Generando reporte de traspasos:', parametros);
    AjaxService.reportes.generarTraspasos(parametros);
}

/**
 * Genera reporte de existencias
 */
function generarReporteExistencias() {
    const parametros = {
        idAlmacen: $('#selectAlmacenReporte').val() ? parseInt($('#selectAlmacenReporte').val()) : null,
        idCategoria: $('#selectCategoriaReporte').val() ? parseInt($('#selectCategoriaReporte').val()) : null,
        soloSinExistencia: $('#checkSoloSinExistencia').is(':checked'),
        pagina: 1,
        registrosPorPagina: 1000
    };

    console.log('📊 Generando reporte de existencias:', parametros);
    AjaxService.reportes.generarExistencias(parametros);
}

/**
 * Genera reporte Kardex
 */
function generarReporteKardex() {
    const parametros = {
        idAlmacen: parseInt($('#selectAlmacenKardex').val()),
        idArticulo: parseInt($('#selectArticuloKardex').val()),
        fechaInicio: $('#fechaInicioKardex').val() || null,
        fechaFin: $('#fechaFinKardex').val() || null
    };

    console.log('📊 Generando kardex:', parametros);
    AjaxService.reportes.generarKardex(parametros);
}

// Exportar funciones globales
window.inicializarReportes = inicializarReportes;
window.generarReporteEntradas = generarReporteEntradas;
window.generarReporteSalidas = generarReporteSalidas;
window.generarReporteTraspasos = generarReporteTraspasos;
window.generarReporteExistencias = generarReporteExistencias;
window.generarReporteKardex = generarReporteKardex;

console.log('✓ reportes.js cargado completamente');
```

---

## ⚙️ Configuraciones de la API

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DONATIVOS;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "API.Services.ReportesService": "Debug",
      "API.Services.ExcelExportService": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

### **appsettings.Development.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "API.Services.ReportesService": "Trace"
    }
  }
}
```

---

## 🎯 Checklist de Implementación

### **Backend (API):**
- [ ] Crear proyecto API separado (si no existe)
- [ ] Instalar paquetes NuGet: EPPlus 7.5.2 y Microsoft.Data.SqlClient 5.2.2
- [ ] Crear carpeta `DTOs/Reportes/` y agregar 5 DTOs
- [ ] Crear `Services/Interfaces/IReportesService.cs`
- [ ] Crear `Services/Interfaces/IExcelExportService.cs`
- [ ] Implementar `Services/Implementation/ReportesService.cs`
- [ ] Implementar `Services/Implementation/ExcelExportService.cs`
- [ ] Crear `Controllers/ReportesController.cs` con 5 endpoints
- [ ] Registrar servicios en `Program.cs`
- [ ] Configurar CORS en `Program.cs`
- [ ] Configurar ConnectionString en `appsettings.json`
- [ ] Probar cada endpoint con Postman/Swagger

### **Frontend (Razor Pages):**
- [ ] Extender `AjaxService.js` con módulo `reportes`
- [ ] Crear `wwwroot/Scripts/reportes.js`
- [ ] Actualizar `Views/Registros/_Reportes.cshtml` con UI completa
- [ ] Agregar referencia a `reportes.js` en el layout/vista
- [ ] Implementar modales para parámetros de cada reporte
- [ ] Probar descarga de archivos Excel desde frontend

### **Testing:**
- [ ] Probar reporte de Entradas con diferentes filtros
- [ ] Probar reporte de Salidas con diferentes filtros
- [ ] Probar reporte de Traspasos con diferentes filtros
- [ ] Probar reporte de Existencias con diferentes filtros
- [ ] Probar reporte Kardex con artículos específicos
- [ ] Verificar formato de archivos Excel generados
- [ ] Verificar manejo de errores (SPs sin datos, errores de BD, etc.)
- [ ] Probar con datasets grandes (>1000 registros)

---

## 🔒 Consideraciones de Seguridad

1. **Validación de parámetros:**
   - Validar rangos de fechas (inicio no mayor que fin)
   - Validar IDs positivos
   - Sanitizar strings para prevenir SQL injection (aunque se usan parámetros, es buena práctica)

2. **Timeouts configurables:**
   - CommandTimeout de 120s para reportes grandes
   - Considerar aumentar para reportes muy pesados

3. **Rate limiting:**
   - Implementar limitador de requests para evitar abuso
   - Considerar caché de reportes frecuentes

4. **Logs detallados:**
   - Registrar cada generación de reporte con usuario y parámetros
   - Logs de errores con stack traces completos

5. **Manejo de errores SQL:**
   - Catch específico de SqlException
   - Mensajes genéricos al cliente, detalles en logs

---

## 📞 Información del Proyecto

- **Repositorio:** sistema-donativos
- **Rama Actual:** main
- **Proyecto Frontend:** donativos (Razor Pages .NET 10)
- **Proyecto API:** api-donativos
- **Base de Datos:** SQL Server (Local/Configurado) - Database: DONATIVOS

---

## 📚 Referencias y Documentación

### **EPPlus:**
- Documentación oficial: https://github.com/EPPlusSoftware/EPPlus
- Samples: https://github.com/EPPlusSoftware/EPPlus.Sample.NetCore

### **ADO.NET:**
- SqlDataAdapter: https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqldataadapter
- DataSet: https://learn.microsoft.com/en-us/dotnet/api/system.data.dataset

### **.NET 10:**
- What's new: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10

---

## 🚀 Próximos Pasos Sugeridos

1. **Fase 1: Configuración inicial (30 min)**
   - Crear estructura de carpetas en API
   - Instalar paquetes NuGet
   - Configurar ConnectionString

2. **Fase 2: DTOs e Interfaces (45 min)**
   - Crear los 5 DTOs
   - Crear las 2 interfaces de servicios

3. **Fase 3: ReportesService (2 horas)**
   - Implementar los 5 métodos de ejecución de SPs
   - Probar con Postman cada SP

4. **Fase 4: ExcelExportService (2 horas)**
   - Implementar generación básica de Excel
   - Refinar estilos y formatos

5. **Fase 5: Controller (1 hora)**
   - Implementar los 5 endpoints
   - Documentar con XML comments

6. **Fase 6: Frontend (2 horas)**
   - Extender AjaxService
   - Crear reportes.js
   - Actualizar _Reportes.cshtml

7. **Fase 7: Testing & Refinamiento (2 horas)**
   - Pruebas exhaustivas
   - Corrección de bugs
   - Optimización de performance

**Tiempo Total Estimado: 10-12 horas**

---

**Última Actualización:** [Fecha]  
**Versión del Documento:** 1.0  
**Autor:** Documentación generada para migración a proyecto API

---

# 🎉 Fin del Documento de Contexto

Este archivo contiene toda la información necesaria para implementar el módulo de reportes en tu proyecto API separado. Guárdalo en un lugar seguro y úsalo como referencia durante todo el proceso de desarrollo.

¡Buena suerte con la implementación! 🚀
