using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("RepresentanteLegal")]
    public class RepresentanteLegal
    {
        [Key]
        public int idRepresentanteLegal { get; set; }
        public int idCliente { get; set; }
        public virtual Cliente cliente { get; set; }
        public int idContacto { get; set; }
        public virtual Contacto contacto { get; set; }
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }

    }
}