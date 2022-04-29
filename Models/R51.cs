using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("R51")]

    public class R51
    {
        [Key]
        public int idR51 { get; set; }
        public int idCurso { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre Curso")]
        public string nombreCurso { get; set; }
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre Diploma")]
        public string nombreDiploma { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Ciudad")]
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string observacion { get; set; }
        public string userEstado { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime fechaCreacion { get; set; }
        public virtual Curso Curso { get; set; }
        public virtual AspNetUsers userCreador { get; set; }
        public ICollection<R51_Checklist> R51_Checklists { get; set; }

        public bool softDelete { get; set; }

    }
}