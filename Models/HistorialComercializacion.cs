using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class HistorialComercializacion
    {
        [Key]
        public int idHistorialComercializacion { get; set; }

        public virtual Comercializacion comercializacion { get; set; }

        [Display(Name = "Usuario Modificador")]
        public virtual AspNetUsers usuarioModificacion { get; set; }

        [Display(Name = "Fecha de Modificación")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}")]
        public DateTime fechaModificacion { get; set; }

        // -------------------- Empresa --------------------
        [Display(Name = "Nombre Empresa")]
        public string nombreEmpresa { get; set; }
        [Display(Name = "RUT")]
        public string rutEmpresa { get; set; }
        [Display(Name = "Razón Social")]
        public string razonSocialEmpresa { get; set; }
        [Display(Name = "Giro")]
        public string giroEmpresa { get; set; }
        [Display(Name = "Contacto")]
        public string contactoEmpresa { get; set; }
        [Display(Name = "Teléfono Corporativo")]
        public string telefonoCorporativoEmpresa { get; set; }
        [Display(Name = "Contacto Encargado Pagos")]
        public string contactoEncargadoPagosEmpresa { get; set; }
        [Display(Name = "Dirección")]
        public string direccionEmpresa { get; set; }

        // ------------------- Curso ----------------------
        [Display(Name = "Modalidad")]
        public string modalidadCurso { get; set; }
        [Display(Name = "Tipo")]
        public string tipoCurso { get; set; }
        [Display(Name = "Nombre Curso")]
        public string nombreCurso { get; set; }
        [Display(Name = "Tipo de Ejecución")]
        public string tipoEjecucionCurso { get; set; }
        [Display(Name = "Nombre Diploma")]
        public string nombreDiplomaCurso { get; set; }
        [Display(Name = "Sin Código SENCE")]
        public bool sinCodigoSenceCurso { get; set; }
        [Display(Name = "Código SENCE")]
        public string codigoSenceCurso { get; set; }
        [Display(Name = "Código Consolidacion")]
        public string codigoConsolidacionCurso { get; set; }

        // -------------- comercializacion -----------------
        [Display(Name = "Código Cotización")]
        public string codigoCotizacionComercializacion { get; set; }
        [Display(Name = "Lider Comercial")]
        public string liderComercialComercializacion { get; set; }
        [Display(Name = "Estado")]
        public string estadoComercializacion { get; set; }
        [Display(Name = "Fecha de Inicio")]
        public string fechaInicioComercializacion { get; set; }
        [Display(Name = "Fecha de Término")]
        public string fechaTerminoComercializacion { get; set; }
        [Display(Name = "Meses Vigencia Credenciales")]
        public string mesesVigenciacredencialesComercializacion { get; set; }
        [Display(Name = "Ciudad")]
        public string ciudadComercializacion { get; set; }
        [Display(Name = "SENCE NET")]
        public string senceNetComercializacion { get; set; }
        [Display(Name = "Lugar de Realización (Cotización)")]
        public string lugarRealizacionComercializacion { get; set; }
        [Display(Name = "Proceso Practico")]
        public bool esProcesoPractico { get; set; }
        [Display(Name = "Diploma con Descriptor de Contenidos")]
        public bool diplomaDescriptorContenidosComercializacion { get; set; }
        [Display(Name = "Observación")]
        public string observacionComercializacion { get; set; }

        public virtual ICollection<TipoVentaHistorialComercializacion> tiposVenta { get; set; }
        public virtual ICollection<RelatorHistorialComercializacion> relatores { get; set; }

        // ------------------ Costo ----------------------
        [Display(Name = "Cantidad participantes")]
        public string cantidadParticipantesCosto { get; set; }
        [Display(Name = "Condiciones de Pago")]
        public string condicionesPagoCosto { get; set; }
        [Display(Name = "Tipo menu")]
        public string tipoMenuCosto { get; set; }
        [Display(Name = "Total")]
        public int totalCosto { get; set; }
        [Display(Name = "Descuento")]
        public int descuentoCosto { get; set; }
        [Display(Name = "Usuario Creador Descuento")]
        public string usuarioCreadorDescuentoCosto { get; set; }
        [Display(Name = "Fecha Descuento")]
        public string fechaDescuentoCosto { get; set; }

        public virtual ICollection<CostoHistorialComercializacion> costos { get; set; }
    }
}