using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("DocumentoCompromiso")]
    public class DocumentoCompromiso
    {
        [Key]
        public int idDocumentoCompromiso { get; set; }

        [Display(Name = "Número de Serie")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string numeroSerie { get; set; }

        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int? monto { get; set; }

        public virtual Storage documento { get; set; }

        public bool vigencia { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }

        public virtual AspNetUsers usuarioUltimaModificacion { get; set; }

        public DateTime fechaCreacion { get; set; }

        public DateTime fechaUltimaModificacion { get; set; }

        public bool softDelete { get; set; }

        [Display(Name = "Tipo de Documento")]
        public virtual TiposDocumentosPago tipoDocCompromiso { get; set; }

        public virtual Factura factura { get; set; }

        public virtual Cotizacion_R13 cotizacion { get; set; }

        [NotMapped]
        public Comercializacion comercializacion { get; set; }

        public virtual Pago tipoVenta { get; set; }
    }
}