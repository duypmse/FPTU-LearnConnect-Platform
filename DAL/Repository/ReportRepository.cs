using AutoMapper;
using BAL.Models;
using CloudinaryDotNet;
using DAL.DTO;
using DAL.Service;
using Firebase.Auth;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IReportRepository : IBaseRepository<ReportDTO>
    {
        public ReportDTO ReportCourse(int userId, int courseId, string reportReason, string reportComment, IFormFile reportImage);
        public ReportDTO ReportMentor(int userId, int mentorId, string reportReason, string reportComment, IFormFile reportImage);
        public IEnumerable<object> GetAllReports(string reportType);
        public IEnumerable<object> GetReportsByCourseIdOrMentorId(int targetId, string reportType);

    }
    public class ReportRepository : IReportRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IFirebaseService uploadService;
        private readonly IMapper _mapper;
        private readonly INotificationRepository notificationRepository;


        public ReportRepository(LearnConnectDBContext context, IFirebaseService uploadService, IMapper mapper, INotificationRepository notificationRepository)
        {
            _context = context;
            this.uploadService = uploadService;
            _mapper = mapper;
            this.notificationRepository = notificationRepository;
        }
        public ReportDTO Add(ReportDTO _objectDTO)
        {
            var _object = _mapper.Map<Report>(_objectDTO);
            var data = _context.Reports.Add(_object).Entity;

            var dataDTO = _mapper.Map<ReportDTO>(data);
            return dataDTO;
        }

        public ReportDTO Get(int id)
        {
            var _object = _context.Reports.Find(id);
            var _objectDTO = _mapper.Map<ReportDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<ReportDTO> GetList()
        {
            var _list = _context.Reports.ToList();
            var _listDTO = _mapper.Map<List<ReportDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, ReportDTO _objectDTO)
        {
            var _object = _context.Reports.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.Reports.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            Report _object = _context.Reports.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Reports.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Reports.Any(e => e.Id == id);
            return _isExist;
        }

        public ReportDTO ReportCourse(int userId, int courseId, string reportReason, string reportComment, IFormFile reportImage)
        {
            if (_context.Reports.Any(r => r.ReportBy == userId && r.CourseId == courseId))
            {
                throw new Exception("The user has already reported this course");
            }

            var imageName = "userId_" + userId + "_report_course_" + courseId;
            object imageUrl;
            if (reportImage != null)
            {
                imageUrl = uploadService.Upload(reportImage, imageName, "Reports").Result;
            }
            else
            {
                imageUrl = null;
            }

            var _object = new ReportDTO
            {
                ReportBy = userId,
                CourseId = courseId,
                ReportType = reportReason,
                Description = reportComment,
                ImageUrl = (string)imageUrl,
                TimeStamp = DateTime.UtcNow.AddHours(7)
            };
            var data = Add(_object);
            if (data != null)
            {
                var usersStaff = _context.Users.Where(u => u.Role == (int)Roles.Staff).Select(u => u.Id).ToArray();
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                var course = _context.Courses.FirstOrDefault(c => c.Id == courseId);

                if (user != null && course != null)
                {
                    notificationRepository.Create(
                        "New Report",
                        $"Course {course.Name} has a new report from student {user.FullName}",
                        usersStaff
                    );
                }
            }


            return data;
        }

        public ReportDTO ReportMentor(int userId, int mentorId, string reportReason, string reportComment, IFormFile reportImage)
        {
            var mentorReport = _context.Mentors
                .Include(m => m.User)
                .Where(m => m.Status == (int)MentorStatus.Active && m.UserId == mentorId).FirstOrDefault();
            
            if (mentorReport == null)
            {
                throw new Exception("Mentor not found");
            }

            if (_context.Reports.Any(r => r.ReportBy == userId && r.MentorId == mentorReport.Id))
            {
                throw new Exception("The user has already reported this mentor");
            }

            var imageName = "userId_" + userId + "_report_mentor_" + mentorReport.Id;
            object imageUrl;
            if (reportImage != null)
            {
                imageUrl = uploadService.Upload(reportImage, imageName, "Reports").Result;
            }
            else
            {
                imageUrl = null;
            }

            var _object = new ReportDTO
            {
                ReportBy = userId,
                MentorId = mentorReport.Id,
                ReportType = reportReason,
                Description = reportComment,
                ImageUrl = (string)imageUrl,
                TimeStamp = DateTime.UtcNow.AddHours(7)
            };
            var data = Add(_object);
            if (data != null)
            {
                var usersStaff = _context.Users.Where(u => u.Role == (int)Roles.Staff).Select(u => u.Id).ToArray();
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null && mentorReport != null)
                {
                    notificationRepository.Create(
                        "New Report",
                        $"Mentor {mentorReport.User.FullName} has a new report from student {user.FullName}",
                        usersStaff
                    );
                }
            }

            return data;
        }

        public IEnumerable<object> GetAllReports(string reportType)
        {
            var allReports = _context.Reports
                .Include(r => r.Course)
                .Include(r => r.Mentor)
                .Where(r => r.Mentor.Status == (int)MentorStatus.Active || r.Course.Status == (int)CourseStatus.Active)
                .ToList();

            var mentors = _context.Mentors.Include(m => m.User).ToList();
            var users = _context.Users.ToList();

            if (reportType.Equals("course", StringComparison.OrdinalIgnoreCase))
            {
                allReports = allReports.Where(r => r.CourseId != null).ToList();
            }
            else if (reportType.Equals("mentor", StringComparison.OrdinalIgnoreCase))
            {
                allReports = allReports.Where(r => r.MentorId != null).ToList();
            }

            var listReports = _mapper.Map<List<ReportDTO>>(allReports);

            var groupedReports = listReports.OrderByDescending(r => r.TimeStamp)
                .GroupBy(report => new { report.CourseId, report.MentorId })
                .Select(group => new
                {
                    CourseInfo = group.Key.CourseId != null
                        ? new
                        {
                            CourseDetails = _mapper.Map<CourseDTO>(_context.Courses.FirstOrDefault(c => c.Id == group.Key.CourseId)),
                            ReportCount = group.Count(),
                        }
                        : null,
                    MentorInfo = group.Key.MentorId != null
                        ? new
                        {
                            MentorDetails = _mapper.Map<MentorDTO>(mentors.FirstOrDefault(m => m.Id == group.Key.MentorId)),
                            User = _mapper.Map<UserDTO>(mentors.FirstOrDefault(m => m.Id == group.Key.MentorId)?.User),
                            ReportCount = group.Count(),
                        }
                        : null
                })
                .ToList();

            return groupedReports;
        }

        public IEnumerable<object> GetReportsByCourseIdOrMentorId(int targetId, string reportType)
        {
            if (reportType.Equals("course", StringComparison.OrdinalIgnoreCase))
            {
                var reportDetails = _context.Reports.Where(r => r.CourseId == targetId).Include(r => r.ReportByNavigation);
                return reportDetails;
            }
            else if (reportType.Equals("mentor", StringComparison.OrdinalIgnoreCase))
            {
                var reportDetails = _context.Reports.Where(r => r.MentorId == targetId).Include(r => r.ReportByNavigation);
                return reportDetails;
            }

            return null;
        }






    }
}
