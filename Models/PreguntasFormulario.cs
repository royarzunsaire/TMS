using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("PreguntasFormulario")]
    public class PreguntasFormulario
    {
        [Key]
        public int idPreguntasFormulario { get; set; }

        //public int idFormulario { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Pregunta")]
        public string pregunta { get; set; }

        //public string urlImage { get; set; }

        public TipoPregunta tipo { get; set; }

        public int orden { get; set; }

        public virtual Formulario formulario { get; set; }

        public virtual List<RespuestasFormulario> respuestaFormulario { get; set; }

        public Storage imagen { get; set; }

        public bool obligatoria { get; set; }
    }

    public enum TipoPregunta
    {
        Abierta,
        Corta,
        Alternativa
    }
}