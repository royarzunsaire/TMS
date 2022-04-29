using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SGC.Models.Feedback
{
    [Table("FeedbackItemCommentMoodle")]
    public class FeedbackItemCommentMoodle
    {
        [Key]
        public int idFeedbackItemCommentMoodle { get; set; }
        public virtual FeedbackMoodle feedbackMoodle { get; set; }
        public string value { get; set; }
    }
}