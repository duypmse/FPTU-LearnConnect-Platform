using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public string Comment1 { get; set; } = null!;
        public DateTime CommentTime { get; set; }
        public int Status { get; set; }
        public int LectureId { get; set; }
    }
}
