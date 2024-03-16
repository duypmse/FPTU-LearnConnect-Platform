using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class LearningPerformanceDTO
    {
        public int Id { get; set; }
        public decimal? Score { get; set; }
        public int? TimeSpent { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
    }
}
