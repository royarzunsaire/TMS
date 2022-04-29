using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class SeccionEncuesta
    {
        [Key]
        public int idSeccionEncuesta { get; set; }

        public int posicion { get; set; }

        public virtual Formulario formulario { get; set; }
    }
}