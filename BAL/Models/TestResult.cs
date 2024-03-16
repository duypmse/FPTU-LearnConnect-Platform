using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class TestResult
    {
        public TestResult()
        {
            UserAnswers = new HashSet<UserAnswer>();
        }

        public int Id { get; set; }
        public decimal? Score { get; set; }
        public int? TimeSpent { get; set; }
        public int UserId { get; set; }
        public int TestId { get; set; }

        public virtual Test Test { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
