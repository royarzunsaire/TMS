using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Participante
    {
        [Key]
        public int idParticipante { get; set; }

        //public int tonelaje { get; set; }
        public DateTime timestart { get; set; }
        [Display(Name = "Matrícula ")]
        public DateTime timeend { get; set; }

        public virtual Comercializacion comercializacion { get; set; }

        public virtual Contacto contacto { get; set; }

        public virtual IList<Notas> notas { get; set; }
        public virtual IList<Attempts> attempts { get; set; }
        public virtual IList<AttemptsQuizUser> attemptsQuizUsers { get; set; }
        public virtual IList<Asistencia> asistencia { get; set; }

        public virtual Storage credenciales { get; set; }

        public bool agregadoAGrupo { get; set; }
        public bool correoEnviado { get; set; }
        public bool conSence { get; set; }
        public bool conDeclaracionJuradaPersona { get; set; }

        //public int? idUserMoodle { get; set; }
        //public string usernameMoodle { get; set; }
        //public string passwordMoodle { get; set; }

    }
}