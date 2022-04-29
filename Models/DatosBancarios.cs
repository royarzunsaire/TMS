using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("DatosBancarios")]

    public class DatosBancarios
    {
        [Key]
        public int idDatosBancarios { get; set; }

        [DisplayName("Número de Cuenta")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string numeroCuenta { get; set; }

        [DisplayName("Banco")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombreBanco { get; set; }

        [DisplayName("Tipo de Cuenta")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public TipoCuenta tipoCuenta { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }
    }

    public enum TipoCuenta
    {
        [Display(Name = "Cuenta Vista")] VISTA,
        [Display(Name = "Cuenta Corriente")] CTA_CTE,
        [Display(Name = "Cuenta Ahorro")] AHORRO
    }

    //TODO: Banco debe ser una clase con mantenedor propio
}