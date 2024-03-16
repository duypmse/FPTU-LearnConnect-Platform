using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class LearningProcessDetailDTO
    {
        public int Id { get; set; }
        public int CurrentStudyTime { get; set; }
        public int MaxStudyTime { get; set; }
        public int TotalTime { get; set; }
        public int LearningProcessId { get; set; }
        public int LectureId { get; set; }
        public int Status { get; set; }
    }
}
