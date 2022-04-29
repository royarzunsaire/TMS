using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("Costo")]
    public class Costo
    {
        [Key]
        public int idCosto { get; set; }
        public int idCotizacion { get; set; }
        public string detalle { get; set; }
        public int cantidad { get; set; }
        public int valor { get; set; }
        public int total { get; set; }

        public int valorMinimo { get; set; }
        public int valorMaximo { get; set; }

        public virtual Cotizacion_R13 cotizacion { get; set; }

    }
}