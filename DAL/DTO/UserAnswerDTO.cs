using BAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class UserAnswerDTO
    {
        public int Id { get; set; }
        public int AnswerId { get; set; }
        public DateTime CreateDate { get; set; }
        public int Status { get; set; }
        public int? TestResultId { get; set; }
    }
}
