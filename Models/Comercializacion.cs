using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class Comercializacion
    {
        [Key]
        public int idComercializacion { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaInicio { get; set; }

        [Display(Name = "Fecha de Término")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaTermino { get; set; }

        [Display(Name = "Observación")]
        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string observacion { get; set; }

        [Display(Name = "Observación")]
        [DataType(DataType.MultilineText)]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string observacionFacturacion { get; set; }

        [Display(Name = "SENCE NET")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string senceNet { get; set; }

        public string idGrupoMoodle { get; set; }

        public string rutaLogo { get; set; }

        public int? aporteEmpresa { get; set; }

        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        public int? valorFinal { get; set; }

        public int? valorCostoEmpresa { get; set; }

        public int? valorCostoSence { get; set; }

        [Display(Name = "Otro")]
        [DataType(DataType.MultilineText)]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener máximo {1} caracteres")]
        public string tipoDocumentoOtro { get; set; }

        [Display(Name = "Meses Vigencia Credenciales")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        //public DateTime vigenciaCredenciales { get; set; }
        public int vigenciaCredenciales { get; set; }

        public virtual Cotizacion_R13 cotizacion { get; set; }

        public virtual ICollection<ComercializacionEstadoComercializacion> comercializacionEstadoComercializacion { get; set; }

        public virtual Ciudad ciudad { get; set; }

        public virtual ICollection<Bloque> bloques { get; set; }

        public virtual ICollection<Participante> participantes { get; set; }

        //public virtual ICollection<Pago> pagos { get; set; }

        public virtual ICollection<RelatorCurso> relatoresCursos { get; set; }

        public virtual ICollection<Evaluacion> evaluaciones { get; set; }

        public virtual ICollection<R19> r19 { get; set; }

        public virtual ICollection<R52> r52 { get; set; }

        public virtual ICollection<HistorialComercializacion> historialComercializacion { get; set; }

        public bool vigencia { get; set; }

        public virtual AspNetUsers usuarioCreador { get; set; }

        public virtual AspNetUsers usuarioUltimaEdicion { get; set; }

        public virtual Storage r24 { get; set; }

        public virtual ICollection<Relator> relatoresConfirmados { get; set; }

        public virtual ICollection<R53> r53 { get; set; }

        public DateTime fechaUltimaEdicion { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaCreacion { get; set; }

        public bool softDelete { get; set; }

        [DataType(DataType.Currency)]
        public int descuento { get; set; }

        public virtual AspNetUsers usuarioCreadorDescuento { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime fechaDescuento { get; set; }

        public virtual List<Observacion> observaciones { get; set; }

        [Display(Name = "Comentario Instructor")]
        [DataType(DataType.MultilineText)]
        public string comentarioInstructor { get; set; }

        [Display(Name = "Comentario OTEC")]
        [DataType(DataType.MultilineText)]
        public string comentarioOtec { get; set; }

        [Display(Name = "Diploma con Descriptor de Contenidos")]
        public bool R23ConDescriptorContenidos { get; set; }

        [Display(Name = "Descargar documentos de los clientes por el comercial")]
        public bool clientDownload { get; set; }
        [Display(Name = "Descargar documentos de los clientes por facturación")]
        public bool clientFactura { get; set; }
        public Faena faena { get; set; }


        //public Sucursal sucursal { get; set; }

        //public DateTime fechaCotizacion { get; set; }

        //public CondicionesPago condicionesPago { get; set; }

        //public Cliente cliente { get; set; }

        //public Contacto representanteCliente { get; set; }

        //public CalendarizacionAbierta modalidadAbierto { get; set; }

        //public TipoCurso tipoCurso { get; set; }

        //public string nombreDiploma { get; set; }

        //public Curso curso { get; set; }

        //public Contacto dirigidoA { get; set; }

        //public int condicionesComerciales { get; set; }

        //public string usuarioVendedor { get; set; }

        //public bool esBorrador { get; set; }

        //public ICollection<ComercializacionDocClienteEspecial> comercializacionDocClientesEspeciales { get; set; }

        //public Facturacion facturacion { get; set; }

        //public string Lugar { get; set; }

        //public int cantParticipantes { get; set; }

        //public string OC { get; set; }

        //public TipoVenta tipoVenta { get; set; }

        //public TipoDocumento tipoDocumento { get; set; }
    }

    //public enum TipoCurso {
    //    [Display(Name = "Curso")] Curso,
    //    [Display(Name = "Recertificación")] Recertificacion,
    //    [Display(Name = "Pre-contrato")] PreContrato,
    //    [Display(Name = "Social")] Social,
    //    [Display(Name = "Duplicado Credencial")] DuplicadoCredencial
    //}

    //public enum TipoVenta
    //{
    //    [Display(Name = "OTIC")] Otic,
    //    [Display(Name = "SENCE")] Sence,
    //    [Display(Name = "Costo Empresa")] CostoEmpresa
    //}

    //public enum TipoDocumento
    //{
    //    [Display(Name = "Orden de compra")] OC,
    //    [Display(Name = "MIGO")] Migo,
    //    [Display(Name = "HES")] HES,
    //    [Display(Name = "Otro")] Otro
    //}
}