using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class R52
    {
        [Key]
        public int idR52 { get; set; }

        public int idFormularioCualitativa { get; set; }

        public virtual Comercializacion comercializacion { get; set; }

        public virtual Relator relator { get; set; }

        public virtual Encuesta encuesta { get; set; }
    }
}