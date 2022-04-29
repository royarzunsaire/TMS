using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class PostCurso
    {
        [Key]
        public int idPostCurso { get; set; }

        [Display(Name = "Correo enviado a cliente")]
        public bool mailClient { get; set; }
   
        public virtual AspNetUsers creadorMailClient { get; set; }
        [Display(Name = "Fecha envío de correo al cliente")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaMailClient { get; set; }

        [Display(Name = "Información revisada")]
        public bool infoCheck { get; set; }
        public virtual AspNetUsers creadorInfoCheck { get; set; }
        [Display(Name = "Fecha de chequeo de la Información")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaInfoCheck { get; set; }

        [Display(Name = "Credenciales Listas")]
        public bool credReady { get; set; }
        public virtual AspNetUsers creadorCredReady { get; set; }
        [Display(Name = "Fecha de disponibilidad de credenciales")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaCredReady { get; set; }

        [Display(Name = "Declaracion Jurada OTEC emitida / R24")]
        public bool djo { get; set; }
        public virtual AspNetUsers creadorDjo { get; set; }
        [Display(Name = "Fecha de DJO lista")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaDjo { get; set; }


        public virtual Comercializacion comercializacion { get; set; }
    }
}