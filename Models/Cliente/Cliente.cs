using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGC.Models
{
    [Table("Cliente")]

    public class Cliente
    {
        [Key]
        public int idCliente { get; set; }
        [DisplayName("Cliente")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombreEmpresa { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string razonSocial { get; set; }
        [MaxLength(13)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string rut { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        //public  int idGiro { get; set; }
        //public virtual Giro giros { get; set; }
        public string telefonoCorporativo { get; set; }
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        //public int idEncargadoPagos { get; set; }
        //public virtual Contacto encargadoPagos { get; set; }
        //EncargadoPagos icollection<ClienteContacto>
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string direccion { get; set; }
        //public int idRepresentanteLegal { get; set; }

        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        //public int idTiposDocumentosPago { get; set; }
        //representante icollection<ClienteContacto> representante
        public string portalNotasExterno { get; set; }
        public string nombrePortal { get; set; }
        public string descEspecial { get; set; }
        public DateTime fechaDescEspecial { get; set; }
        public string creadorDescEspecial { get; set; }
        public bool postVenta { get; set; }
        public bool encuestaSatisfaccion { get; set; }
        public bool encuestaSatisfaccionElerning { get; set; }
        public DateTime fechaAlertaEncuestaSatisfaccion { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public SituacionComercial situacionComercial { get; set; }
        public DateTime fechaCreacion { get; set; }
        public string usuarioCreador { get; set; }
        public bool softDelete { get; set; }

        public int? idMandante { get; set; }
        public virtual Mandante mandante { get; set; }
        public virtual ICollection<ClienteContacto> clienteContactos { get; set; }
        public virtual ICollection<RepresentanteLegal> representanteLegals { get; set; }
        public virtual ICollection<EncargadoPago> encargadoPagos { get; set; }
        public virtual ICollection<ClienteGiro> clienteGiros { get; set; }
        public virtual ICollection<ClienteTipoDocumentosPago> clienteTipoDocumentosPagos { get; set; }
        public virtual ICollection<ClienteContactoCotizacion> clienteContactoCotizacion { get; set; }
        public virtual ICollection<R43> r43 { get; set; }
        public virtual ICollection<Cotizacion_R13> cotizaciones { get; set; }
        public virtual ICollection<ClienteUsuario> usuariosAsignados { get; set; }

        [Display(Name = "Última fecha de envío")]
        public DateTime ultimaFechaEnvioCorreo { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Frecuencia de envío de Correo en días")]

        public int cantDiasEnvioCorreo { get; set; }
        [Display(Name = "Enviar correo en las 24 horas")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]

        public bool enviarCapacitacionRealizadaEnvioCorreo { get; set; }
        [Display(Name = "Enviar correo resumen")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]

        public bool enviarResumenEnvioCorreo { get; set; }

        public Cliente()
        {
            cantDiasEnvioCorreo = 7;
            enviarCapacitacionRealizadaEnvioCorreo = false;
            enviarResumenEnvioCorreo = true;
            ultimaFechaEnvioCorreo = DateTime.Now;
        }

    }
    public enum SituacionComercial
    {
        Vigente,
        Pendiente


    }
}