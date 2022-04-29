using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class UrlMaterialCurso
    {
        [Key]
        public int idUrlMaterialCurso { get; set; }

        [StringLength(999, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 0)]
        public string url { get; set; }

        [StringLength(250, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 0)]
        public string descripcion { get; set; }
    }
}