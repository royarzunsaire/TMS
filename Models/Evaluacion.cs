using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Evaluacion
    {
        [Key]
        public int idEvaluacion { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombre { get; set; }

        [Display(Name = "Modalidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public TipoEvaluacion tipo { get; set; }

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public CategoriaEvaluacion categoria { get; set; }

        public string idQuizMoodle { get; set; }

        public virtual Formulario formulario { get; set; }

        public virtual Curso curso { get; set; }

        public virtual ICollection<Comercializacion> comercializacion { get; set; }

        public DateTime fechaCreacion { get; set; }
        public DateTime fechaModificacion { get; set; }
        public virtual AspNetUsers usuarioCreacion { get; set; }
        public virtual AspNetUsers usuarioModificacion { get; set; }
        public bool softDelete { get; set; }
    }

    public enum TipoEvaluacion
    {
        Cuestionario,
        Presencial,
        Moodle
    }

    public enum CategoriaEvaluacion
    {
        Diagnostico,
        Practico,
        Teorico
    }
}