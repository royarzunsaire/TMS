using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Calendarizacion
    {
        [Key]
        public int idCalendarizacion { get; set; }

        [DisplayName("Detalle")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres", MinimumLength = 0)]
        public string detalle { get; set; }

        [DisplayName("Inicio del Periodo")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime inicioPeriodo { get; set; }

        [DisplayName("Fin del Periodo")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime finPeriopdo { get; set; }

        public virtual ICollection<CalendarizacionAbierta> calendarizacionesAbiertas { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }

        [Display(Name = "Sucursal")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public virtual Sucursal sucursal { get; set; }
    }
}