using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ClienteUsuario")]
    public class ClienteUsuario
    {
        [Key]
        public virtual int idClienteUsuario { get; set; }
        public virtual Cliente cliente { get; set; }
        public virtual AspNetUsers usuario { get; set; }
        public virtual DateTime fechaAsignado { get; set; }
    }
}