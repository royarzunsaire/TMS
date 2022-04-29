namespace SGC.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Permission")]
    public partial class Permission
    {
        public int PermissionID { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(128)]
        public string RoleID { get; set; }

        public int MenuID { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual AspNetRoles AspNetRoles { get; set; }
    }
}
