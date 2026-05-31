# Configuración de Credenciales - Sistema de Donativos

## 🔐 User Secrets (Desarrollo Local)

Las credenciales **NO** deben estar en `appsettings.json`. En desarrollo local, usa **User Secrets**:

### Configurar credenciales locales

```bash
cd donativos

# Inicializar User Secrets (ya está hecho si ves UserSecretsId en el .csproj)
dotnet user-secrets init

# Configurar cadena de conexión
dotnet user-secrets set "ConnectionStrings:donativoCn" "Server=TU_SERVIDOR;Database=DonativosDB;User Id=TU_USUARIO;Password=TU_PASSWORD;..."

# Configurar Email
dotnet user-secrets set "Email:Username" "tu-email@gmail.com"
dotnet user-secrets set "Email:Password" "tu-app-password"

# Configurar FileStorage (NAS)
dotnet user-secrets set "FileStorage:Username" "tu-usuario-nas"
dotnet user-secrets set "FileStorage:Password" "tu-password-nas"
```

### Verificar secretos configurados

```bash
dotnet user-secrets list
```

---

## 🌐 Variables de Entorno (Producción/Staging)

En servidores de producción o staging, usa **variables de entorno**:

### Windows Server (IIS)

En el servidor, configura las variables en el Application Pool o sistema:

```powershell
# Ejemplo: configurar en variables de entorno del sistema
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__donativoCn", "Server=...", "Machine")
[System.Environment]::SetEnvironmentVariable("Email__Username", "correo@example.com", "Machine")
[System.Environment]::SetEnvironmentVariable("Email__Password", "password", "Machine")
[System.Environment]::SetEnvironmentVariable("FileStorage__Username", "usuario", "Machine")
[System.Environment]::SetEnvironmentVariable("FileStorage__Password", "password", "Machine")
```

**Nota:** En variables de entorno, reemplaza `:` por `__` (doble guión bajo).

### Linux/Docker

En el `docker-compose.yml` o archivo `.env`:

```yaml
environment:
  - ConnectionStrings__donativoCn=Server=...
  - Email__Username=correo@example.com
  - Email__Password=password
  - FileStorage__Username=usuario
  - FileStorage__Password=password
```

### Azure App Service

En el portal de Azure:
1. Ve a tu App Service
2. **Configuration** > **Application settings**
3. Agrega las variables:
   - `ConnectionStrings__donativoCn`
   - `Email__Username`
   - `Email__Password`
   - `FileStorage__Username`
   - `FileStorage__Password`

---

## ✅ Validación

Para verificar que las credenciales se cargan correctamente, revisa los logs al iniciar la aplicación. 

**IMPORTANTE:** Si las credenciales no están configuradas, la aplicación lanzará excepciones al arrancar.

---

## ⚠️ NUNCA comitear credenciales

Los archivos `appsettings.json` y `appsettings.Development.json` **NO** deben contener credenciales reales. 
Solo deben tener placeholders vacíos o valores de ejemplo no sensibles.
