using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ClienteEstadoComercial")]
    public class ClienteEstadoComercial
    {
        [Key]
        public virtual string idClienteGiro { get; set; }
        public virtual DateTime fechaCreacion { get; set; }
        public virtual string usuarioCreador { get; set; }
        public virtual EstadoComercial estadoComercial { get; set; }
        public virtual Cliente cliente { get; set; }
    }
}