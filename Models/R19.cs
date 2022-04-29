using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class R19
    {
        [Key]
        public int idR19 { get; set; }

        public int? idFormularioRelator { get; set; }

        public virtual Comercializacion comercializacion { get; set; }

        public virtual Relator relator { get; set; }

        public virtual Encuesta encuesta { get; set; }
    }
}