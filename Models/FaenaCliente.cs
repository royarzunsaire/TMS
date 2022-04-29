using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("FaenaCliente")]

    public class FaenaCliente
    {
        [Key]
        [Required(ErrorMessage = "El campo faena es obligatorio")]
        public int idFaenaCliente { get; set; }
        
        [Required]
        public Cliente cliente { get; set; }
       
        [Required]
        public Faena faena { get; set; }

    }
}