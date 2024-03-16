using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Schedule
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
