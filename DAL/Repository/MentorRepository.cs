using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Firebase.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using DAL.Service;
using CloudinaryDotNet;

namespace DAL.Repository
{
    public interface IMentorRepository : IBaseRepository<MentorDTO> {
        public object BecomeMentor(int userId,
                                    int specializationId,
                                    string description,
                                    string reason,
                                    string accountNumber,
                                    string bankName,
                                    string identityCardFrontDescription,
                                    IFormFile identityCardFrontUrl,
                                    string identityCardBackDescription,
                                    IFormFile identityCardBackUrl,
                                    string descriptionDocument,
                                    IFormFile verificationDocument);
        public object AddSpecializeByMentor(int userId,
                                    int specializationId,
                                    string reason,
                                    string descriptionDocument,
                                    IFormFile verificationDocument);
        public object ProcessMentorRequest(int staffUserId, int mentorUserId, int specializationId, bool acceptRequest, string? rejectReason);
        public IEnumerable<object> GetMentorsInfo();
        public MentorDTO UpdateMentorStatus(int mentorId, MentorStatus status);
        public IEnumerable<object> GetTop3MentorsByRating();
        public object GetSpecializationsRequest(int mentorUserId, SpecializationOfMentorStatus status);
        public MentorDTO GetByUserId(int userId);
        public MentorDTO GetByMentorUserId(int id);
        public string BanMentor(int mentorId, bool isBan, string reason);

    }
    public class MentorRepository : IMentorRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;
        private readonly IFirebaseService uploadService;
        private readonly INotificationRepository notificationRepository;
        private readonly IUserRepository userRepository;

        public MentorRepository(LearnConnectDBContext context, IMapper mapper, IFirebaseService uploadService, 
            INotificationRepository notificationRepository,
            IUserRepository userRepository)
        {
            _context = context;
            _mapper = mapper;
            this.uploadService = uploadService;
            this.notificationRepository = notificationRepository;
            this.userRepository = userRepository;
        }
        public MentorDTO Add(MentorDTO _objectDTO)
        {
            var _object = _mapper.Map<Mentor>(_objectDTO);
            _context.Mentors.Add(_object);
            return null;
        }

