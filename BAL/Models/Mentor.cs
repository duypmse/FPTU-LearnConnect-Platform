using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class Mentor
    {
        public Mentor()
        {
            Courses = new HashSet<Course>();
            Ratings = new HashSet<Rating>();
            Reports = new HashSet<Report>();
            SpecializationOfMentors = new HashSet<SpecializationOfMentor>();
            VerificationDocuments = new HashSet<VerificationDocument>();
        }

        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal? AverageRating { get; set; }
        public string? PaypalId { get; set; }
        public string? PaypalAddress { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<SpecializationOfMentor> SpecializationOfMentors { get; set; }
        public virtual ICollection<VerificationDocument> VerificationDocuments { get; set; }
    }
}
