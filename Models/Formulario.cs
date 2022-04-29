using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Formulario")]
    public class Formulario
    {
        [Key]
        public int idFormulario { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        [StringLength(100, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Descripción / Introducción")]
        [DataType(DataType.MultilineText)]
        [StringLength(999, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string descripcion { get; set; }

        //public int idTipoFormulario { get; set; }

        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        //[DataType(DataType.Date)]
        //[Column(TypeName = "Date")]
        public DateTime fechaCreacion { get; set; }

        public DateTime fechaUltimaModificacion { get; set; }

        //[Display(Name = "Usuario Creador")]
        public virtual AspNetUsers usuarioCreacion { get; set; }

        public virtual AspNetUsers usuarioUltimaModificacion { get; set; }

        public bool linkPublico { get; set; }

        public bool softDelete { get; set; }

        //public bool valido { get; set; }

        public virtual ICollection<PreguntasFormulario> preguntasFormularios { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo Formulario")]
        public TipoFormulario? tipoFormulario { get; set; }
    }

    public enum TipoFormulario
    {
        R19,
        Evaluacion,
        R52,
        R16,
        R43,
        R43_E,
        R53
    }
}