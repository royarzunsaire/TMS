using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Encuesta
    {
        [Key]
        public int idEncuesta { get; set; }

        public virtual ICollection<SeccionEncuesta> seccionEncuesta { get; set; }

        public virtual ICollection<RespuestasContestadasFormulario> respuestas { get; set; }
    }
}