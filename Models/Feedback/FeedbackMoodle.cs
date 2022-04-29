using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SGC.Models.Feedback
{
    [Table("FeedbackMoodle")]
    public class FeedbackMoodle
    {
        [Key]
        public int idFeedbackMoodle { get; set; }
        public int completedcount { get; set; }
        public int itemscount { get; set; }
        public virtual Comercializacion comercializacion { get; set; }
        public virtual AspNetUsers usuario { get; set; }
        public DateTime lastUpdate { get; set; }
        public virtual List<FeedbackItemMoodle> feedbackItemMoodle { get; set; }
        public virtual List<FeedbackItemCommentMoodle> feedbackItemCommentMoodle { get; set; }

    }
}