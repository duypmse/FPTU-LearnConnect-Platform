using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class FavoriteCourseDTO
    {
        public int Id { get; set; }
        public int FavoriteCourseId { get; set; }
        public int UserId { get; set; }
    }
}
