using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class FacturaEstadoFactura
    {
        [Key]
        public int idFacturaEstadoFactura { get; set; }

        public EstadoFactura estado { get; set; }

        public DateTime fechaCreacion { get; set; }

        public DateTime? fechaEstado { get; set; }

        public string Observacion { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }

        public virtual Factura factura { get; set; }
    }

    public enum EstadoFactura
    {
        [Display(Name = "No_Facturado")] No_Facturado,
        [Display(Name = "Facturado")] Facturado,
        [Display(Name = "Refacturado")] Refacturado,
        [Display(Name = "Pagado")] Pagado,
        [Display(Name = "Abonado")] Abonado,
        [Display(Name = "No Aplica")] No_Aplica
    }
}