using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface ISpecializationOfMentorRepository : IBaseRepository<SpecializationOfMentorDTO>
    {
        public IEnumerable<object> GetListByMentorId(int mentorUserId);
        IEnumerable<object> GetSpecializationsNotRequestYet(int mentorUserId, int majorId);
        public object GetSpecializationAndMentorRequest(int specializationOfMentorId);
        public object GetAllSpecializationAndMentorRequest(string requestType);


    }
    public class SpecializationOfMentorRepository : ISpecializationOfMentorRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationRepository notificationRepository;

        public SpecializationOfMentorRepository(LearnConnectDBContext context, IMapper mapper, INotificationRepository notificationRepository)
        {
            _context = context;
            _mapper = mapper;
            this.notificationRepository = notificationRepository;
        }
        public SpecializationOfMentorDTO Add(SpecializationOfMentorDTO _objectDTO)
        {
            var _object = _mapper.Map<SpecializationOfMentor>(_objectDTO);
            _context.SpecializationOfMentors.Add(_object);
            return null;
        }

        public SpecializationOfMentorDTO Get(int id)
        {
            var _object = _context.SpecializationOfMentors.Find(id);
            var _objectDTO = _mapper.Map<SpecializationOfMentorDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<SpecializationOfMentorDTO> GetList()
        {
            var _list = _context.SpecializationOfMentors.ToList();
            var _listDTO = _mapper.Map<List<SpecializationOfMentorDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, SpecializationOfMentorDTO _objectDTO)
        {
            var _object = _context.SpecializationOfMentors.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.SpecializationOfMentors.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            SpecializationOfMentor _object = _context.SpecializationOfMentors.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.SpecializationOfMentors.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.SpecializationOfMentors.Any(e => e.Id == id);
            return _isExist;
        }

        public IEnumerable<object> GetListByMentorId(int mentorUserId)
        {
            var _listSpecMentorDTO = _context.SpecializationOfMentors
                .Where(r => r.Mentor.UserId == mentorUserId && r.Status == (int)SpecializationOfMentorStatus.Approve )
                .Join(_context.Specializations,
                    specOfmentor => specOfmentor.SpecializationId,
                    spec => spec.Id,
                    (specOfmentor, spec) => new
                    {
                        SpecId = spec.Id,
                        SpecName = spec.Name
                    })
                .ToList();

            return _listSpecMentorDTO;
        }

        public IEnumerable<object> GetSpecializationsNotRequestYet(int mentorUserId, int majorId)
        {
            var approvedSpecIds = _context.SpecializationOfMentors
                .Where(r => r.Mentor.UserId == mentorUserId && (r.Status == (int)SpecializationOfMentorStatus.Approve || r.Status == (int)SpecializationOfMentorStatus.Pending))
                .Select(r => r.SpecializationId)
                .ToList();

            var unapprovedSpecializations = _context.Specializations
                .Where(spec =>
                    spec.MajorId == majorId &&
                    !approvedSpecIds.Contains(spec.Id)
                )
                .Select(spec => new { SpecId = spec.Id, SpecName = spec.Name })
                .ToList();

            return unapprovedSpecializations;
        }

        public object GetSpecializationAndMentorRequest(int specializationOfMentorId)
        {
            var specializationOfMentor = _context.SpecializationOfMentors
                .Include(s => s.Mentor)
                    .ThenInclude(m => m.User)
                .Include(s => s.Specialization)
                .SingleOrDefault(s => s.Id == specializationOfMentorId);

            if (specializationOfMentor == null)
            {
                throw new Exception("SpecializationOfMentor with the provided ID was not found.");
            }

            var verificationDocuments = _context.VerificationDocuments
                .Where(doc => doc.SpecializationOfMentorId == specializationOfMentorId)
                .ToList();

            var response = new
            {
                SpecializationOfMentor = new
                {
                    Id = specializationOfMentor.Id,
                    Description = specializationOfMentor.Description,
                    VerificationDate = specializationOfMentor.VerificationDate,
                    Note = specializationOfMentor.Note,
                    Status = specializationOfMentor.Status,
                },
                Mentor = new
                {
                    Id = specializationOfMentor.Mentor.Id,
                    Description = specializationOfMentor.Mentor.Description,
                },
                User = new
                {
                    Id = specializationOfMentor.Mentor.User.Id,
                    Name = specializationOfMentor.Mentor.User.FullName,
                    ProfilePictureUrl = specializationOfMentor.Mentor.User.ProfilePictureUrl,
                    Email = specializationOfMentor.Mentor.User.Email

                },
                Specialization = new
                {
                    Id = specializationOfMentor.Specialization.Id,
                    Name = specializationOfMentor.Specialization.Name
                },
                VerificationDocuments = verificationDocuments.Select(doc => new
                {
                    Id = doc.Id,
                    Description = doc.Description,
                    DocumentUrl = doc.DocumentUrl
                }).ToList()
            };

            return response;
        }

        public object GetAllSpecializationAndMentorRequest(string requestType)
        {
            IQueryable<SpecializationOfMentor> query = _context.SpecializationOfMentors
                .Include(s => s.Mentor)
                    .ThenInclude(m => m.User)
                .Include(s => s.Specialization);

            if (string.Equals(requestType, "mentor", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => _context.VerificationDocuments.Count(doc => doc.SpecializationOfMentorId == s.Id) == 3);
            }
            else if (string.Equals(requestType, "specialization", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => _context.VerificationDocuments.Count(doc => doc.SpecializationOfMentorId == s.Id) == 1);
            }
            else
            {
                throw new ArgumentException("Invalid filterType");
            }

            var responseList = query
                .OrderByDescending(specializationOfMentor => specializationOfMentor.VerificationDate)
                .Select(specializationOfMentor => new
            {
                SpecializationOfMentor = new
                {
                    Id = specializationOfMentor.Id,
                    Description = specializationOfMentor.Description,
                    VerificationDate = specializationOfMentor.VerificationDate,
                    Note = specializationOfMentor.Note,
                    Status = specializationOfMentor.Status,
                },
                Mentor = new
                {
                    Id = specializationOfMentor.Mentor.Id,
                    Description = specializationOfMentor.Mentor.Description,
                },
                User = new
                {
                    Id = specializationOfMentor.Mentor.User.Id,
                    Name = specializationOfMentor.Mentor.User.FullName,
                    ProfilePictureUrl = specializationOfMentor.Mentor.User.ProfilePictureUrl,
                    Email = specializationOfMentor.Mentor.User.Email
                },
                Specialization = new
                {
                    Id = specializationOfMentor.Specialization.Id,
                    Name = specializationOfMentor.Specialization.Name
                },
                VerificationDocuments = _context.VerificationDocuments
                    .Where(doc => doc.SpecializationOfMentorId == specializationOfMentor.Id && doc.DocumentType == (int)DocumentType.Verification)
                    .Select(doc => new
                    {
                        Id = doc.Id,
                        Description = doc.Description,
                        DocumentUrl = doc.DocumentUrl
                    }).ToList(),
                SpecializationDocuments = _context.VerificationDocuments
                    .Where(doc => doc.SpecializationOfMentorId == specializationOfMentor.Id && doc.DocumentType == (int)DocumentType.Specialization)
                    .Select(doc => new
                    {
                        Id = doc.Id,
                        Description = doc.Description,
                        DocumentUrl = doc.DocumentUrl
                    }).ToList(),
                }).ToList();

            SendNotiSchedule();

            return responseList;
        }

        public void SendNotiSchedule()
        {
            try
            {
                var scheduleList = _context.Schedules
                    .ToList();

                foreach (var schedule in scheduleList)
                {
                    double countTimeSendNotiStart = (schedule.StartDate - DateTime.UtcNow.AddHours(7)).TotalHours;
                    double countTimeSendNotiEnd = (schedule.EndDate - DateTime.UtcNow.AddHours(7)).TotalHours;
                    if (0 < countTimeSendNotiStart && countTimeSendNotiStart < 24 && schedule.Status == (int)SendNotiScheduleStatus.NotSent)
                    {
                        var usersReceive = _context.Users.Where(u => u.Id == schedule.UserId).Select(u => u.Id).ToArray();
                        var course = _context.Courses.FirstOrDefault(c => c.Id == schedule.CourseId);

                        if (usersReceive != null && course != null)
                        {
                            notificationRepository.Create(
                                    "Reminder Notification",
                                    $"Based on your schedule, you are set to start your studying course {course.Name} tomorrow. Note: {schedule.Note}. Wishing you a productive and successful learning journey!",
                                    usersReceive
                                );

                            schedule.Status = (int)SendNotiScheduleStatus.SentStart;
                        }
                    }
                    if (0 < countTimeSendNotiEnd && countTimeSendNotiEnd < 24 && schedule.Status == (int)SendNotiScheduleStatus.SentStart)
                    {
                        var usersReceive = _context.Users.Where(u => u.Id == schedule.UserId).Select(u => u.Id).ToArray();
                        var course = _context.Courses.FirstOrDefault(c => c.Id == schedule.CourseId);

                        if (usersReceive != null && course != null)
                        {
                            notificationRepository.Create(
                                    "Reminder Notification",
                                    $"Based on your schedule, you are set to end studying course {course.Name} tomorrow. Please take note: {schedule.Note}. Try to complete your study schedule on time!",
                                    usersReceive
                                );

                            schedule.Status = (int)SendNotiScheduleStatus.SentEnd;
                        }
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }










    }
}
