using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class SpecializationOfMentorDTO
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public int SpecializationId { get; set; }
        public int MentorId { get; set; }
    }
}
