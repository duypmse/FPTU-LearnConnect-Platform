using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Major
    {
        public Major()
        {
            Specializations = new HashSet<Specialization>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Specialization> Specializations { get; set; }
    }
}
