using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class TituloCurricular
    {
        [Key]
        public int idTituloCurricular { get; set; }

        [DisplayName("Tipo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public TipoTitulo nombreTitulo { get; set; }

        [DisplayName("Descripción")]
        [StringLength(250, ErrorMessage = "El campo Descripción puede tener entre 2 y 250 caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo Descripción es obligatorio")]
        public string descripcion { get; set; }

        [DisplayName("Institución")]
        [StringLength(50, ErrorMessage = "El campo Institución puede tener entre 2 y 50 caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo Institución es obligatorio")]
        public string institucion { get; set; }

        [DisplayName("Año")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "El campo Año no es válido")]
        [StringLength(4, ErrorMessage = "El campo Año debe tener 4 caracteres", MinimumLength = 4)]
        [Required(ErrorMessage = "El campo Año es obligatorio")]
        public string fecha { get; set; }

        public virtual Storage documento { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }
    }

    public enum TipoTitulo
    {
        [Display(Name = "Estudios Universitarios")] UNIVERSITARIO,
        [Display(Name = "Estudios Técnicos")] TECNICO,
        [Display(Name = "Cursos de capacitación")] CAPACITACION
    }
}