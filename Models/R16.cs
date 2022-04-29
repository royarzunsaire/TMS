using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class R16
    {
        [Key]
        public int idR16 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fecha { get; set; }

        public virtual Encuesta encuesta { get; set; }

        public virtual Relator relator { get; set; }

        public virtual Storage archivo { get; set; }

        //public virtual ICollection<RespuestasContestadasFormulario> respuestas { get; set; }
    }
}