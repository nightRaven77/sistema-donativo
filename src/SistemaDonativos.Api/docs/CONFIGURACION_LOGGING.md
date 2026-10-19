# 📋 Configuración de Logging en Producción

## 📖 Descripción

Este documento describe cómo configurar y acceder a los logs de la API de Donativos en ambientes de pruebas y producción cuando se ejecuta en IIS.

---

## 🔍 Problema Común

**Síntoma:** La aplicación tiene logging configurado con Serilog pero no se ven los archivos de logs en el servidor.

**Causa raíz:** Falta de permisos de escritura para el Application Pool de IIS en la carpeta de logs.

---

## 📍 Ubicación de los Logs

La aplicación está configurada para escribir logs en:

```
<DirectorioBase>\logs\api-YYYYMMDD.txt
```

**Ejemplo en IIS:**
```
C:\inetpub\wwwroot\apiDonativos\logs\api-20250128.txt
```

### Características de los logs:
- ✅ **Rotación diaria** - Un archivo nuevo cada día
- ✅ **Retención de 30 días** - Se eliminan automáticamente logs antiguos
- ✅ **Límite de 10MB** por archivo
- ✅ **Formato estructurado** con timestamp, nivel y mensaje

---

## ✅ Solución: Configurar Permisos en IIS

### 1️⃣ Identificar el Application Pool

1. Abrir **IIS Manager** (`inetmgr`)
2. Seleccionar el sitio de la API (ej: `apiDonativos`)
3. En el panel derecho, clic en **"Basic Settings"**
4. Anotar el nombre del **Application Pool** (ej: `DefaultAppPool` o `apiDonativos`)

### 2️⃣ Dar Permisos de Escritura (PowerShell)

Ejecutar en **PowerShell como Administrador**:

```powershell
# ============================================
# CONFIGURACIÓN - Ajustar según tu entorno
# ============================================
$rutaApp = "C:\inetpub\wwwroot\apiDonativos"  # Ruta real de tu API
$appPoolName = "apiDonativos"                 # Nombre de tu AppPool

# ============================================
# SCRIPT - No modificar
# ============================================

# 1. Crear carpeta logs si no existe
Write-Host "Creando carpeta de logs..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "$rutaApp\logs" | Out-Null

# 2. Obtener ACL actual
$acl = Get-Acl "$rutaApp\logs"

# 3. Crear regla de permisos para el AppPool
$identity = "IIS AppPool\$appPoolName"
$fileSystemRights = "FullControl"
$inheritanceFlags = "ContainerInherit,ObjectInherit"
$propagationFlags = "None"
$accessControlType = "Allow"

$permission = $identity, $fileSystemRights, $inheritanceFlags, $propagationFlags, $accessControlType
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission

# 4. Aplicar permisos
$acl.SetAccessRule($accessRule)
Set-Acl "$rutaApp\logs" $acl

Write-Host "✅ Permisos configurados correctamente para '$appPoolName'" -ForegroundColor Green
Write-Host "📁 Ruta de logs: $rutaApp\logs" -ForegroundColor Cyan

# 5. Reiniciar AppPool
Write-Host "Reiniciando Application Pool..." -ForegroundColor Yellow
Restart-WebAppPool -Name $appPoolName

Write-Host "✅ Configuración completada. Los logs se crearán en la próxima petición." -ForegroundColor Green
```

### 3️⃣ Verificar Configuración

```powershell
# Verificar permisos
$acl = Get-Acl "C:\inetpub\wwwroot\apiDonativos\logs"
$acl.Access | Where-Object { $_.IdentityReference -like "*AppPool*" } | Format-Table

# Hacer una petición de prueba a la API
Invoke-WebRequest -Uri "http://localhost/apiDonativos/api/health" -UseBasicParsing

# Verificar que se creó el archivo de log
dir "C:\inetpub\wwwroot\apiDonativos\logs"
```

---

## 📖 Leer los Logs

### Opción 1: Ver el log más reciente

```powershell
# Ver todo el contenido
Get-Content "C:\inetpub\wwwroot\apiDonativos\logs\api-20250128.txt"

# Ver las últimas 50 líneas
Get-Content "C:\inetpub\wwwroot\apiDonativos\logs\api-20250128.txt" -Tail 50

# Ver en tiempo real (modo "tail -f")
Get-Content "C:\inetpub\wwwroot\apiDonativos\logs\api-20250128.txt" -Wait -Tail 50
```

### Opción 2: Buscar errores

```powershell
# Buscar todos los errores del día
Select-String -Path "C:\inetpub\wwwroot\apiDonativos\logs\api-20250128.txt" -Pattern "\[ERR\]"

# Buscar logs de un usuario específico
Select-String -Path "C:\inetpub\wwwroot\apiDonativos\logs\api-20250128.txt" -Pattern "Usuario: 5"

# Buscar logs de un módulo específico (Salida, Traspaso, etc.)
Select-String -Path "C:\inetpub\wwwroot\apiDonativos\logs\api-20250128.txt" -Pattern "salida"
```

### Opción 3: Logs de los últimos 7 días

```powershell
# Ver todos los archivos de log
dir "C:\inetpub\wwwroot\apiDonativos\logs"

# Buscar errores en los últimos 7 días
Get-ChildItem "C:\inetpub\wwwroot\apiDonativos\logs\*.txt" | 
    Where-Object { $_.LastWriteTime -gt (Get-Date).AddDays(-7) } | 
    Select-String -Pattern "\[ERR\]"
```

