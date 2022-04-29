using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SGC.Models.Feedback
{
    [Table("FeedbackItemMoodle")]
    public class FeedbackItemMoodle
    {
        public virtual FeedbackMoodle feedbackMoodle { get; set; }
        [Key]
        public int idFeedbackItemMoodle { get; set; }
        public int feedback { get; set; }
        public string name { get; set; }
        public string presentation { get; set; }
        public string typ { get; set; }
        public virtual List<FeedbackItemDataMoodle> feedbackItemDataMoodle { get; set; }
    


    }
    public enum TipoFeedbackItem
    {
        [Description("Infraestructura")]
        Infraestructura,
        [Description("Material de Apoyo")]
        MaterialDeApoyo,
        [Description("Relator")]
        Relator,
        [Description("Servicio de alimentación")]
        Alimentacion
       
    }
}