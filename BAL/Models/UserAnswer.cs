using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class UserAnswer
    {
        public int Id { get; set; }
        public int AnswerId { get; set; }
        public DateTime CreateDate { get; set; }
        public int Status { get; set; }
        public int? TestResultId { get; set; }

        public virtual TestResult? TestResult { get; set; }
    }
}
