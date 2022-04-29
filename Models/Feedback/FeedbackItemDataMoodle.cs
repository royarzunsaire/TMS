using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SGC.Models.Feedback
{
    [Table("FeedbackItemDataMoodle")]
    public class FeedbackItemDataMoodle
    {
        [Key]
        public int idFeedbackItemDataMoodle { get; set; }
        public virtual FeedbackItemMoodle feedbackItemMoodle { get; set; }
        public string answertext { get; set; }
        public double answercount { get; set; }
        public string value { get; set; }
    }
}