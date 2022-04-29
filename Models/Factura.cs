using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Factura
    {
        //[ForeignKey("documentoCompromiso")]
        [Key]
        public int idFactura { get; set; }

        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int costo { get; set; }

        [Display(Name = "Número de Serie")]
        [DataType(DataType.Text)]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string numero { get; set; }

        [Display(Name = "Valor Pagado")]
        [DataType(DataType.Currency)]
        public int valorPagado { get; set; }

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public TipoFactura? tipo { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string observacion { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaCreacion { get; set; }


        [Display(Name = "Fecha Facturación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaFacturacion { get; set; }

        public DateTime fechaUltimaModificacion { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }

        public virtual AspNetUsers usuarioUltimaModificacion { get; set; }

        public virtual ICollection<FacturaEstadoFactura> estados { get; set; }

        public virtual Storage archivo { get; set; }

        //public virtual DocumentoCompromiso documentoCompromiso { get; set; }

        public bool softDelete { get; set; }
    }

    public enum TipoFactura
    {
        [Display(Name = "OTIC")] OTIC,
        [Display(Name = "SENCE")] SENCE,
        [Display(Name = "Costo Empresa")] Costo_Empresa,
        [Display(Name = "Adicional")] Adicional
    }
}