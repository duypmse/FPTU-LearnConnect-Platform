using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Test
    {
        public Test()
        {
            Questions = new HashSet<Question>();
            TestResults = new HashSet<TestResult>();
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int TotalQuestion { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public int CourseId { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<TestResult> TestResults { get; set; }
    }
}
