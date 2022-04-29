using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("Contacto")]
    public class Contacto
    {

        [Key]
        public int idContacto { get; set; }

        [Display(Name = "Nombres")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^([A-ZÑÁÉÍÓÚ]|[a-zñáéíóú]| )*$", ErrorMessage = "El campo {0} no es válido")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string nombres { get; set; }

        [Display(Name = "Apellido Paterno")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^([A-ZÑÁÉÍÓÚ]|[a-zñáéíóú]| )*$", ErrorMessage = "El campo {0} no es válido")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string apellidoPaterno { get; set; }

        [Display(Name = "Apellido Materno")]
        [RegularExpression(@"^([A-ZÑÁÉÍÓÚ]|[a-zñáéíóú]| )*$", ErrorMessage = "El campo {0} no es válido")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string apellidoMaterno { get; set; }

        [Display(Name = "Sin RUT")]
        public bool? sinRut { get; set; }

        [Display(Name = "RUN")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(13)]
        public string run { get; set; }

        [NotMapped]
        public string runCompleto { get { return run; } }

        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]+$", ErrorMessage = "El campo {0} no es válido")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string correo { get; set; }

        [Display(Name = "Teléfono/Celular")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[+]?([0-9]| )*$", ErrorMessage = "El campo {0} no es válido")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string telefono { get; set; }

        [Display(Name = "Firma url")]
        [DataType(DataType.Url)]
        public string urlFirma { get; set; }

        // public Cliente Cliente { get; set;}

        //public int idCliente { get; set; }
        [NotMapped]
        [Display(Name = "Nombre")]
        public string nombreCompleto
        {
            get
            {
                return nombres + " " + apellidoPaterno + " " + apellidoMaterno;
            }
        }

        [Display(Name = "Vigente")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public Boolean vigente { get; set; }

        public string usuarioCreador { get; set; }

        public DateTime fechaCreacion { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime? fechaNacimiento { get; set; }

        [Display(Name = "Dirección")]
        [StringLength(250, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 0)]
        public string direccion { get; set; }

        [Display(Name = "Estado Civil")]
        [StringLength(50, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 0)]
        public string estadoCivil { get; set; }

        public virtual ICollection<ClienteContacto> clienteContactos { get; set; }

        public virtual ICollection<ClienteContactoCotizacion> clienteContactoCotizacion { get; set; }

        public virtual ICollection<RespuestasContestadasFormulario> respuestasEncuesta { get; set; }

        public virtual AspNetUsers usuario { get; set; }

        public bool softDelete { get; set; }

        [Display(Name = "Tipo")]
        public TipoContacto tipoContacto { get; set; }

        public string idUsuarioMoodle { get; set; }
    }

    public enum TipoContacto
    {
        Cliente,
        Relator,
        Participante
    }
}