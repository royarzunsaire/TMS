using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class LugarAlmuerzo
    {
        [Key]
        [Required(ErrorMessage = "El campo Lugar de Almuerzo es obligatorio")]
        public int idLugarAlmuerzo { get; set; }

        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }

        public bool softDelete { get; set; }
    }
}