# Guía de Migración RestSharp v106 a v107+

## Resumen de Cambios Realizados

Se ha actualizado el proyecto de RestSharp versión 106.15.0 a 107.0.3. Los cambios principales realizados fueron:

### 1. **Actualización del Paquete NuGet**
- **Archivo**: `TulaV2.csproj`
- Cambió: `RestSharp v106.15.0` ? `RestSharp v107.0.3`

### 2. **Cambios en la API de RestSharp**

#### RestClient y RestClientOptions
- **Antes (v106)**:
```csharp
var client = new RestClient(API_URL + "/endpoint");
client.Timeout = -1;
```

- **Ahora (v107+)**:
```csharp
var options = new RestClientOptions(API_URL + "/endpoint");
var client = new RestClient(options);
```

#### RestRequest
- **Antes (v106)**:
```csharp
var request = new RestRequest(Method.GET);
request.AddHeader("Authorization", "Bearer " + token);
```

- **Ahora (v107+)**:
```csharp
var request = new RestRequest();
request.AddHeader("Authorization", "Bearer " + token);
// El método se especifica en ExecuteAsync
```

#### Métodos de Ejecución
- **Antes (v106)**:
```csharp
IRestResponse response = client.Execute(request);
```

- **Ahora (v107+)**:
```csharp
// Los métodos son ahora asincronos y requieren await
RestResponse response = await client.ExecuteAsync(request);

// O de forma sincrónica (para compatibilidad):
RestResponse response = client.ExecuteAsync(request).Result;
```

#### Especificación de Método HTTP
- **Antes (v106)**:
```csharp
var request = new RestRequest(Method.POST);
var response = client.Execute(request);
```

- **Ahora (v107+)**:
```csharp
var request = new RestRequest();
// El método se pasa a ExecuteAsync:
var response = client.ExecuteAsync(request, Method.Post).Result;
```

### 3. **Solución de Compatibilidad: RestSharpHelper**

Se creó un archivo de extensión `TulaV2/Extensions/RestSharpHelper.cs` que centraliza el uso de RestSharp y proporciona métodos sincronos para facilitar la migración:

```csharp
// Uso simplificado:
var response = RestSharpHelper.ExecuteGet(API_URL, token, "/endpoint");
var response = RestSharpHelper.ExecutePost(API_URL, token, "/endpoint", body);
var response = RestSharpHelper.ExecutePut(API_URL, token, "/endpoint", body);
var response = RestSharpHelper.ExecuteDelete(API_URL, token, "/endpoint");
```

### 4. **Archivos Modificados**

#### Controllers Actualizados:
1. **`SalidasController.cs`**
   - Reemplazó uso directo de RestClient/RestRequest con `RestSharpHelper`
   - Cambió método `ExecuteApi` para usar el helper
   - Mantiene la misma funcionalidad con API compatible

2. **`RegistrosController.cs`**
   - Actualizado para usar `RestSharpHelper` en todos los métodos
   - Removed referencia innecesaria de `Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel`
   - Simplificado el código repetitivo

3. **`ReportesController.cs`**
   - Actualizado método `donador()` para usar `RestSharpHelper`
   - Actualizado método `Index()` para usar `RestSharpHelper`

### 5. **Ventajas de Esta Migración**

- ? **Compatibilidad con versiones recientes** de RestSharp
- ? **Código centralizado** en el helper para mantenimiento más fácil
- ? **Mejor rendimiento** con manejo moderno de HTTP
- ? **Soporte para async/await** cuando sea necesario
- ? **Menos repetición de código** en los controllers

### 6. **Pruebas Recomendadas**

1. Verificar que todas las llamadas a API funcionen correctamente:
   - Endpoints GET (obtener catálogos)
   - Endpoints POST (crear registros)
   - Endpoints PUT (actualizar registros)
   - Endpoints DELETE (eliminar registros)

2. Verificar manejo de errores:
   - Respuestas 401 (No autorizado)
   - Respuestas 500 (Error del servidor)
   - Timeouts y conexiones perdidas

3. Verificar respuestas JSON se desserializan correctamente

### 7. **Cambios Adicionales Considerados**

Si en el futuro se desea optimizar el código:
- Convertir métodos a async/await completamente (eliminar `.Result`)
- Implementar retry logic
- Agregar logging detallado de las llamadas HTTP
- Implementar circuit breaker para resiliencia

---

**Nota**: Los métodos en `RestSharpHelper` usan `.Result` para mantener la compatibilidad sincrónica. En una futura refactorización, se recomienda convertir a async/await en los controllers.
