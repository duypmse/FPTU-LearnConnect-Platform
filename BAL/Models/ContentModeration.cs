using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class ContentModeration
    {
        public ContentModeration()
        {
            FlagDetails = new HashSet<FlagDetail>();
        }

        public int Id { get; set; }
        public string VideoUrl { get; set; } = null!;
        public int ContentLength { get; set; }
        public decimal? PercentExplicit { get; set; }
        public decimal? PercentUnsafe { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? PreviewDate { get; set; }
        public int Status { get; set; }
        public int? PreviewBy { get; set; }
        public int LectureId { get; set; }
        [JsonIgnore]
        public virtual Lecture Lecture { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<FlagDetail> FlagDetails { get; set; }
    }
}
