using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Rating
    {
        public int Id { get; set; }
        public decimal Rating1 { get; set; }
        public string? Comment { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Status { get; set; }
        public int RatingBy { get; set; }
        public int? CourseId { get; set; }
        public int? MentorId { get; set; }

        public virtual Course? Course { get; set; }
        public virtual Mentor? Mentor { get; set; }
        public virtual User RatingByNavigation { get; set; } = null!;
    }
}
