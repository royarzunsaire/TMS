namespace SGC.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Menu")]
    public partial class Menu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Menu()
        {
            CustomPermission = new HashSet<CustomPermission>();
            Permission = new HashSet<Permission>();
        }

        public int MenuID { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50)]
        public string DisplayName { get; set; }

        public virtual int ParentMenuID { get; set; }

        public int OrderNumber { get; set; }

        [StringLength(100)]
        public string MenuURL { get; set; }

        [StringLength(25)]
        public string MenuIcon { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CustomPermission> CustomPermission { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Permission> Permission { get; set; }
    }
}
