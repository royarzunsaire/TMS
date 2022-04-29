using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGC.Models
{
    [Table("R51_Checklist")]
    public class R51_Checklist
    {
        [Key]
        public int R51_ChecklistID { get; set; }
        public int idChecklist { get; set; }
        public int idR51 { get; set; }
        public string valor { get; set; }
        public string detalle { get; set; }
        public string aplica { get; set; }
        public string comentario { get; set; }
        public virtual R51 R51 { get; set; }
        public virtual Checklist Checklist { get; set; }
    }
}