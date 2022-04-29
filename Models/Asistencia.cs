using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Asistencia")]
    public class Asistencia
    {
        [Key]
        public int idAsistencia { get; set; }

        //public int idParticipante { get; set; }
        //public int idBloque { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public bool asistio { get; set; }
        public string descripcion { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime fecha { get; set; }

        public virtual Participante participante { get; set; }
        public virtual Bloque bloque { get; set; }
    }
}