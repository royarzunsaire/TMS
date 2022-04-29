using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Usuario
    {
        [Key]
        public int idUsuario { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(25)]
        public string Name { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}