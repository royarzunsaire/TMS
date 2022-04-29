using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class R06_ActaReunion
    {
        [Key]
        public int idR06 { get; set; }

        [Display(Name = "Creado por")]
        public virtual AspNetUsers usuarioCreador { get; set; }

        [Display(Name = "Creado en")]
        public DateTime dateCreation { get; set; }

        [Display(Name = "Modificado por")]
        public virtual AspNetUsers usuarioUltimaEdicion { get; set; }

        [Display(Name = "Modificado en")]
        public DateTime dateEdited { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fecha { get; set; }

        [Display(Name = "Hora de Inicio")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime horarioInicio { get; set; }

        [Display(Name = "Hora de Término")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime horarioTermino { get; set; }

        [Display(Name = "Temas tradados")]
        public string temasTratados { get; set; }

        [Display(Name = "Acuerdos")]
        public string acuerdos { get; set; }

        public ICollection<ParticipantesReunion> ParticipantesReunion { get; set; }

        public bool softDelete { get; set; }

    }
}