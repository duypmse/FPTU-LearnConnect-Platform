using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class LearningProcessDTO
    {
        public int Id { get; set; }
        public decimal? PercentComplete { get; set; }
        public int Status { get; set; }
        public int? CourseId { get; set; }
        public int? UserId { get; set; }
    }
}
