using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ClienteGiro")]
    public class ClienteGiro
    {
        //[Key]
        //public int idClienteGiro { get; set; }
        public virtual int idCliente { get; set; }
        public virtual int idGiro { get; set; }
        public virtual Giro giro { get; set; }
        public virtual Cliente cliente { get; set; }
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }
    }
}