using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SGC.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        // Add the Address Info:
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        // Use a sensible display name for views:
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }


        public IEnumerable<SelectListItem> RolesList { get; set; }


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

        [Display(Name = "RUN")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(13)]
        public string run { get; set; }

        [Display(Name = "Teléfono/Celular")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[+]?([0-9]| )*$", ErrorMessage = "El campo {0} no es válido")]
        [StringLength(20, ErrorMessage = "El campo {0} puede tener entre {2} y {1} caracteres", MinimumLength = 2)]
        public string telefono { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime? fechaNacimiento { get; set; }
    }
}