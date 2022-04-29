using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class ConfiguracionUsuarioRelator
    {
        [Key]
        public int idConfiguracionUsuarioRelator { get; set; }

        [Display(Name = "Contraseña")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{6,}$", ErrorMessage = "Las contraseñas deben tener al menos un carácter que no sea una letra ni un dígito. Las contraseñas deben tener al menos un dígito ('0'-'9'). Las contraseñas deben tener al menos una letra en mayúscula ('A'-'Z').")]
        public string contrasenia { get; set; }

        public virtual AspNetRoles rol { get; set; }
    }
}