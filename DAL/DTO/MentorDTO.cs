using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class MentorDTO
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal? AverageRating { get; set; }
        public string? PaypalId { get; set; }
        public string? PaypalAddress { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }
    }
}
