using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Checklist")]

    public class Checklist
    {
        [Key]
        public int idChecklist { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Detalle")]
        public string detalle { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Categoria")]
        public string valor { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Vigencia")]
        public int vigencia { get; set; }
        public ICollection<R51_Checklist> R51_Checklists { get; set; }

        public bool softDelete { get; set; }
    }
}