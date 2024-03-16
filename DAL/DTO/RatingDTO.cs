using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class RatingDTO
    {
        public int Id { get; set; }
        public decimal Rating1 { get; set; }
        public string? Comment { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Status { get; set; }
        public int RatingBy { get; set; }
        public int? CourseId { get; set; }
        public int? MentorId { get; set; }
    }
}
