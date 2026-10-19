using System.Collections.Generic;

namespace donativos.Models
{
    /// <summary>
    /// Respuesta estándar de la API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Respuesta genérica sin datos
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
    }

    /// <summary>
    /// Respuesta para operaciones de archivos
    /// </summary>
    public class ArchivoResponse
    {
        public string Url { get; set; }
        public string NombreOriginal { get; set; }
        public string NombreGuardado { get; set; }
    }

    /// <summary>
    /// Respuesta para guardado de archivos
    /// </summary>
    public class GuardarArchivosResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public List<ArchivoResponse> Archivos { get; set; }
        public string Diagnostico { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Respuesta para inserción de artículos
    /// </summary>
    public class InsertarArticuloResultado
    {
        public bool Exito { get; set; }
        public string Nombre { get; set; }
        public string Mensaje { get; set; }
        public string Respuesta { get; set; }
        public string StatusCode { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Respuesta para inserción múltiple de artículos
    /// </summary>
    public class InsertarArticulosResponse
    {
        public bool Exito { get; set; }
        public int Procesados { get; set; }
        public int Exitosos { get; set; }
        public int Errores { get; set; }
        public List<InsertarArticuloResultado> Resultados { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }
    }
}
