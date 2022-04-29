using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Item")]
    public class Item
    {
        [Key]
        public int idItem { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int valor { get; set; }
        public string descripcion { get; set; }
        public int idCategoriaItem { get; set; }
        public virtual CategoriaItem categoria { get; set; }
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }
        public int vigencia { get; set; }

    }

}