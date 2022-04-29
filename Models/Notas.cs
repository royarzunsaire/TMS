using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Notas")]
    public class Notas
    {
        [Key]
        public int idNotas { get; set; }
        
        public int idParticipante { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nota")]
        public string nota { get; set; }

        public double porcentaje { get; set; }

        [Display(Name = "Descripción")]
        public string descripcion { get; set; }

        public string idNotaMoodle { get; set; }

        public virtual Participante participante { get; set; }

        public virtual Evaluacion evaluacion { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public virtual DateTime fechaRealizacion { get; set; }

        public virtual ICollection<RespuestaEvaluacion> respuestas { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public virtual DateTime fechaIngresoManual { get; set; }
        public bool manual { get; set; }
        public virtual AspNetUsers usuarioIngreso { get; set; }
    }
}