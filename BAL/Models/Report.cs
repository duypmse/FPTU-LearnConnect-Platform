using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Report
    {
        public int Id { get; set; }
        public string ReportType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ReportBy { get; set; }
        public int? CourseId { get; set; }
        public int? MentorId { get; set; }

        public virtual Course? Course { get; set; }
        public virtual Mentor? Mentor { get; set; }
        public virtual User ReportByNavigation { get; set; } = null!;
    }
}
