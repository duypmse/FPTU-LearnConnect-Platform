using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;

namespace DAL.Repository
{
    public interface IUserRepository : IBaseRepository<UserDTO>
    {
        public User GetByEmail(string email);
        public object CreateStaffAccount(string fullName, string email, string password);
        string GenerateToken(User user);
        public object Update(int id, int? gender, string? phoneNumber, string? biography, string? paypalId, string? paypalAddress);
        public bool IsRequestBecomeMentor(int id);
        public List<string> GetStaffMail();

    }
    public class UserRepository : IUserRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;
        private IConfiguration Configuration { get; }

        public UserRepository(LearnConnectDBContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            Configuration = configuration;
        }

        public UserDTO Add(UserDTO _objectDTO)
        {
            var _object = _mapper.Map<User>(_objectDTO);
            _context.Users.Add(_object);
            return null;
        }

        public UserDTO Get(int id)
        {
            var _object = _context.Users.Find(id);
            var _objectDTO = _mapper.Map<UserDTO>(_object);
            return _objectDTO;
        }

        public bool IsRequestBecomeMentor(int id)
        {
            bool isRequestBecomeMentor = false;

            var specRequest = _context.SpecializationOfMentors
                .Where(s => s.Status == (int)SpecializationOfMentorStatus.Pending
                && s.Mentor.Status == (int)MentorStatus.Inactive)
                .FirstOrDefault(s => s.Mentor.UserId == id);

            if(specRequest != null)
            {
                isRequestBecomeMentor = true;
            }

            return isRequestBecomeMentor;
        }

        public IEnumerable<UserDTO> GetList()
        {
            var _list = _context.Users.ToList();
            var _listDTO = _mapper.Map<List<UserDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, UserDTO _objectDTO)
        {
            var existingUser = _context.Users.Find(id);

            if (existingUser == null)
            {
                return 0; 
            }

            if (_objectDTO.Gender.HasValue)
            {
                existingUser.Gender = _objectDTO.Gender;
            }

            if (_objectDTO.BioDescription != null)
            {
                existingUser.BioDescription = _objectDTO.BioDescription;
            }

            if (_objectDTO.PhoneNumber != null)
            {
                existingUser.PhoneNumber = _objectDTO.PhoneNumber;
            }

            _context.Users.Update(existingUser);
            return 1;
        }

        public object Update(int id, int? gender, string? phoneNumber, string? biography, string? paypalId, string? paypalAddress)
        {
            var existingUser = _context.Users.Find(id);

            if (existingUser == null)
            {
                throw new Exception("User not found for the provided userId.");
            }

            var existingMentor = _context.Mentors
                .Where(m => m.Status == (int)MentorStatus.Active)
                .FirstOrDefault(m => m.UserId == existingUser.Id);

            if (gender != -1)
            {
                existingUser.Gender = gender;
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                existingUser.PhoneNumber = phoneNumber;
            }

            if (!string.IsNullOrEmpty(biography))
            {
                existingUser.BioDescription = biography;
            }

            if (existingMentor != null)
            {
                if (!string.IsNullOrEmpty(paypalId))
                {
                    existingMentor.PaypalId = paypalId;
                }

                if (!string.IsNullOrEmpty(paypalAddress))
                {
                    existingMentor.PaypalAddress = paypalAddress;
                }

                var paymentError = _context.PaymentTransactions
                    .Where(p => p.MentorId == existingMentor.Id && p.TransactionType == (int)TransactionTypeStatus.Pay && p.Status == (int)TransactionStatus.Error)
                    .ToList();
                foreach (var transaction in paymentError)
                {
                    transaction.Status = (int)TransactionStatus.Pending;
                }
            }

            _context.SaveChanges();

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            string serializedUser = JsonSerializer.Serialize(existingUser, options);
            string serializedMentor = JsonSerializer.Serialize(existingMentor, options);
            var message = new
            {
                Success = true,
                Message = "Update successfully!"
            };

            return message;
        }


        public int Delete(int id)
        {
            User _object = _context.Users.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Users.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Users.Any(e => e.Id == id);
            return _isExist;
        }

        public User GetByEmail(string email)
        {
            var _object = _context.Users.FirstOrDefault(user => user.Email == email);
            return _object;
        }

        public object CreateStaffAccount(string fullName, string email, string password)
        {
            try
            {
                var newUser = new User
                {
                    Email = email,
                    Password = password,
                    Role = (int)Roles.Staff,
                    FullName = fullName,
                    ProfilePictureUrl = "https://firebasestorage.googleapis.com/v0/b/learnconnect-6f324.appspot.com/o/images%2Fstaff.png?alt=media&token=0284d522-2950-40c5-a6f5-f3aaa50eb40b",
                    RegistrationDate = DateTime.UtcNow.AddHours(7),
                    Status = (int)UserStatus.Active
                };

                _context.Users.Add(newUser);

                var answerDTO = _mapper.Map<User>(newUser);

                return answerDTO;
            }
            catch (Exception ex)
            {
                return $"Error creating test: {ex.Message}";
            }
        }

        public string GenerateToken(User user)
        {
            if (user == null)
            {
                return "User is null!";
            }
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Configuration["JWT:Key"];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),

                    new Claim("TokenId", Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(120),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);

            return jwtTokenHandler.WriteToken(token);
        }

        public List<string> GetStaffMail()
        {
            var staffMails = _context.Users.Where(s => s.Role == (int)Roles.Staff).Select(s => s.Email).ToList();
            return staffMails;
        }
    }
}
