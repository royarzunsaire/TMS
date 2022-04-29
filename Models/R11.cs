using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("R11")]

    public class R11
    {
        [Key]
        public int idR11 { get; set; }

        public int idCurso { get; set; }

        [Display(Name = "Nombre Curso")]
        public string nombreCurso { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Fundamentación Técnica")]
        public string fundamentacionTecnica { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Objetivo General")]
        public string objetivoGeneral { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Cantidad de Personas")]
        public int cantPersona { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Población Objetivo")]
        public string poblacionObjetivo { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Requisitos de Ingreso")]
        public string requisitosIngreso { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Técnicas Metodológicas")]
        public string tecnicaMetodologica { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Material Didáctico")]
        public string materialDidactico { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Material didáctico a entregar a los participantes")]
        public string materialEntregable { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Requisitos Legales, Normativos, reglamentarios que apliquen según tipo de curso")]
        public string requisitosReglamentarios { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        //[Display(Name = "Nombre Módulo")]
        //public string nombreModulo { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        //[Display(Name = "Instructor")]
        //public string instructor { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Requisitos Técnicos (Requisitos para evaluar aprendizaje)")]
        public string requisitosTecnicos { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Requisitos Técnicos de Relatores que entregan sus conocimientos en sala")]
        public string requisitosTecnicosRelatores { get; set; }

        [Column(TypeName = "datetime2")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaCreacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Horas Teóricas")]
        public double horasTeoricas { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Horas Prácticas")]
        public double horasPracticas { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Meses Duración Vigencia")]
        public int mesesDuracionVigencia { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Column(TypeName = "datetime2")]
        [Display(Name = "Fecha Caducidad")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaCaducidad { get; set; }

        [Display(Name = "Código SENCE")]
        public string codigoSence { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int idCategoria { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Diploma Aprobación")]
        public int diplomaAprobacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Diploma Participación")]
        public int DiplomaParticipacion { get; set; }

        public virtual ICollection<ContenidoEspecificoR11> conteidoEspecifico { get; set; }

        public virtual ICollection<EscolaridadR11> escolaridadR11 { get; set; }

        public virtual Relator relator { get; set; }

        public bool softDelete { get; set; }
    }
}