namespace donativos.Models
{
    /// <summary>
    /// Modelo para recibir credenciales en el login. 
    /// No debe mezclarse con el modelo Usuario que viene del API.
    /// </summary>
    public class LoginViewModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
