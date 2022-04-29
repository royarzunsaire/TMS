namespace SGC.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CustomPermission")]
    public partial class CustomPermission
    {
        public int CustomPermissionID { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(128)]
        public string UserID { get; set; }

        public int MenuID { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual AspNetUsers AspNetUsers { get; set; }
    }
}
