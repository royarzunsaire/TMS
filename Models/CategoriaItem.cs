using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("CategoriaItem")]
    public class CategoriaItem
    {
        [Key]
        public int idCategoriaItem { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre")]
        public string nombre { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }

        public int vigencia { get; set; }
    }
}