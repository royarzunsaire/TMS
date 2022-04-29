using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class CostoHistorialComercializacion
    {
        [Key]
        public int idCostoHistorialComercializacion { get; set; }
        [Display(Name = "Detalles")]
        public string detalle { get; set; }
        [Display(Name = "Cantidad")]
        public string cantidad { get; set; }
        [Display(Name = "Valor")]
        public string valor { get; set; }
        [Display(Name = "Total")]
        public string total { get; set; }
    }
}