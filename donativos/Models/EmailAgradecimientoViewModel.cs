using System;
using System.Collections.Generic;

namespace donativos.Models
{
    public class EmailAgradecimientoViewModel
    {
        public string NombreDonante { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public List<ArticuloAgradecimientoDTO> Articulos { get; set; }
        public string NombreCentroAcopio { get; set; }
        public string EmailDestino { get; set; }
        public string LogoContentId { get; set; }
    }

    public class ArticuloAgradecimientoDTO
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public string UnidadMedida { get; set; }
    }
}
