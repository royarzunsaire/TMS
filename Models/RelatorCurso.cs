using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class RelatorCurso
    {
        [Required(ErrorMessage = "El campo Curso es obligatorio")]
        public virtual int idCurso { get; set; }

        [Required(ErrorMessage = "El campo Relator es obligatorio")]
        public virtual int idRelator { get; set; }

        public virtual Curso curso { get; set; }

        public virtual Relator relator { get; set; }

        [DisplayName("Valido SENCE")]
        public bool validoSence { get; set; }

        [DisplayName("REUF")]
        public bool reuf { get; set; }

        public virtual ICollection<Comercializacion> comercializaciones { get; set; }

        public string usuarioCreador { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaCreacion { get; set; }

        public bool softDelete { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime? fechaValidoSence { get; set; }
    }
}