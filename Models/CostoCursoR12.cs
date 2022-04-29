using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("CostoCursoR12")]
    public class CostoCursoR12
    {
        [Key]
        public int idCostoCursoR12 { get; set; }
        public int idCurso { get; set; }
        public virtual Curso curso { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime fechaCreacion { get; set; }

        public ICollection<CostoParticularCurso> costoParticularCurso { get; set; }

        public bool softDelete { get; set; }
    }
}