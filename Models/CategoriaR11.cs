using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("CategoriaR11")]
    public class CategoriaR11
    {
        [Key]
        public int idCategoria { get; set; }

        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string categoria { get; set; }

        [Display(Name = "Identificador")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string identificador { get; set; }

        [Display(Name = "Vigencia")]
        public int vigencia { get; set; }

        public ICollection<R11> r11 { get; set; }

        public bool softDelete { get; set; }
    }
}