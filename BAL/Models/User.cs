using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class User
    {
        public User()
        {
            Enrollments = new HashSet<Enrollment>();
            FavoriteCourses = new HashSet<FavoriteCourse>();
            Notifications = new HashSet<Notification>();
            Ratings = new HashSet<Rating>();
            Reports = new HashSet<Report>();
            Schedules = new HashSet<Schedule>();
            TestResults = new HashSet<TestResult>();
        }

        public int Id { get; set; }
        public string? Password { get; set; }
        public string Email { get; set; } = null!;
        public int Role { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime? Dob { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Gender { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? BioDescription { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int Status { get; set; }

        public virtual Mentor? Mentor { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<FavoriteCourse> FavoriteCourses { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        [JsonIgnore]
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<TestResult> TestResults { get; set; }
    }
}
