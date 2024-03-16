using System;
using System.Collections.Generic;

namespace BAL.Models
{
    public partial class Question
    {
        public Question()
        {
            Answers = new HashSet<Answer>();
        }

        public int Id { get; set; }
        public string QuestionText { get; set; } = null!;
        public int QuestionType { get; set; }
        public int Status { get; set; }
        public int TestId { get; set; }

        public virtual Test Test { get; set; } = null!;
        public virtual ICollection<Answer> Answers { get; set; }
    }
}
