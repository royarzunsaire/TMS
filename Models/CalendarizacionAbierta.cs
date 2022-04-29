using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class CalendarizacionAbierta
    {
        [Key]
        public int idCalendarizacionAbierta { get; set; }

        [DisplayName("Curso")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public virtual Curso curso { get; set; }

        [DisplayName("Fecha de Inicio")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaInicio { get; set; }

        [DisplayName("Fecha de Término")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaTermino { get; set; }

        [DisplayName("Descripción")]
        [DataType(DataType.MultilineText)]
        [StringLength(999, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres", MinimumLength = 0)]
        public string descripcion { get; set; }

        [DisplayName("Color")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public ColorEvento colorEvento { get; set; }

        [DisplayName("Estado")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public Estado estado { get; set; }

        public string codigoConsolidacion { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }
    }

    public enum ColorEvento
    {
        [Display(Name = "Azul")] blue,
        [Display(Name = "Rojo")] red,
        [Display(Name = "Naranjo")] orange,
        [Display(Name = "Verde")] green
    }

    public enum Estado
    {
        [Display(Name = "Programado")] Programado,
        [Display(Name = "Incorporado")] Incorporado
    }
}