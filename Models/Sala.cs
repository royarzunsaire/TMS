using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Sala
    {
        [Key]
        [Required(ErrorMessage = "El campo Sala es obligatorio")]
        public int idSala { get; set; }

        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }

        public bool softDelete { get; set; }
    }
}