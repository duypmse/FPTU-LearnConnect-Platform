using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int? UserId { get; set; }
    }
}
