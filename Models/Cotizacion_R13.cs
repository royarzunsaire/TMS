using Foolproof;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Cotizacion_R13")]
    public class Cotizacion_R13
    {
        [Key]
        public int idCotizacion_R13 { get; set; }

        public int idCliente { get; set; }

        [Display(Name = "Código")]
        public string codigoCotizacion { get; set; }


        //Datos Cliente
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre Empresa")]
        public string nombreEmpresa { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Razón Social")]
        public string razonSocial { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Giro")]
        public string giro { get; set; }
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Teléfono Corporativo")]
        public string telefonoCorporativo { get; set; }

        //Datos Facturacion
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Contacto Encargado Pagos")]
        public int? contactoEncargadoPago { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Dirección")]
        public string direccion { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Contacto")]
        public int? contacto { get; set; }

        //Datos curso
        [RequiredIfFalse("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Modalidad")]
        public string modalidad { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo")]
        public string tipoCurso { get; set; }

        [RequiredIfFalse("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        public int? idCurso { get; set; }

        [RequiredIfFalse("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre Diploma")]
        public string nombreDiploma { get; set; }

        [Display(Name = "Código SENCE")]
        public string codigoSence { get; set; }

        [Display(Name = "Sin Código SENCE")]
        public string tieneCodigoSence { get; set; }

        [RequiredIfFalse("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Lugar Realización")]
        public string lugarRealizacion { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Column(TypeName = "datetime2")]
        [Display(Name = "Fecha Inicio")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime? fechaInicio { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Column(TypeName = "datetime2")]
        [Display(Name = "Fecha Término")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime? fechaTermino { get; set; }

        //Datos costos
        [RequiredIfFalse("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Cantidad Participantes")]
        public int? cantidadParticipante { get; set; }

        public int isAutoCotizacion { get; set; }

        [RequiredIfFalse("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Condiciones de Pago")]
        public string condicionesDePago { get; set; }

        [RequiredIfFalse("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo Menu")]
        public string tipoMenu { get; set; }

        [RequiredIfTrue("isValorUnico", ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Valor")]
        public int? valorUnico { get; set; }

        public bool isValorUnico { get; set; }

        public double horasCurso { get; set; }

        [Display(Name = "Meses Vigencia Credenciales")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int vigenciaCredenciales { get; set; }

        [Display(Name = "Fecha de Creación")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime fechaCreacion { get; set; }
        public virtual Cliente cliente { get; set; }
        public virtual Curso curso { get; set; }

        public ICollection<Costo> costo { get; set; }

        public virtual ICollection<DocumentoCompromiso> documentosCompromiso { get; set; }

        public virtual CalendarizacionAbierta calendarizacionAbierta { get; set; }

        public virtual ICollection<CotizacionAporteCapacitacion> cotizacionAporteCapacitacion { get; set; }

        [Display(Name = "Sucursal")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public virtual Sucursal sucursal { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }

        public bool softDelete { get; set; }

        public bool procesoPractico { get; set; }
        public Faena faena { get; set; }
    }
}