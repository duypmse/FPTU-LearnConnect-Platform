using BAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class TestDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int TotalQuestion { get; set; }
        public DateTime CreateDate { get; set; }
        public int Status { get; set; }
        public int CourseId { get; set; }
    }
}
