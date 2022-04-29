using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("ParametrosMoodle")]
    public class ParametrosMoodle
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Nombre de Usuario Moodle")]
        public string username { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Contraseña Moodle")]
        public string password { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "URL Moodle")]
        public string urlMoodle { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Web Service")]
        public string service { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Contraseña Usuarios")]
        public string contraseñaUsuarios { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "ID Rol Estudiante")]
        public string idRolEstudiante { get; set; }
    }
}