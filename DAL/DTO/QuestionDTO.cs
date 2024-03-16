using BAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = null!;
        public int QuestionType { get; set; }
        public int Status { get; set; }
        public int TestId { get; set; }
    }
}
