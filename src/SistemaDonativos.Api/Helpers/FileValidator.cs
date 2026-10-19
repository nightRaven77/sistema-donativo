using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace apiDonativos.Helpers
{
    /// <summary>
    /// Validador de archivos para prevenir uploads maliciosos o excesivos
    /// ✅ FASE 2: Seguridad y límites en archivos
    /// </summary>
    public static class FileValidator
    {
        /// <summary>
        /// Extensiones de archivo permitidas (sin punto)
        /// </summary>
        private static readonly HashSet<string> ExtensionesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "pdf",   // Documentos
            "jpg",   // Imágenes
            "jpeg",
            "png",
            "gif",
            "xlsx",  // Excel
            "xls",
            "docx",  // Word
            "doc"
        };

        /// <summary>
        /// Tamaño máximo por archivo: 5 MB
        /// </summary>
        public const long TamañoMaximoPorArchivo = 5 * 1024 * 1024; // 5 MB en bytes

        /// <summary>
        /// Número máximo de archivos por donativo/salida/traspaso
        /// </summary>
        public const int MaximoArchivos = 10;

        /// <summary>
        /// Valida un nombre de archivo (extensión y longitud)
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo a validar</param>
        /// <param name="errorMessage">Mensaje de error si falla la validación</param>
        /// <returns>True si es válido, False en caso contrario</returns>
        public static bool ValidarNombreArchivo(string nombreArchivo, out string errorMessage)
        {
            errorMessage = null;

            // Validar que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                errorMessage = "El nombre del archivo no puede estar vacío";
                return false;
            }

            // Validar longitud del nombre
            if (nombreArchivo.Length > 255)
            {
                errorMessage = "El nombre del archivo excede la longitud máxima de 255 caracteres";
                return false;
            }

            // Obtener extensión
            string extension = Path.GetExtension(nombreArchivo)?.TrimStart('.')?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension))
            {
                errorMessage = "El archivo debe tener una extensión válida";
                return false;
            }

            // Validar extensión permitida
            if (!ExtensionesPermitidas.Contains(extension))
            {
                errorMessage = $"Extensión '{extension}' no permitida. Extensiones válidas: {string.Join(", ", ExtensionesPermitidas)}";
                return false;
            }

            // Validar caracteres peligrosos en el nombre
            char[] caracteresInvalidos = Path.GetInvalidFileNameChars();
            if (nombreArchivo.Any(c => caracteresInvalidos.Contains(c)))
            {
                errorMessage = "El nombre del archivo contiene caracteres inválidos";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida el tamaño de un archivo (en bytes)
        /// </summary>
        /// <param name="tamañoBytes">Tamaño del archivo en bytes</param>
        /// <param name="errorMessage">Mensaje de error si falla la validación</param>
        /// <returns>True si es válido, False en caso contrario</returns>
        public static bool ValidarTamañoArchivo(long tamañoBytes, out string errorMessage)
        {
            errorMessage = null;

            if (tamañoBytes <= 0)
            {
                errorMessage = "El tamaño del archivo debe ser mayor a 0";
                return false;
            }

            if (tamañoBytes > TamañoMaximoPorArchivo)
            {
                double tamañoMB = tamañoBytes / (1024.0 * 1024.0);
                double maxMB = TamañoMaximoPorArchivo / (1024.0 * 1024.0);
                errorMessage = $"El archivo excede el tamaño máximo permitido de {maxMB:F2} MB (tamaño actual: {tamañoMB:F2} MB)";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida una lista de archivos (cantidad y validaciones individuales)
        /// </summary>
        /// <param name="archivos">Lista de nombres de archivos</param>
        /// <param name="errorMessage">Mensaje de error si falla la validación</param>
        /// <returns>True si es válido, False en caso contrario</returns>
        public static bool ValidarListaArchivos(IEnumerable<string> archivos, out string errorMessage)
        {
            errorMessage = null;

            if (archivos == null)
            {
                return true; // Lista nula es válida (sin archivos)
            }

            var listaArchivos = archivos.ToList();

            // Validar cantidad máxima
            if (listaArchivos.Count > MaximoArchivos)
            {
                errorMessage = $"Se excede el límite de {MaximoArchivos} archivos por registro (intentando subir {listaArchivos.Count})";
                return false;
            }

            // Validar cada archivo individual
            for (int i = 0; i < listaArchivos.Count; i++)
            {
                if (!ValidarNombreArchivo(listaArchivos[i], out string error))
                {
                    errorMessage = $"Archivo #{i + 1}: {error}";
                    return false;
                }
            }

            // Validar nombres duplicados
            var nombresDuplicados = listaArchivos
                .GroupBy(a => a.ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (nombresDuplicados.Any())
            {
                errorMessage = $"Archivos duplicados detectados: {string.Join(", ", nombresDuplicados)}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida una URL de archivo (incluye almacenamiento externo)
        /// Acepta URLs HTTP/HTTPS y rutas de almacenamiento externo (Azure Blob, AWS S3, etc.)
        /// </summary>
        /// <param name="url">URL o ruta del archivo</param>
        /// <param name="errorMessage">Mensaje de error si falla la validación</param>
        /// <returns>True si es válido, False en caso contrario</returns>
        public static bool ValidarUrlArchivo(string url, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(url))
            {
                errorMessage = "La URL del archivo no puede estar vacía";
                return false;
            }

            // Validar longitud máxima de URL
            if (url.Length > 2048)
            {
                errorMessage = "La URL del archivo excede la longitud máxima de 2048 caracteres";
                return false;
            }

            // ✅ AJUSTE: Intentar validar como URI (más flexible para almacenamiento externo)
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                // Si no es una URI absoluta, validar que sea una ruta válida
                if (!Uri.TryCreate(url, UriKind.Relative, out _))
                {
                    errorMessage = "La URL del archivo no tiene un formato válido";
                    return false;
                }
                // Si es relativa, está OK para almacenamiento externo
                return true;
            }

            // ✅ AJUSTE: Aceptar más esquemas para almacenamiento externo
            var esquemasPermitidos = new[] 
            { 
                Uri.UriSchemeHttp,      // http://
                Uri.UriSchemeHttps,     // https://
                Uri.UriSchemeFtp,       // ftp:// (para almacenamiento FTP)
                Uri.UriSchemeFile,      // file:// (para pruebas locales)
                "blob",                 // blob:// (Azure Blob Storage)
                "s3",                   // s3:// (AWS S3)
                "gs"                    // gs:// (Google Cloud Storage)
            };

            if (!esquemasPermitidos.Contains(uriResult.Scheme.ToLowerInvariant()))
            {
                errorMessage = $"Esquema '{uriResult.Scheme}' no permitido. Esquemas válidos: {string.Join(", ", esquemasPermitidos)}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtiene el tipo MIME basado en la extensión del archivo
        /// </summary>
        public static string ObtenerTipoMime(string nombreArchivo)
        {
            string extension = Path.GetExtension(nombreArchivo)?.TrimStart('.')?.ToLowerInvariant();

            return extension switch
            {
                "pdf" => "application/pdf",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xls" => "application/vnd.ms-excel",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "doc" => "application/msword",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Sanitiza un nombre de archivo para prevenir ataques de path traversal
        /// </summary>
        public static string SanitizarNombreArchivo(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return "archivo_sin_nombre";

            // Remover caracteres inválidos
            char[] caracteresInvalidos = Path.GetInvalidFileNameChars();
            string nombreLimpio = new string(nombreArchivo
                .Where(c => !caracteresInvalidos.Contains(c))
                .ToArray());

            // Prevenir path traversal
            nombreLimpio = Path.GetFileName(nombreLimpio);

            // Limitar longitud
            if (nombreLimpio.Length > 200)
            {
                string extension = Path.GetExtension(nombreLimpio);
                string nombreBase = Path.GetFileNameWithoutExtension(nombreLimpio);
                nombreLimpio = nombreBase.Substring(0, 200 - extension.Length) + extension;
            }

            return nombreLimpio;
        }
    }
}
