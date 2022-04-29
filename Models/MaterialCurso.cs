using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class MaterialCurso
    {
        [Key]
        public int idMaterialCurso { get; set; }

        public virtual Storage archivo { get; set; }
    }
}