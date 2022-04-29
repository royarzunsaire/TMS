using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class R43
    {
        [Key]
        public int idR43 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fecha { get; set; }

        public virtual Encuesta encuesta { get; set; }

        public virtual Cliente cliente { get; set; }

        public virtual ClienteContacto clienteContacto { get; set; }
    }
}