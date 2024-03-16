using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class Enrollment
    {
        public Enrollment()
        {
            LearningProcessDetails = new HashSet<LearningProcessDetail>();
            PaymentTransactions = new HashSet<PaymentTransaction>();
        }

        public int Id { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public bool IsFree { get; set; }
        public decimal? PercentComplete { get; set; }
        public decimal? TotalScore { get; set; }
        public int? TimeSpent { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }

        [JsonIgnore]
        public virtual Course Course { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<LearningProcessDetail> LearningProcessDetails { get; set; }
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    }
}
