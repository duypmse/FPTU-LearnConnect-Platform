using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Specialization
    {
        public Specialization()
        {
            Courses = new HashSet<Course>();
            SpecializationOfMentors = new HashSet<SpecializationOfMentor>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int MajorId { get; set; }

        public virtual Major Major { get; set; } = null!;
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<SpecializationOfMentor> SpecializationOfMentors { get; set; }
    }
}
