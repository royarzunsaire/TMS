using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ClienteContactoCotizacion")]
    public class ClienteContactoCotizacion
    {


        public virtual int idContacto { get; set; }
        public virtual int idCliente { get; set; }
        public virtual Contacto contacto { get; set; }
        public virtual Cliente cliente { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public bool vigencia { get; set; }
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }



    }
}