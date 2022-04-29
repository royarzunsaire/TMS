using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class RelatorHistorialComercializacion
    {
        [Key]
        public int idRelatorHistorialComercializacion { get; set; }
        [Display(Name = "Nombre")]
        public string nombre { get; set; }
        [Display(Name = "RUN")]
        public string run { get; set; }
        [Display(Name = "Correo Electrónico")]
        public string correoElectronico { get; set; }
        [Display(Name = "Teléfono/Celular")]
        public string telefono { get; set; }
        [Display(Name = "Válido SENCE")]
        public string validoSence { get; set; }
    }
}