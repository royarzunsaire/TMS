using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Mandante")]
    public class Mandante
    {
        [Key]
        public int idMandante { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        public string nombreMandante { get; set; }

        [Display(Name = "RUT")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public virtual string rut { get; set; }

        [Display(Name = "Dirección")]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string direccion { get; set; }

        public int vigencia { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }

        public bool softDelete { get; set; }
    }
}