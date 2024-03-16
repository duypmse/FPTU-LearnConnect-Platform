using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class ContentModerationDTO
    {
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
    }
}
