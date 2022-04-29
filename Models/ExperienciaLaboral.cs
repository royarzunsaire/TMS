using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class ExperienciaLaboral
    {
        [Key]
        public int idExperienciaLaboral { get; set; }

        [DisplayName("Fecha de Inicio")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo Fecha de Inicio es obligatorio")]
        public DateTime? fechaInicio { get; set; }

        [DisplayName("Fecha de Término")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo Fecha de Término es obligatorio")]
        public DateTime? fechaTermino { get; set; }

        [DisplayName("Cargo")]
        [StringLength(50, ErrorMessage = "El campo Cargo puede tener entre 2 y 50 caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo Cargo es obligatorio")]
        public string cargo { get; set; }

        [DisplayName("Empresa")]
        [StringLength(50, ErrorMessage = "El campo Empresa puede tener entre 2 y 50 caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo Empresa es obligatorio")]
        public string empresa { get; set; }

        [DisplayName("Faena")]
        [StringLength(50, ErrorMessage = "El campo Faena puede tener máximo 50 caracteres", MinimumLength = 0)]
        public string faena { get; set; }

        public virtual Storage documento { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }

        //TODO: DOCUMENTO
    }
}