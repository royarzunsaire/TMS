using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ClienteTipoDocumentosPago")]
    public class ClienteTipoDocumentosPago
    {
        //[Key]
        //public int idClienteTipoDocumentosPago { get; set; }
        public virtual int idCliente { get; set; }
        public virtual int idTipoDocumentosPago { get; set; }
        public virtual TiposDocumentosPago tipoDocumentosPago { get; set; }
        public virtual Cliente cliente { get; set; }
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }
    }
}