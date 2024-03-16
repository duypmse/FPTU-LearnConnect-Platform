using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class VerificationDocument
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int DocumentType { get; set; }
        public string DocumentUrl { get; set; } = null!;
        public int? SpecializationOfMentorId { get; set; }
        public int MentorId { get; set; }

        public virtual Mentor Mentor { get; set; } = null!;
    }
}
