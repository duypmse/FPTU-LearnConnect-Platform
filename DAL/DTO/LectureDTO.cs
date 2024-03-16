using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class LectureDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string ContentUrl { get; set; } = null!;
        public int ContentType { get; set; }
        public int LectureLength { get; set; }
        public string? RejectReason { get; set; }
        public int Status { get; set; }
        public int CourseId { get; set; }
    }
}