        public MentorDTO Get(int id)
        {
            var _object = _context.Mentors.Find(id);
            var _objectDTO = _mapper.Map<MentorDTO>(_object);
            return _objectDTO;
        }
        public MentorDTO GetByMentorUserId(int mentorUserId)
        {
            var mentor = _context.Mentors
                .Where(m => m.Status == (int)MentorStatus.Active)
                .FirstOrDefault(m => m.UserId == mentorUserId);

            if (mentor == null)
            {
                return null;
            }

            var mentorDTO = _mapper.Map<MentorDTO>(mentor);
            return mentorDTO;
        }
        public MentorDTO GetByUserId(int userId)
        {
            var _object = _context.Mentors.FirstOrDefault(m => m.UserId == userId);
            var _objectDTO = _mapper.Map<MentorDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<MentorDTO> GetList()
        {
            var _list = _context.Mentors.ToList();
            var _listDTO = _mapper.Map<List<MentorDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, MentorDTO _objectDTO)
        {
            var _object = _context.Mentors.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.Mentors.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            Mentor _object = _context.Mentors.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Mentors.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Mentors.Any(e => e.Id == id);
            return _isExist;
        }

        private VerificationDocument CreateVerificationDocument(int userId, string description, int docType, IFormFile document, int specializationOfMentorId)
        {
            var user = _context.Users.Find(userId);
            var mentorId = _context.Mentors.FirstOrDefault(m => m.UserId == userId).Id;
            var documentName = $"{description}-{user.FullName}";
            var documentUrlUpload = uploadService.Upload(document, documentName, "VerificationDocuments").Result;

            var verificationDocument = new VerificationDocument
            {
                Description = description,
                DocumentType = docType,
                DocumentUrl = documentUrlUpload,
                SpecializationOfMentorId = specializationOfMentorId,
                MentorId = mentorId
            };

            _context.VerificationDocuments.Add(verificationDocument);

            return verificationDocument;
        }


        public object BecomeMentor(int userId,
                                    int specializationId,
                                    string description,
                                    string reason,
                                    string accountNumber,
                                    string bankName,
                                    string identityCardFrontDescription,
                                    IFormFile identityCardFrontUrl,
                                    string identityCardBackDescription,
                                    IFormFile identityCardBackUrl,
                                    string descriptionDocument,
                                    IFormFile verificationDocument)
        {
            var mentorExsit = _context.Mentors
                .Where(m => m.UserId == userId).FirstOrDefault();

            var mentorExistPending = _context.Mentors
                    .Where(m => m.UserId == userId && m.Status == (int)MentorStatus.Inactive &&
                    m.SpecializationOfMentors.Any(s => s.Status == (int)SpecializationOfMentorStatus.Pending))
                    .FirstOrDefault();
            if (mentorExistPending != null) {
                throw new Exception("Request is pending, waiting for staff to accept!");
            }

            if (mentorExsit == null)
            {
                var mentor = new Mentor
                {
                    UserId = userId,
                    Description = description,
                    PaypalId = accountNumber,
                    PaypalAddress = bankName,
                    Status = (int)MentorStatus.Inactive
                };

                _context.Mentors.Add(mentor);
                _context.SaveChanges();

                var specializationOfMentor = new SpecializationOfMentor
                {
                    SpecializationId = specializationId,
                    MentorId = mentor.Id,
                    Description = reason,
                    VerificationDate = DateTime.UtcNow.AddHours(7), // Sua lai ten (De xuat: RequestDate)
                    Note = null,
                    Status = (int)SpecializationOfMentorStatus.Pending
                };

                _context.SpecializationOfMentors.Add(specializationOfMentor);
                _context.SaveChanges();

                int mentorId = _context.Mentors.First(m => m.UserId == userId).Id;
                int specializationOfMentorId = _context.SpecializationOfMentors.First(m => m.MentorId == mentorId).Id;

                int docType = 0;
                descriptionDocument = "Document";
                var verificationDocumentFront = CreateVerificationDocument(userId, identityCardFrontDescription, docType = 1, identityCardFrontUrl, specializationOfMentorId);
                var verificationDocumentBack = CreateVerificationDocument(userId, identityCardBackDescription, docType = 1, identityCardBackUrl, specializationOfMentorId);
                var verificationDocumentDTO = CreateVerificationDocument(userId, descriptionDocument, docType = 0, verificationDocument, specializationOfMentorId);
            }else
            {
                if (!string.IsNullOrEmpty(description))
                {
                    mentorExsit.Description = description;
                }

                if (!string.IsNullOrEmpty(accountNumber))
                {
                    mentorExsit.PaypalId = accountNumber;
                }

                if (!string.IsNullOrEmpty(bankName))
                {
                    mentorExsit.PaypalAddress = bankName;
                }
                var specializationOfMentor = new SpecializationOfMentor
                {
                    SpecializationId = specializationId,
                    MentorId = mentorExsit.Id,
                    Description = reason,
                    VerificationDate = DateTime.UtcNow.AddHours(7),
                    Note = null,
                    Status = (int)SpecializationOfMentorStatus.Pending
                };

                _context.SpecializationOfMentors.Add(specializationOfMentor);
                _context.SaveChanges();
                int mentorId = _context.Mentors.OrderBy(m => m.Id).Last(m => m.UserId == userId).Id;
                int specializationOfMentorId = _context.SpecializationOfMentors.OrderBy(m => m.Id).Last(m => m.MentorId == mentorId).Id;

                int docType = 0;

                var verificationDocumentFront = CreateVerificationDocument(userId, identityCardFrontDescription, docType = 1, identityCardFrontUrl, specializationOfMentorId);
                var verificationDocumentBack = CreateVerificationDocument(userId, identityCardBackDescription, docType = 1, identityCardBackUrl, specializationOfMentorId);
                var verificationDocumentDTO = CreateVerificationDocument(userId, descriptionDocument, docType = 0, verificationDocument, specializationOfMentorId);
            }


            var result = new
            {
                Message = "Successfully created a request to become a Mentor!",
            };

            if (result != null)
            {
                var usersStaff = _context.Users.Where(u => u.Role == (int)Roles.Staff).Select(u => u.Id).ToArray();
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                var specialization = _context.Specializations.FirstOrDefault(c => c.Id == specializationId);

                if (user != null && specialization != null)
                {
                    notificationRepository.Create(
                        "New Request",
                        $"Student {user.FullName} has just sent a request to become a mentor for specialization {specialization.Name}",
                        usersStaff
                    );
                }
            }
            return result;
        }


        public object AddSpecializeByMentor(int userId, int specializationId, string reason, string descriptionDocument, IFormFile verificationDocument)
        {
            var specializationMentorExsit = _context.SpecializationOfMentors
                .Where(s => s.Mentor.UserId == userId && s.SpecializationId == specializationId && s.Status == (int)SpecializationOfMentorStatus.Approve).FirstOrDefault();
            
            var specializationMentorRequestExsit = _context.SpecializationOfMentors
                .Where(s => s.Mentor.UserId == userId && s.SpecializationId == specializationId && s.Status == (int)SpecializationOfMentorStatus.Pending).FirstOrDefault();

            if (specializationMentorRequestExsit != null)
            {
                throw new Exception("You are already a mentor of this specialization.");
            }
            if (specializationMentorRequestExsit != null)
            {
                throw new Exception("Request is pending, waiting for staff to accept!");
            }

            var mentorId = _context.Mentors.FirstOrDefault(m => m.UserId == userId)?.Id;
            if (mentorId == null)
            {
                throw new Exception("Mentor not found for the given user.");
            }

            var specializationOfMentor = new SpecializationOfMentor
            {
                SpecializationId = specializationId,
                MentorId = mentorId.Value,
                Description = reason,
                VerificationDate = DateTime.UtcNow.AddHours(7),
                Note = null,
                Status = (int)SpecializationOfMentorStatus.Pending
            };

            _context.SpecializationOfMentors.Add(specializationOfMentor);
            _context.SaveChanges();

            var specializationOfMentorId = specializationOfMentor.Id;

            int docType = 0;

            var verificationDocumentDTO = CreateVerificationDocument(userId, descriptionDocument, docType, verificationDocument, specializationOfMentorId);

            var result = new
            {
                Message = "Successfully created a new Specialization!",
            };

            if (result != null)
            {
                var usersStaff = _context.Users.Where(u => u.Role == (int)Roles.Staff).Select(u => u.Id).ToArray();
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                var specialization = _context.Specializations.FirstOrDefault(c => c.Id == specializationId);

                if (user != null && specialization != null)
                {
                    notificationRepository.Create(
                        "New Request",
                        $"Mentor {user.FullName} has just sent a request for add more specialization {specialization.Name}",
                        usersStaff
                    );
                }
            }

            return result;
        }

        public object ProcessMentorRequest(int staffUserId, int mentorUserId, int specializationId, bool acceptRequest, string? note)
        {
            var staffUser = _context.Users.Find(staffUserId);
            var mentorUser = _context.Users.Find(mentorUserId);

            if (staffUser == null || mentorUser == null)
            {
                throw new Exception("Invalid staff or mentor user");
            }

            var mentorRequest = _context.Mentors
                .FirstOrDefault(m => m.UserId == mentorUserId);

            if (mentorRequest == null)
            {
                throw new Exception("Mentor request not found");
            }

            var specializationOfMentor = _context.SpecializationOfMentors
                .FirstOrDefault(c => c.MentorId == mentorRequest.Id && c.SpecializationId == specializationId && c.Status == (int)SpecializationOfMentorStatus.Pending);

            if (specializationOfMentor == null)
            {
                throw new Exception("Course specialization of mentor not found");
            }

            if (acceptRequest)
            {
                if(specializationOfMentor.Status == (int)SpecializationOfMentorStatus.Pending)
                {
                    mentorRequest.Status = (int)MentorStatus.Active;
                    specializationOfMentor.Status = (int)SpecializationOfMentorStatus.Approve;
                    mentorUser.Role = (int)Roles.Mentor;
                    specializationOfMentor.Note = note ?? "Approved!";


                    var usersReceive = _context.Users.Where(u => u.Id == mentorUserId).Select(u => u.Id).ToArray();
                    var specialization = _context.Specializations.FirstOrDefault(c => c.Id == specializationId);

                    if (usersReceive != null && specialization != null)
                    {
                        notificationRepository.Create(
                            "New Response",
                            $"You have just been approved as a mentor of {specialization.Name} specialization.",
                            usersReceive
                        );
                    }
                }
                else
                {
                    throw new Exception("Only can accept when SpecializationOfMentorStatus is Pending");
                }
            }
            else
            {

                if (specializationOfMentor.Status == (int)SpecializationOfMentorStatus.Pending)
                {
                    specializationOfMentor.Status = (int)SpecializationOfMentorStatus.Reject;
                    specializationOfMentor.Note = note;

                    var usersReceive = _context.Users.Where(u => u.Id == mentorUserId).Select(u => u.Id).ToArray();
                    var specialization = _context.Specializations.FirstOrDefault(c => c.Id == specializationId);

                    if (usersReceive != null && specialization != null)
                    {
                        notificationRepository.Create(
                            "New Response",
                            $"You have just been rejected as a mentor of {specialization.Name} specialization.",
                            usersReceive
                        );
                    }
                }
            }

            _context.SaveChanges();

            var result = new
            {
                Message = acceptRequest ? "Mentor request accepted successfully!" : "Mentor request rejected successfully!",
                Data = new
                {
                    MentorUserId = mentorUserId,
                    StaffUserId = staffUserId,
                    AcceptRequest = acceptRequest,
                    RejectReason = note
                }
            };

            return result;
        }

        public IEnumerable<object> GetMentorsInfo()
        {

            var users = _context.Users.ToList();

            var _listUserDTO = _mapper.Map<List<UserDTO>>(users);


            var mentors = _context.Mentors.ToList().Where(m => m.Status == (int)MentorStatus.Active);

            foreach (var mentor in mentors)
            {
                mentor.AverageRating = _context.Ratings
                    .Where(r => r.MentorId == mentor.Id && r.Status == (int)RatingStatus.Show)
                    .Select(r => (decimal?)r.Rating1)
                    .DefaultIfEmpty()
                    .Average();
            }



            var _listMentorDTO = _mapper.Map<List<MentorDTO>>(mentors);

            var _listReturnMentorDTO = _listMentorDTO.Join(_listUserDTO, m => m.UserId, u => u.Id, (m, u) => new
            {
                MentorInfo = m,
                UserInfo = u
            });



            return _listReturnMentorDTO;
        }

        public MentorDTO UpdateMentorStatus(int mentorId, MentorStatus status)
        {
            var mentor = _context.Mentors.Find(mentorId);
            if (mentor == null)
            {
                throw new Exception("Mentor not found");
            }
            var user = _context.Users.Where(u => u.Id == mentor.UserId).FirstOrDefault();
            var coursesMentor = _context.Courses.Where(m => m.MentorId == mentorId).ToList();
            var specializeMentor = _context.SpecializationOfMentors.Where(s => s.MentorId == mentorId).ToList();

            
            if (user != null)
            {
                if (status == MentorStatus.Inactive)
                {
                    mentor.Status = (int)status;
                    user.Role = (int)Roles.Student;
                    foreach (var m in coursesMentor)
                    {
                        m.Status = (int)CourseStatus.Pending;
                    }
                    foreach (var m in specializeMentor)
                    {
                        m.Status = (int)SpecializationOfMentorStatus.Reject;
                    }
                }
                if (status == MentorStatus.Active)
                {
                    mentor.Status = (int)status;
                    user.Role = (int)Roles.Mentor;
                    foreach (var m in coursesMentor)
                    {
                        m.Status = (int)CourseStatus.Active;
                    }
                    foreach (var m in specializeMentor)
                    {
                        m.Status = (int)SpecializationOfMentorStatus.Approve;
                    }
                }
            }
            _context.SaveChanges();

            var mentorDTO = _mapper.Map<MentorDTO>(mentor);

            return mentorDTO;
        }

        public IEnumerable<object> GetTop3MentorsByRating()
        {
            var mentors = _context.Mentors.ToList().Where(m => m.Status == (int)MentorStatus.Active);
            var _listMentorDTO = _mapper.Map<List<MentorDTO>>(mentors);

            var users = _context.Users.ToList();
            var _listUserDTO = _mapper.Map<List<UserDTO>>(users);

            var ratings = _context.Ratings.ToList().Where(r => r.Status == (int)RatingStatus.Show);

            var mentorRatings = _listMentorDTO.GroupJoin(
                ratings,
                mentor => mentor.Id,
                rating => rating.MentorId,
                (mentor, mentorRatings) => new
                {
                    MentorInfo = mentor,
                    UserInfo = _listUserDTO.FirstOrDefault(u => u.Id == mentor.UserId),
                    AverageRating = mentorRatings.Any() ? mentorRatings.Average(r => r.Rating1) : 0,
                }
            );

            var top3Mentors = mentorRatings.OrderByDescending(m => m.AverageRating).Take(3);

            return top3Mentors.Select(m => new
            {
                MentorInfo = new
                {
                    MentorId = m.MentorInfo.Id,
                    MentorUserId = m.UserInfo.Id,
                    MentorName = m.UserInfo.FullName,
                    MentorImage = m.UserInfo.ProfilePictureUrl,
                    AverageRating = m.AverageRating
                }
            });
        }

        public object GetSpecializationsRequest(int mentorUserId, SpecializationOfMentorStatus status)
        {
            var specializationOfMentorDetails = _context.SpecializationOfMentors
                .Where(s => s.Mentor.User.Id == mentorUserId && s.Status == (int)status)
                .Select(specializationOfMentor => new
                {
                    SpecializationOfMentor = specializationOfMentor,
                    Mentor = new { Id = specializationOfMentor.Mentor.Id, Description = specializationOfMentor.Mentor.Description },
                    Specialization = new { Id = specializationOfMentor.Specialization.Id, Name = specializationOfMentor.Specialization.Name },
                    VerificationDocuments = _context.VerificationDocuments
                        .Where(doc => doc.SpecializationOfMentorId == specializationOfMentor.Id)
                        .ToList()
                })
                .ToList();

            if (specializationOfMentorDetails == null || !specializationOfMentorDetails.Any())
            {
                throw new Exception("Not found SpecializationOfMentor!");
            }

            return specializationOfMentorDetails;
        }

        public string BanMentor(int mentorId, bool isBan, string reason)
        {            
            var mentor = _context.Mentors
                    .Include(m => m.User)
                    .FirstOrDefault(m => m.Id == mentorId);

            if (mentor == null)
            {
                throw new Exception("Mentor not found");
            }

            mentor.User.Role = isBan ? (int)Roles.Student : (int)Roles.Mentor;
            mentor.Description = isBan ? reason : reason;
            mentor.Status = isBan ? (int)MentorStatus.Inactive : (int)MentorStatus.Active;

            var courses = _context.Courses.Where(c => c.MentorId == mentor.Id);

            foreach (var course in courses)
            {
                course.Status = isBan ? (int)CourseStatus.Banned : (int)CourseStatus.Active;
            }

            var usersReceive = _context.Users.Where(u => u.Id == mentor.User.Id).Select(u => u.Id).ToArray();

            if (usersReceive != null && reason != null)
            {
                string notificationMessage = isBan
                    ? $"You have been banned from being a mentor of LearnConnect. Reason: {reason}. If you have any questions, please contact the support team via email contact.learnconnect@gmail.com"
                    : $"You have been unbanned";

                notificationRepository.Create(
                    "New Notification",
                    notificationMessage,
                    usersReceive
                );
            }

            return isBan ? "Mentor banned successfully" : "Mentor unbanned successfully";
        }

    }
}