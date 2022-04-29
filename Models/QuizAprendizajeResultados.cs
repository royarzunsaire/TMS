using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SGC.Models
{
    public class QuizAprendizajeResultados
    {
        [Key]
        public virtual int idQuizAprendizajeResultados { get; set; }
        public virtual string resultado { get; set; }
        public virtual string tipoAprendizaje { get; set; }
        public virtual string descripcion { get; set; }
    }
}