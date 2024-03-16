using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int? UserId { get; set; }

        public virtual User? User { get; set; }
    }
}
