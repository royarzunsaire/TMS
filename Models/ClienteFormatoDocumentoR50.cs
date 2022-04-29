using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ClienteFormatoDocumentoR50")]
    public class ClienteFormatoDocumentoR50
    {
        [Key]
        public virtual string idClienteGiro { get; set; }
        public virtual DateTime fechaCreacion { get; set; }
        public virtual string usuarioCreador { get; set; }
        public virtual FormatoDocumentoR50 formatoDocumentoR50 { get; set; }
        public virtual Cliente cliente { get; set; }
    }
}