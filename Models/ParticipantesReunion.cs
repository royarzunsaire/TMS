using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class ParticipantesReunion
    {
        [Key]
        public int idParticipanteExterno { get; set; }
        public string nombre { get; set; }

        public string telefono { get; set; }

        public string empArea { get; set; }

        public string firma { get; set; }
        
      
        public virtual R06_ActaReunion R06 { get; set; }

        public virtual AspNetUsers idAspNetUser { get; set; }
    }
}