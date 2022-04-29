using System.ComponentModel.DataAnnotations;

namespace SGC.Models
{
    public class ViewModelRestablecerContraseniaUsuario
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string idUsuario { get; set; }

        [Display(Name = "Usuario")]
        public string userName { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string newPassword { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        public string newPasswordConfirm { get; set; }
    }
}