using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public string Comment1 { get; set; } = null!;
        public DateTime CommentTime { get; set; }
        public int Status { get; set; }
        public int LectureId { get; set; }

        public virtual Lecture Lecture { get; set; } = null!;
        public virtual User User { get; set; } = null!;

    }
}
