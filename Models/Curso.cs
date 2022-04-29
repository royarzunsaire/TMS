using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("Curso")]
    public class Curso
    {
        [Key]
        [Display(Name = "Curso")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int idCurso { get; set; }

        public string codigoCurso { get; set; }

        [Display(Name = "Nombre Curso")]
        public string nombreCurso { get; set; }

        public string idCursoMoodle { get; set; }

        public virtual ICollection<MaterialCurso> materialCurso { get; set; }

        public virtual ICollection<UrlMaterialCurso> urlMaterialCurso { get; set; }

        public virtual ICollection<RelatorCurso> relatorCurso { get; set; }

        public virtual ICollection<RelatorCursoSolicitado> relatorCursoSolicitado { get; set; }

        public virtual ICollection<Evaluacion> evaluaciones { get; set; }

        public bool softDelete { get; set; }

        [Display(Name = "Tipo de Ejecución")]
        public TipoEjecucion tipoEjecucion { get; set; }

        public bool materialCompleto { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime? fechaValidacionMaterial { get; set; }

        public virtual AspNetUsers usuarioValidacionMaterial { get; set; }
    }

    public enum TipoEjecucion
    {
        Presencial,
        Elearning_Sincrono,
        Elearning_Asincrono,
        Recertificacion,
        Recertificacion_Sincronica,
        Recertificacion_Asincronica
    }
}