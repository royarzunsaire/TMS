using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Pais")]

    public class Pais
    {
        [Key]
        public int idPais { get; set; }
        [Display(Name = "Paises")]
        public string nombrePais { get; set; }
        public ICollection<Ciudad> Ciudad { get; set; }
    }
}