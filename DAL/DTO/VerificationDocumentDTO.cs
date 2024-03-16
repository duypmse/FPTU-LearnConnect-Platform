using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class VerificationDocumentDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int DocumentType { get; set; }
        public string DocumentUrl { get; set; } = null!;
        public int? SpecializationOfMentorId { get; set; }
        public int MentorId { get; set; }
    }
}
