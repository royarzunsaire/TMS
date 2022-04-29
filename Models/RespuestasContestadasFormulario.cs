using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("RespuestasContestadasFormulario")]

    public class RespuestasContestadasFormulario
    {
        [Key]
        public int idRespuestasContestadasFormulario { get; set; }

        //public int idRespuestasFormulario { get; set; }

        public string respuesta { get; set; }

        //public int idContacto { get; set; }

        public DateTime fecha { get; set; }

        public virtual PreguntasFormulario pregunta { get; set; }

        public virtual Contacto contacto { get; set; }

        public virtual Encuesta encuesta { get; set; }

        public virtual RespuestasFormulario respuestaFormulario { get; set; }

        public RespuestasContestadasFormulario()
        {
            fecha = DateTime.Now;
        }
    }
}