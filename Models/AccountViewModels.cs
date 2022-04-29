using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        //[Display(Name = "Email")]
        //[EmailAddress]
        //public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Recordar?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        //[Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(100, ErrorMessage = "La {0} deberia tener como minimo {2} de largo.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Add the new address properties:
        [Display(Name = "Direccion")]
        public string Address { get; set; }

        [Display(Name = "Ciudad")]
        public string City { get; set; }

        [Display(Name = "Departamento")]
        public string State { get; set; }

        // Use a sensible display name for views:
        [Display(Name = "Codigo Postal")]
        public string PostalCode { get; set; }


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

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}