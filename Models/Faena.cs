using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Faena")]

    public class Faena
    {
        [Key]
        [Required(ErrorMessage = "El campo faena es obligatorio")]
        public int idFaena { get; set; }
       
        [Required(ErrorMessage = "El campo faena es obligatorio")]
        [Display(Name = "Faena")]
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public bool softDelete { get; set; }
        

    }
}