namespace SGC.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("AspNetRoles")]
    public partial class AspNetRoles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AspNetRoles()
        {
            Permission = new HashSet<Permission>();
            AspNetUsers = new HashSet<AspNetUsers>();
        }
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(128)]
        public string Discriminator { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Permission> Permission { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUsers> AspNetUsers { get; set; }

        public List<NotificacionConfig> notificacionConfig { get; set; }
    }
}
