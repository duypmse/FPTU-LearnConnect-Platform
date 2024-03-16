using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class FavoriteCourse
    {
        public int Id { get; set; }
        public int FavoriteCourseId { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
