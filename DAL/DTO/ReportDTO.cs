using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class ReportDTO
    {
        public int Id { get; set; }
        public string ReportType { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ReportBy { get; set; }
        public int? CourseId { get; set; }
        public int? MentorId { get; set; }
    }
}
