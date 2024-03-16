using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class UserDTO
    {
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
    }
}
