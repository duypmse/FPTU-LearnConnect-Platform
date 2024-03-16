using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class EnrollmentDTO
    {
        public int Id { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public bool IsFree { get; set; }
        public decimal? PercentComplete { get; set; }
        public decimal? TotalScore { get; set; }
        public int? TimeSpent { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
    }
}
