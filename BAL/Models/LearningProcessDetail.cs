using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class LearningProcessDetail
    {
        public int Id { get; set; }
        public int CurrentStudyTime { get; set; }
        public int MaxStudyTime { get; set; }
        public int TotalTime { get; set; }
        public int? TimeSpent { get; set; }
        public int Status { get; set; }
        public int EnrollmentId { get; set; }
        public int LectureId { get; set; }
        [JsonIgnore]
        public virtual Enrollment Enrollment { get; set; } = null!;
        public virtual Lecture Lecture { get; set; } = null!;
    }
}
