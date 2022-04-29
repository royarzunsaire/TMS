using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Observacion
    {
        [Key]
        public int idObservacion { get; set; }

        public DateTime fechaCreacion { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }

        [DataType(DataType.MultilineText)]
        public string observacion { get; set; }
    }
}