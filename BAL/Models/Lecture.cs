using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class Lecture
    {
        public Lecture()
        {
            Comments = new HashSet<Comment>();
            LearningProcessDetails = new HashSet<LearningProcessDetail>();
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string ContentUrl { get; set; } = null!;
        public int ContentType { get; set; }
        public int LectureLength { get; set; }
        public string? RejectReason { get; set; }
        public int Status { get; set; }
        public int CourseId { get; set; }

        [JsonIgnore]
        public virtual Course Course { get; set; } = null!;
        public virtual ContentModeration? ContentModeration { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<LearningProcessDetail> LearningProcessDetails { get; set; }
    }
}
