using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Pago")]
    public class Pago
    {
        [Key]
        public int idPago { get; set; }

        [DisplayName("Tipo de Venta")]
        public TipoPago tipoPago { get; set; }

        //[DisplayName("Valor")]
        //public int valor { get; set; }

        //[DisplayName("Fecha de Pago")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        //public DateTime? fechaPago { get; set; }

        //public string urlStorage { get; set; }

        //[DisplayName("Observación")]
        //[StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        //public string observacion { get; set; }

        public virtual Otic otic { get; set; }
    }

    public enum TipoPago
    {
        [Display(Name = "OTIC")] Otic,
        [Display(Name = "SENCE")] Sence,
        [Display(Name = "Costo Empresa")] CostoEmpresa
    }
}