using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class SpecializationOfMentor
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public int SpecializationId { get; set; }
        public int MentorId { get; set; }

        public virtual Mentor Mentor { get; set; } = null!;
        public virtual Specialization Specialization { get; set; } = null!;
    }
}
