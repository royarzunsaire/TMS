namespace SGC.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MenuTemp")]
    public partial class MenuTemp
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MenuID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string DisplayName { get; set; }



        public int ParentMenuID { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OrderNumber { get; set; }

        [StringLength(100)]
        public string MenuURL { get; set; }

        [StringLength(25)]
        public string MenuIcon { get; set; }
    }
}
