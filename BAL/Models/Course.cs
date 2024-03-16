using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class Course
    {
        public Course()
        {
            Enrollments = new HashSet<Enrollment>();
            Lectures = new HashSet<Lecture>();
            Ratings = new HashSet<Rating>();
            Reports = new HashSet<Report>();
            Tests = new HashSet<Test>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string ImageUrl { get; set; } = null!;
        public decimal Price { get; set; }
        public int? TotalEnrollment { get; set; }
        public int LectureCount { get; set; }
        public int ContentLength { get; set; }
        public decimal? AverageRating { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public int SpecializationId { get; set; }
        public int MentorId { get; set; }

        public virtual Mentor Mentor { get; set; } = null!;
        [JsonIgnore]
        public virtual Specialization Specialization { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Lecture> Lectures { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}
