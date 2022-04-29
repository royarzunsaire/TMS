using System;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Bloque
    {
        [Key]
        public int idBloque { get; set; }


        [Display(Name = "Hora de Inicio")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime horarioInicio { get; set; }

        [Display(Name = "Hora de Término")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime horarioTermino { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fecha { get; set; }

        public virtual Comercializacion comercializacion { get; set; }

        public virtual LugarAlmuerzo lugarAlmuerzo { get; set; }

        public virtual Sala sala { get; set; }

        public virtual Relator relator { get; set; }

        //------------new
        public virtual AspNetUsers coordinador { get; set; }

 

    }
}