---

## 🔧 Configuración Avanzada

### Opción A: Usar Carpeta Centralizada del Sistema

Si prefieres tener los logs en una ubicación centralizada fuera de `wwwroot`:

**Modificar `Program.cs`:**

```csharp
// Opción 2 (RECOMENDADO PRODUCCIÓN): Carpeta centralizada del sistema
var logPath = @"D:\Logs\ApiDonativos\api-.txt";
```

**Crear carpeta y dar permisos:**

```powershell
$rutaLogsCentralizada = "D:\Logs\ApiDonativos"
$appPoolName = "apiDonativos"

# Crear carpeta
New-Item -ItemType Directory -Force -Path $rutaLogsCentralizada

# Dar permisos
$acl = Get-Acl $rutaLogsCentralizada
$permission = "IIS AppPool\$appPoolName","FullControl","ContainerInherit,ObjectInherit","None","Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $rutaLogsCentralizada $acl
```

### Opción B: Configurar Niveles de Log por Ambiente

**Crear `appsettings.Production.json`:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "apiDonativos.Services": "Information"
    }
  }
}
```

**Modificar `Program.cs`** para usar configuración desde JSON:

```csharp
// Instalar: dotnet add package Serilog.Settings.Configuration

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

---

## 🐛 Troubleshooting

### ❌ Problema: No se crea la carpeta `logs`

**Diagnóstico:**

```powershell
# Verificar permisos del AppPool en el directorio raíz
$acl = Get-Acl "C:\inetpub\wwwroot\apiDonativos"
$acl.Access | Where-Object { $_.IdentityReference -like "*AppPool*" } | Format-Table
```

**Solución:**

```powershell
# Dar permisos al directorio raíz también
$rutaApp = "C:\inetpub\wwwroot\apiDonativos"
$appPoolName = "apiDonativos"

$acl = Get-Acl $rutaApp
$permission = "IIS AppPool\$appPoolName","Modify","ContainerInherit,ObjectInherit","None","Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $rutaApp $acl
```

---

### ❌ Problema: Logs no aparecen después de configurar permisos

**Revisar Event Viewer:**

```powershell
# Abrir Event Viewer
eventvwr.msc

# Navegar a: Windows Logs > Application
# Buscar eventos de tipo "Error" o "Warning" con fuente "IIS"
```

**Verificar que la aplicación está ejecutándose:**

```powershell
# Ver estado del AppPool
Get-WebAppPoolState -Name "apiDonativos"

# Reiniciar si está detenido
Start-WebAppPool -Name "apiDonativos"
```

**Verificar que Serilog se inicializa correctamente:**

Los logs deben mostrar al inicio:

```
2025-01-28 10:30:45.123 [INF] ==========================================
2025-01-28 10:30:45.124 [INF] Iniciando aplicación API Donativos
2025-01-28 10:30:45.125 [INF] Ruta de logs: C:\inetpub\wwwroot\apiDonativos\logs\api-.txt
2025-01-28 10:30:45.126 [INF] Directorio base: C:\inetpub\wwwroot\apiDonativos\
2025-01-28 10:30:45.127 [INF] ==========================================
```

---

### ❌ Problema: Logs muy grandes

**Verificar tamaño de archivos:**

```powershell
Get-ChildItem "C:\inetpub\wwwroot\apiDonativos\logs" | 
    Sort-Object Length -Descending | 
    Select-Object Name, @{Name="SizeMB";Expression={[math]::Round($_.Length/1MB,2)}} | 
    Format-Table
```

**Solución: Ajustar retención en `Program.cs`:**

```csharp
.WriteTo.File(
    path: logPath,
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7,  // ✅ Reducir a 7 días
    fileSizeLimitBytes: 5_000_000,  // ✅ Reducir a 5MB
    // ...
)
```

---

## 📊 Niveles de Log

La aplicación usa los siguientes niveles:

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| `[INF]` | Información general | `Salida creada exitosamente en 245ms. Folio: SAL-2025-00042` |
| `[WRN]` | Advertencias | `Error de validación al crear salida. Usuario: 5, Error: Inventario insuficiente` |
| `[ERR]` | Errores | `Error de base de datos al crear salida. Usuario: 5` |
| `[FTL]` | Errores fatales | `La aplicación terminó inesperadamente` |

---

## 📚 Recursos Adicionales

- [Serilog Documentation](https://serilog.net/)
- [IIS Configuration Reference](https://learn.microsoft.com/en-us/iis/configuration/)
- [Troubleshooting IIS Permissions](https://learn.microsoft.com/en-us/troubleshoot/developer/webapps/iis/www-administration-management/403-forbidden-request)

---

## ✅ Checklist de Configuración

- [ ] Identificar el Application Pool de la API
- [ ] Ejecutar script de permisos en PowerShell
- [ ] Reiniciar el Application Pool
- [ ] Hacer petición de prueba a la API
- [ ] Verificar que se creó la carpeta `logs/`
- [ ] Verificar que se creó el archivo `api-YYYYMMDD.txt`
- [ ] Leer el contenido del log y confirmar mensajes de inicio
- [ ] Configurar monitoreo automático (opcional)

---

**Última actualización:** 28 de enero de 2025  
**Versión de la API:** .NET 10  
**Framework de Logging:** Serilog 3.x
