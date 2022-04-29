using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Ciudad")]

    public class Ciudad
    {
        [Key]
        [Required(ErrorMessage = "El campo Ciudad es obligatorio")]
        public int idCiudad { get; set; }
        [Display(Name = "Ciudades")]
        public string nombreCiudad { get; set; }
        public int idPais { get; set; }
        public virtual Pais pais { get; set; }
    }
}