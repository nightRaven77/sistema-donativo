namespace apiDonativos.Models.Dtos
{
    public class RegistrarUsuarioRequest
    {
        public string usuario { get; set; }
        public string password { get; set; }
        public string centroAcopio { get; set; }
        public int centroID { get; set; }
        public string nombreCompleto { get; set; }
        public string correoElectronico { get; set; }
        public string telefono { get; set; }
    }

    public class RegistrarUsuarioResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
        public int? idUsuario { get; set; }
    }

    public class ActualizarPasswordRequest
    {
        public string usuario { get; set; }
        public string passwordActual { get; set; }
        public string nuevaPassword { get; set; }
    }

    public class ActualizarPasswordResponse
    {
        public bool exito { get; set; }
        public string mensaje { get; set; }
    }
}