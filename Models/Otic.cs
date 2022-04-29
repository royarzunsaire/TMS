using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Otic
    {
        [Key]
        public int idOtic { get; set; }

        [DisplayName("RUT")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string rut { get; set; }

        [DisplayName("Nombre")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }

        [DisplayName("Dirección")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string direccion { get; set; }

        [Display(Name = "Teléfono")]
        [RegularExpression(@"^[+]?([0-9]| )*$", ErrorMessage = "El campo {0} no es válido")]
        [StringLength(20, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string telefono { get; set; }

        [Display(Name = "Contacto")]
        public Contacto contacto { get; set; }

        public string usuarioCreador { get; set; }

        public DateTime fechaCreacion { get; set; }

        public bool softDelete { get; set; }
    }
}