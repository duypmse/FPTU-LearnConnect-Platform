using AutoMapper;
using BAL.Models;
using DAL.DTO;
using DAL.Service;
using Firebase.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface ILectureRepository : IBaseRepository<LectureDTO> {
        IEnumerable<LectureDTO> GetLecturesByCourseId(int courseId);
        public IEnumerable<LectureDTO> GetLecturesByCourseId(int? userId, int courseId);
        LectureDTO GetLectureDetailByCourseId(int courseId, int lectureId);
        public LectureDTO CreateLecture(int userId, int courseId, string title, string content, IFormFile? contentFile, int contentType, string? contentUrl);
        public LectureDTO UpdateLecture(int userId, int courseId, int lectureId, string? title, string? content, IFormFile? contentFile, string? contentUrl, int contentType);
        public object ProcessLectureRequest(int lectureId, bool acceptRequest, string? note);

    }
    public class LectureRepository : ILectureRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IFirebaseService uploadService;
        private readonly IContentModerationRepository contentModerationRepo;
        private readonly IMapper _mapper;

        public LectureRepository(LearnConnectDBContext context, IFirebaseService uploadService, IContentModerationRepository contentModerationRepo, IMapper mapper)
        {
            _context = context;
            this.uploadService = uploadService;
            this.contentModerationRepo = contentModerationRepo;
            _mapper = mapper;
        }

        public LectureDTO Add(LectureDTO _objectDTO)
        {
            var _object = _mapper.Map<Lecture>(_objectDTO);
            var data = _context.Lectures.Add(_object).Entity;
            SaveChanges();
            return _mapper.Map<LectureDTO>(data);
        }

        public LectureDTO Get(int id)
        {
            var _object = _context.Lectures.Find(id);
            var _objectDTO = _mapper.Map<LectureDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<LectureDTO> GetList()
        {
            var _list = _context.Lectures.ToList();
            var _listDTO = _mapper.Map<List<LectureDTO>>(_list);
            return _listDTO;
        }

        public IEnumerable<LectureDTO> GetLecturesByCourseId(int courseId)
        {
            var lectures = _context.Lectures.Where(lecture => lecture.CourseId == courseId
            && lecture.Status != (int)LectureStatus.InActive).ToList();
            var lecturesDTO = _mapper.Map<List<LectureDTO>>(lectures);
            return lecturesDTO;
        }

        public IEnumerable<LectureDTO> GetLecturesByCourseId(int? userId, int courseId)
        {
            var lectures = _context.Lectures
                .Where(lecture => lecture.CourseId == courseId)
                .ToList();

            var user = _context.Users.Find(userId);

            var course = _context.Courses
                .Include(c => c.Mentor)
                .FirstOrDefault(c => c.Id == courseId);

            if(course == null)
            {
                throw new Exception("Not found Course!");
            }

            var enrollment = _context.Enrollments.FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId && (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed));

            if (userId == null || (enrollment == null && course.Mentor.UserId != userId))
            {
                lectures = lectures.Where(l => l.Status == (int)LectureStatus.Active).ToList();
            }
            else if (user?.Role == (int)Roles.Staff || course.Mentor.UserId == userId)
            {
                lectures = lectures.Where(l => l?.Status != null).ToList();
            }
            else if (enrollment != null)
            {
                lectures = lectures.Where(l => l.Status != (int)LectureStatus.InActive).ToList();
            }

            var lecturesDTO = _mapper.Map<List<LectureDTO>>(lectures);
            return lecturesDTO;
        }

        public LectureDTO GetLectureDetailByCourseId(int courseId, int lectureId)
        {
            var lecture = _context.Lectures.FirstOrDefault(l => l.CourseId == courseId && l.Id == lectureId);

            if (lecture == null)
            {
                return null; 
            }

            var lectureDTO = _mapper.Map<LectureDTO>(lecture);
            return lectureDTO;
        }

        public int Update(int id, LectureDTO _objectDTO)
        {
            var _object = _context.Lectures.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.Lectures.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            Lecture _object = _context.Lectures.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _object.Status = (int)LectureStatus.InActive;
            _context.Lectures.Update(_object);

            var course = _context.Courses.Find(_object.CourseId);
            course.LectureCount =  _context.Lectures.Where(l => l.CourseId == _object.Id && l.Status == (int)CourseStatus.Active).Count();
            _context.Courses.Update(course);

            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Lectures.Any(e => e.Id == id);
            return _isExist;
        }

        public LectureDTO CreateLecture(int userId, int courseId, string title, string content, IFormFile? contentFile, int contentType, string? contentUrl)
        {
            // Tìm Mentor có UserId trùng với userId được truyền vào
            var mentor = _context.Mentors.FirstOrDefault(m => m.UserId == userId);

            if (mentor == null)
            {
                // Nếu không tìm thấy mentor, bạn có thể trả về một thông báo lỗi hoặc xử lý lỗi theo ý muốn
                // Ví dụ: Trả về một đối tượng chứa thông báo lỗi
                throw new Exception("No mentor found for the provided UserId.");
            }
            if(contentFile != null)
            {
                var random = GenerateUniqueCourseId();
                var lectureNumber = _context.Lectures.Where(l => l.CourseId == courseId).Count();
                var imageName = "Course" + courseId + "_Lecture" + (lectureNumber + 1) + "_" + random;
                contentUrl = uploadService.Upload(contentFile, imageName, "lectures").Result;
            }

            var _object = new LectureDTO
            {
                Title = title,
                Content = content,
                ContentUrl = contentUrl,
                ContentType = contentType,
                Status = (int)LectureStatus.Pending,
                CourseId = courseId
            };

            var data = Add(_object);
            _context.SaveChanges();
            var course = _context.Courses.Find(courseId);
            var countLecture = _context.Lectures.Where(l => l.CourseId == courseId).Count();
            course.LectureCount = countLecture;
            course.ContentLength = 7;
            _context.SaveChanges();

            return data;
        }

        
        public LectureDTO UpdateLecture(int userId, int courseId, int lectureId, string? title, string? content, IFormFile? contentFile, string? contentUrl, int contentType)
        {
            var existingLecture = _context.Lectures.Find(lectureId);

            if (existingLecture == null)
            {
                throw new Exception("Lecture not found for the provided lectureId.");
            }

            if (existingLecture.CourseId != courseId)
            {
                throw new Exception("Lecture does not belong to the provided courseId.");
            }
            var courseLecture = _context.Courses.Find(existingLecture.CourseId);

            var mentor = _context.Mentors.Find(courseLecture.MentorId);

            if (mentor.UserId != userId)
            {
                throw new Exception("The user is not the mentor of the provided course.");
            }

            if (!string.IsNullOrEmpty(title))
            {
                existingLecture.Title = title;
            }

            if (!string.IsNullOrEmpty(content))
            {
                existingLecture.Content = content;
            }
            

            if (contentType != -1)
            {
                existingLecture.ContentType = (int)contentType;
            }
            

            if (contentFile != null)
            {
                var random = GenerateUniqueCourseId();
                var lectureNumber = _context.Lectures.Where(l => l.CourseId == courseId).Count();
                var srcName = "Course" + courseId + "_Lecture" + (lectureNumber + 1) + "_" + random;
                var newContentUrl = uploadService.Upload(contentFile, srcName, "lectures").Result;
                existingLecture.ContentUrl = newContentUrl;
            }

            if (contentUrl != null)
            {
                existingLecture.ContentUrl = contentUrl;
            }

            existingLecture.Status = (int)LectureStatus.Pending;

            var course = _context.Courses.Where(c => c.Id == courseId).FirstOrDefault();
            course.Status = (int)CourseStatus.Pending;
            course.LectureCount = _context.Lectures.Where(l => l.CourseId == course.Id && l.Status == (int)CourseStatus.Active).Count();
            _context.Courses.Update(course);

            _context.SaveChanges();

            return _mapper.Map<LectureDTO>(existingLecture);
        }

        public object ProcessLectureRequest(int lectureId, bool acceptRequest, string? note)
        {
            try
            {
                var existingLecture = _context.Lectures
                    .Where(l => l.Status == (int)LectureStatus.Pending)
                    .FirstOrDefault(l => l.Id == lectureId);

                var course = _context.Courses.Find(existingLecture.CourseId);

                if (existingLecture == null)
                {
                    throw new Exception("Lecture not found for the provided lectureId.");
                }

                existingLecture.Status = acceptRequest ? (int)LectureStatus.Active : (int)LectureStatus.Reject;
                existingLecture.RejectReason = (acceptRequest && note is null)  ? "Approved!" : note;

                var contentModeration = _context.ContentModerations.FirstOrDefault(c => c.LectureId == lectureId);
                if (contentModeration == null)
                {
                    throw new Exception("Please wait to moderation content.");
                }
                contentModeration.PreviewDate = DateTime.Now;
                contentModeration.Status = acceptRequest ? (int)ContentModerationStatus.Approve : (int)ContentModerationStatus.Reject;
                _context.SaveChanges();

                if (acceptRequest)
                {
                    course.LectureCount = _context.Lectures.Where(l => l.CourseId == course.Id && l.Status == (int)CourseStatus.Active).Count();
                }
                _context.SaveChanges();

                return new
                {
                    Message = acceptRequest
                        ? "Lecture request accepted successfully!"
                        : "Lecture request rejected successfully!",
                    Data = new
                    {
                        LectureId = lectureId,
                        AcceptRequest = acceptRequest,
                        Note = note
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Error = ex.Message
                };
            }
        }





        private int GenerateUniqueCourseId()
        {
            return (int)(DateTime.Now - new DateTime(2020, 1, 1)).TotalSeconds;
        }
    }
}
