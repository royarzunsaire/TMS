using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Relator
    {
        [Key]
        public int idRelator { get; set; }

        public virtual Contacto contacto { get; set; }

        public virtual DatosBancarios datosBancarios { get; set; }

        public virtual ICollection<TituloCurricular> tituloCurricular { get; set; }

        public virtual ICollection<ExperienciaLaboral> experienciaLaboral { get; set; }

        public virtual ICollection<RelatorCurso> relatorCurso { get; set; }

        public virtual ICollection<RelatorCursoSolicitado> relatorCursoSolicitado { get; set; }

        public virtual ICollection<R16> r16 { get; set; }

        public virtual ICollection<R11> r11 { get; set; }

        public virtual ICollection<Comercializacion> confirmadoEnComercializaciones { get; set; }

        public virtual ICollection<R52> r52 { get; set; }

        public virtual ICollection<R19> r19 { get; set; }

        public virtual ICollection<R53> r53 { get; set; }

        [Display(Name = "Vinculado con SENCE")]
        public bool vinculadoSENCE { get; set; }

        public string urlStorageIdentification { get; set; }

        public DateTime? fechaVencimientoIdentificacion { get; set; }

        public string urlCartaAprobacion { get; set; }

        public bool? validarContenido { get; set; }

        public bool? validarMaterialCurso { get; set; }

        [Display(Name = "Imagen firma")]
        public virtual Storage imagenFirma { get; set; }

        [Display(Name = "Documento autorización")]
        public virtual Storage imagenDocumentoAutorizacion { get; set; }

        [Display(Name = "Imagen cédula")]
        public virtual Storage imagenCedula { get; set; }

        public DateTime fechaCreacion { get; set; }

        public string usuarioCreador { get; set; }

        public bool softDelete { get; set; }
    }
}