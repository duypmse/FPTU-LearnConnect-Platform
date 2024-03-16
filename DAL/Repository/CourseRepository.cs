using AutoMapper;
using AutoMapper.Internal;
using BAL.Models;
using CloudinaryDotNet;
using DAL.DTO;
using DAL.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface ICourseRepository : IBaseRepository<CourseDTO>
    {
        public object GetCourseWithFavorite(int id, int userId);
        public IEnumerable<object> GetListWithFavorite(int userId);
        public IEnumerable<object> GetUserCoursesWithFavorite(int userId);
        public IEnumerable<object> GetListTop6WithFavorite(int userId);
        public object GetCourseAfterEnroll(int courseId, int userId);
        public IEnumerable<CourseDTO> GetCoursesByUserIdAndStatus(int userId);
        public IEnumerable<object> GetListCourseAfterEnroll(int userId);
        public object GetCourseByUserIdAndCourseId(int userId, int courseId);
        public IEnumerable<CourseDTO> GetListCourseMentor();
        public CourseDTO CreateCourse(int userId, string courseName, string description,
                                    string shortDescription, int price, int lecture,
                                    int contentLength, int categoryId, IFormFile courseImage);
        public CourseDTO UpdateCourse(int userId, int courseId, string? courseName, string? description,
            string? shortDescription, int price, int lectureCount, int contentLength, 
            int categoryId, IFormFile? courseImage);

        public IEnumerable<object> GetListCourseContainEnrolled(int userId);
        public IEnumerable<CourseDTO> GetListBySpecializationId(int specializationId);
        public IEnumerable<object> GetListWithFilterAuthen(int userId,
                                                     int? specializationId = null,
                                                     decimal? priceMin = null,
                                                     decimal? priceMax = null,
                                                     decimal? minAverageRating = null,
                                                     bool orderByLatestCreationDate = false,
                                                     bool orderByEnrollmentCount = false,
                                                     string? searchQuery = null);
        public IEnumerable<CourseDTO> GetListWithFilter(int? specializationId = null,
                                                        decimal? priceMin = null,
                                                        decimal? priceMax = null,
                                                        decimal? minAverageRating = null,
                                                        bool orderByLatestCreationDate = false,
                                                        bool orderByEnrollmentCount = false,
                                                        string? searchQuery = null);
        public IEnumerable<CourseDTO> GetListCourseByStatus(CourseStatus courseStatus);
        public object ProcessCourseRequest(int courseId, bool acceptRequest, string? note);
        public CourseDTO GetCoursePending(int id);
        public object GetCourseDetailsByMentor(int mentorUserId, int courseId);

        //public string BanCourse(int courseId, bool isBan);
        public string BanCourse(int courseId, bool isBan, string note);

    }
    public class CourseRepository : ICourseRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;
        private readonly IFirebaseService uploadService;
        private readonly INotificationRepository notificationRepository;


        public CourseRepository(LearnConnectDBContext context, IMapper mapper, IFirebaseService uploadService, INotificationRepository notificationRepository)
        {
            _context = context;
            _mapper = mapper;
            this.uploadService = uploadService;
            this.notificationRepository = notificationRepository;
        }
        public IEnumerable<CourseDTO> GetList()
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .Where(c => c.Status == (int)CourseStatus.Active)
                .ToList();

            var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);

            foreach (var courseDTO in courseDTOs)
            {
                courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseDTO.Id && r.Status == (int)RatingStatus.Show);
            }

            return courseDTOs;
        }

        public IEnumerable<CourseDTO> GetListWithFilter(int? specializationId = null,
                                                        decimal? priceMin = null,
                                                        decimal? priceMax = null,
                                                        decimal? minAverageRating = null,
                                                        bool orderByLatestCreationDate = false,
                                                        bool orderByEnrollmentCount = false,
                                                        string? searchQuery = null)
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .OrderByDescending(c => c.TotalEnrollment)
                .Where(c => c.Status == (int)CourseStatus.Active);

            if (specializationId.HasValue)
            {
                courses = courses.Where(c => c.SpecializationId == specializationId.Value);
            }

            if (priceMin.HasValue || priceMax.HasValue)
            {
                courses = FilterCoursesByPrice(courses, priceMin, priceMax);
            }

            if (minAverageRating.HasValue)
            {
                courses = courses.Where(c => c.AverageRating >= minAverageRating.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                courses = courses.Where(c => c.Name.Contains(searchQuery));
            }else
            {
                courses.ToList();
            }

            if (orderByLatestCreationDate)
            {
                courses = courses.OrderByDescending(c => c.CreateDate);
            }
            else if (orderByEnrollmentCount)
            {
                courses = courses.OrderByDescending(c => c.TotalEnrollment);
            }

            var courseDTOs = _mapper.Map<List<CourseDTO>>(courses.ToList());

            foreach (var courseDTO in courseDTOs)
            {
                courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseDTO.Id && r.Status == (int)RatingStatus.Show);
            }

            courseDTOs = courseDTOs.OrderByDescending(c => c.TotalRatingCount).ToList();

            return courseDTOs;
        }

        public IEnumerable<object> GetListCourseContainEnrolled(int userId)
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .Where(c => c.Status == (int)CourseStatus.Active)
                .ToList();

            var enrollmentWithInProcessingOrCompletedStatus = _context.Enrollments
                .Where(e =>e.UserId == userId &&
                            (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed))
                .ToList();

            

            var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);

            foreach (var courseDTO in courseDTOs)
            {
                courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseDTO.Id && r.Status == (int)RatingStatus.Show);

                var isEnroll = enrollmentWithInProcessingOrCompletedStatus
                            .Any(e => e.CourseId == courseDTO.Id && e.UserId == userId);
                if (isEnroll)
                {
                    courseDTO.Enrolled = true;

                }else
                {
                    courseDTO.Enrolled = false;
                }

            }

            return courseDTOs;
        }


        public IEnumerable<object> GetListWithFavorite(int userId)
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .Include(c => c.Enrollments)
                .Where(c => c.Status == (int)CourseStatus.Active)
                .ToList();
            //var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);


            var listCourseIdFavorite = _context.FavoriteCourses.Where(f => f.UserId == userId).Select(f => f.FavoriteCourseId).ToList();
            var returnCourses = new List<object>();
            foreach (var course in courses)
            {
                var returnCourse = new
                {
                    course.Id,
                    course.Name,
                    course.Description,
                    course.ShortDescription,
                    course.ImageUrl,
                    course.Price,
                    course.TotalEnrollment,
                    course.LectureCount,
                    course.ContentLength,
                    course.AverageRating,
                    course.CreateDate,
                    course.Status,
                    course.SpecializationId,
                    course.MentorId,
                    Enrolled = course.Enrollments.Any(e => e.UserId == userId && (e.Status == (int)EnrollmentStatus.Completed || e.Status == (int)EnrollmentStatus.InProcessing)),

                    //courseDTO.CategoryName,
                    //courseDTO.MentorName,
                    //courseDTO.MentorProfilePictureUrl,
                    TotalRatingCount = _context.Ratings.Count(r => r.CourseId == course.Id && r.Status == (int)RatingStatus.Show),
                    IsFavorite = listCourseIdFavorite.Contains(course.Id),
                };
                returnCourses.Add(returnCourse);
            };

            return returnCourses;
        }

        public IEnumerable<object> GetListWithFilterAuthen(int userId,
                                                     int? specializationId = null,
                                                     decimal? priceMin = null,
                                                     decimal? priceMax = null,
                                                     decimal? minAverageRating = null,
                                                     bool orderByLatestCreationDate = false,
                                                     bool orderByEnrollmentCount = false,
                                                     string? searchQuery = null)
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.TotalEnrollment)
                .Where(c => c.Status == (int)CourseStatus.Active);

            if (specializationId.HasValue)
            {
                courses = courses.Where(c => c.SpecializationId == specializationId.Value);
            }

            if (priceMin.HasValue || priceMax.HasValue)
            {
                courses = FilterCoursesByPrice(courses, priceMin, priceMax);
            }

            if (minAverageRating.HasValue)
            {
                courses = courses.Where(c => c.AverageRating >= minAverageRating.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                courses = courses.Where(c => c.Name.Contains(searchQuery));
            }
            else
            {
                courses.ToList();
            }

            if (orderByLatestCreationDate)
            {
                courses = courses.OrderByDescending(c => c.CreateDate);
            }
            else if (orderByEnrollmentCount)
            {
                courses = courses.OrderByDescending(c => c.TotalEnrollment);
            }

            var courseDTOs = _mapper.Map<List<CourseDTO>>(courses.ToList());

            foreach (var courseDTO in courseDTOs)
            {
                courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseDTO.Id && r.Status == (int)RatingStatus.Show);
            }

            courseDTOs = courseDTOs.OrderByDescending(c => c.TotalRatingCount).ToList();


            var listCourseIdFavorite = _context.FavoriteCourses
                .Where(f => f.UserId == userId)
                .Select(f => f.FavoriteCourseId)
                .ToList();

            var returnCourses = new List<object>();

            foreach (var course in courses.ToList())
            {
                var returnCourse = new
                {
                    course.Id,
                    course.Name,
                    course.Description,
                    course.ShortDescription,
                    course.ImageUrl,
                    course.Price,
                    course.TotalEnrollment,
                    course.LectureCount,
                    course.ContentLength,
                    course.AverageRating,
                    course.CreateDate,
                    course.Status,
                    course.SpecializationId,
                    course.MentorId,
                    Enrolled = course.Enrollments.Any(e => e.UserId == userId && (e.Status == (int)EnrollmentStatus.Completed || e.Status == (int)EnrollmentStatus.InProcessing)),
                    TotalRatingCount = _context.Ratings.Count(r => r.CourseId == course.Id && r.Status == (int)RatingStatus.Show),
                    IsFavorite = listCourseIdFavorite.Contains(course.Id),
                };

                returnCourses.Add(returnCourse);
            }
            
            // Sort the returnCourses list based on TotalEnrollment in descending order

            return returnCourses;
        }


        private IQueryable<Course> FilterCoursesByPrice(IQueryable<Course> courses, decimal? priceMin, decimal? priceMax)
        {
            if (priceMin.HasValue && priceMax.HasValue)
            {
                return courses.Where(c => c.Price >= priceMin.Value && c.Price <= priceMax.Value);
            }
            else if (priceMin.HasValue)
            {
                return courses.Where(c => c.Price >= priceMin.Value);
            }
            else if (priceMax.HasValue)
            {
                return courses.Where(c => c.Price <= priceMax.Value);
            }
            else
            {
                // If neither priceMin nor priceMax is provided, include free courses
                return courses.Where(c => c.Price == 0);
            }
        }

        public IEnumerable<object> GetListTop6WithFavorite(int userId)
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .Include(c => c.Enrollments)
                .Where(c => c.Status == (int)CourseStatus.Active)
                .ToList();
            //var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);
            var topCourses = courses.OrderByDescending(c => c.TotalEnrollment ?? 0).Take(6).ToList();

            var listCourseIdFavorite = _context.FavoriteCourses.Where(f => f.UserId == userId).Select(f => f.FavoriteCourseId).ToList();
            var returnCourses = new List<object>();
            foreach (var course in topCourses)
            {
                var returnCourse = new
                {
                    course.Id,
                    course.Name,
                    course.Description,
                    course.ShortDescription,
                    course.ImageUrl,
                    course.Price,
                    course.TotalEnrollment,
                    course.LectureCount,
                    course.ContentLength,
                    course.AverageRating,
                    course.CreateDate,
                    course.Status,
                    course.SpecializationId,
                    course.MentorId,
                    Enrolled = course.Enrollments.Any(e => e.UserId == userId && (e.Status == (int)EnrollmentStatus.Completed || e.Status == (int)EnrollmentStatus.InProcessing)),
                    //courseDTO.CategoryName,
                    //courseDTO.MentorName,
                    //courseDTO.MentorProfilePictureUrl,
                    TotalRatingCount = _context.Ratings.Count(r => r.CourseId == course.Id && r.Status == (int)RatingStatus.Show),
                    IsFavorite = listCourseIdFavorite.Contains(course.Id),
                };
                returnCourses.Add(returnCourse);
            };

            return returnCourses;
        }

        public IEnumerable<CourseDTO> GetListCourseMentor()
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .ToList();

            var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);

            foreach (var courseDTO in courseDTOs)
            {
                courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseDTO.Id && r.Status == (int)RatingStatus.Show);
                courseDTO.LecturePendingCount = _context.Lectures.Count(l => l.CourseId == courseDTO.Id && l.Status == (int)LectureStatus.Pending);
            }

            return courseDTOs;
        }

        public IEnumerable<CourseDTO> GetListCourseByStatus(CourseStatus courseStatus)
        {
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .Where(c => c.Status == (int)courseStatus)
                .OrderByDescending(c => c.CreateDate)
                .ToList();

            var courseDTOs = _mapper.Map<List<CourseDTO>>(courses);
            foreach (var courseDTO in courseDTOs)
            {
                courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseDTO.Id && r.Status == (int)RatingStatus.Show);
                var userMentor = _context.Users.FirstOrDefault(u => _context.Mentors.FirstOrDefault(m => m.Id == courseDTO.MentorId).UserId == u.Id);
                courseDTO.MentorName = userMentor.FullName;
                //MentorImage = courseDTO.MentorProfilePictureUrl;
                courseDTO.MentorProfilePictureUrl = userMentor.ProfilePictureUrl;
                courseDTO.LecturePendingCount = _context.Lectures.Count(l => l.CourseId == courseDTO.Id && l.Status == (int)LectureStatus.Pending);
            }

            return courseDTOs;
        }

        public CourseDTO Get(int id)
        {
            var course = _context.Courses.Where(c => c.Status == (int)CourseStatus.Active)
                .Include(c => c.Specialization)
                .Include(c => c.Mentor.User)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return null;
            }

            var courseDTO = _mapper.Map<CourseDTO>(course);
            courseDTO.MentorName = course.Mentor.User.FullName;
            courseDTO.SpecializationName = course.Specialization.Name;
            courseDTO.MentorProfilePictureUrl = course.Mentor.User.ProfilePictureUrl;
            courseDTO.MentorId = course.Mentor.Id;

            var totalRatingCount = _context.Ratings.Count(r => r.CourseId == id && r.Status == (int)RatingStatus.Show);
            courseDTO.TotalRatingCount = totalRatingCount;

            return courseDTO;
        }

        public object GetCourseWithFavorite(int id, int userId)
        {
            if(userId == 0)
            {
                var course = _context.Courses
                //.Where(c => c.Status == (int)CourseStatus.Active)
                .Include(c => c.Specialization)
                .Include(c => c.Mentor.User)
                .FirstOrDefault(c => c.Id == id);

                if (course == null)
                {
                    return null;
                }

                var courseDTO = _mapper.Map<CourseDTO>(course);
                courseDTO.MentorName = course.Mentor.User.FullName;
                courseDTO.SpecializationName = course.Specialization.Name;
                courseDTO.MentorProfilePictureUrl = course.Mentor.User.ProfilePictureUrl;
                courseDTO.MentorId = course.Mentor.UserId;

                var totalRatingCount = _context.Ratings.Count(r => r.CourseId == id && r.Status == (int)RatingStatus.Show);
                courseDTO.TotalRatingCount = totalRatingCount;

                return courseDTO;
            } else
            {
                var course = _context.Courses
                .Include(c => c.Specialization)
                .Include(c => c.Mentor.User)
                .Where(c => c.Status == (int)CourseStatus.Active)
                .FirstOrDefault(c => c.Id == id);

                var returnCourse = new
                {
                    course.Id,
                    course.Name,
                    course.Description,
                    course.ShortDescription,
                    course.ImageUrl,
                    course.Price,
                    course.TotalEnrollment,
                    course.LectureCount,
                    course.ContentLength,
                    course.AverageRating,
                    course.CreateDate,
                    course.Note,
                    course.Status,
                    course.SpecializationId,
                    course.MentorId,
                    MentorUserId = course.Mentor.UserId,
                    Enrolled = _context.Enrollments.Any(e => e.CourseId == id && e.UserId == userId && (e.Status == (int)EnrollmentStatus.Completed || e.Status == (int)EnrollmentStatus.InProcessing)),
                    MentorName = course.Mentor.User.FullName,
                    SpecializationName = course.Specialization.Name,
                    MentorProfilePictureUrl = course.Mentor.User.ProfilePictureUrl,
                    TotalRatingCount = _context.Ratings.Count(r => r.CourseId == course.Id && r.Status == (int)RatingStatus.Show),
                    IsFavorite = _context.FavoriteCourses.Any(f => f.UserId == userId && f.FavoriteCourseId == id)
                };
                    

                return returnCourse;
            }
        }

        public CourseDTO GetCoursePending(int id)
        {
            var course = _context.Courses
                //.Where(c => c.Status == (int)CourseStatus.Pending)
                .Include(c => c.Specialization)
                .Include(c => c.Mentor.User)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return null;
            }

            var courseDTO = _mapper.Map<CourseDTO>(course);
            courseDTO.MentorName = course.Mentor.User.FullName;
            courseDTO.SpecializationName = course.Specialization.Name;
            courseDTO.MentorProfilePictureUrl = course.Mentor.User.ProfilePictureUrl;
            courseDTO.MentorId = course.Mentor.Id;

            var totalRatingCount = _context.Ratings.Count(r => r.CourseId == id && r.Status == (int)RatingStatus.Show);
            courseDTO.TotalRatingCount = totalRatingCount;

            return courseDTO;
        }



        public CourseDTO Add(CourseDTO _objectDTO)
        {
            
            _objectDTO.Id = 0;
            _objectDTO.CreateDate = DateTime.Now;
            _objectDTO.Status = 1;
            _objectDTO.AverageRating = 0;
            _objectDTO.TotalEnrollment = 0;
            var courseEntity = _mapper.Map<Course>(_objectDTO);
            if (!string.IsNullOrEmpty(_objectDTO.ImageUrl))
            {
                courseEntity.ImageUrl = _objectDTO.ImageUrl;
            }

            var data = _context.Courses.Add(courseEntity).Entity;
            SaveChanges();

            return _mapper.Map<CourseDTO>(data);
        }

        public int Update(int id, CourseDTO _objectDTO)
        {
            var existingObject = _context.Courses.Include(c => c.Specialization).FirstOrDefault(course => course.Id == id);

            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            Course _object = _context.Courses.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Courses.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Courses.Any(e => e.Id == id);
            return _isExist;
        }

        public object GetCourseAfterEnroll(int courseId, int userId)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId);
            if (enrollment == null)
            {
                return null;
            }

            var course = _context.Courses
                .Include(c => c.Specialization)
                .Include(c => c.Mentor.User)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                return null;
            }

            var courseDTO = _mapper.Map<CourseDTO>(course);
            courseDTO.MentorName = course.Mentor.User.FullName;
            courseDTO.SpecializationName = course.Specialization.Name;
            courseDTO.MentorProfilePictureUrl = course.Mentor.User.ProfilePictureUrl;
            courseDTO.MentorId = course.MentorId;

            courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseId && r.Status == (int)RatingStatus.Show); 

            var listLectures = _context.Lectures.Where(lecture => lecture.CourseId == courseId).Select(lecture => lecture.Title).ToList();
            var countLecture = listLectures.Count;

            var percentComplete = _context.Enrollments.FirstOrDefault(l => l.CourseId == courseId && l.UserId == userId);
            if(percentComplete.PercentComplete == null)
            {
                percentComplete.PercentComplete = 0;
            }
            var courseAfterEnroll = new
            {
                Course = courseDTO,
                LectureCount = countLecture,
                ListLectures = listLectures,
                MentorId = course.MentorId,
                MentorName = course.Mentor.User.FullName,
                PercentComplete = percentComplete != null ? percentComplete.PercentComplete : 0
            };
            return courseAfterEnroll;
        }

        public IEnumerable<object> GetListCourseAfterEnroll(int userId)
        {
            var enrolledCourses = GetCoursesByUserIdAndStatus(userId);
            var courseAfterEnrollList = new List<object>();

            foreach (var course in enrolledCourses)
            {
                var courseAfterEnroll = GetCourseAfterEnroll(course.Id, userId);
                if (courseAfterEnroll != null)
                {
                    courseAfterEnrollList.Add(courseAfterEnroll);
                }
            }

            return courseAfterEnrollList;
        }

        public IEnumerable<CourseDTO> GetCoursesByUserIdAndStatus(int userId)
        {
            var enrolledCourses = _context.Enrollments
                .Where(enrollment => enrollment.UserId == userId && (enrollment.Status == (int)EnrollmentStatus.InProcessing || enrollment.Status == (int)EnrollmentStatus.Completed))
                .OrderByDescending(enrollment => enrollment.PaymentTransactions.First().SuccessDate)
                .Select(enrollment => enrollment.Course);

            var courseDTOs = _mapper.Map<List<CourseDTO>>(enrolledCourses);

            foreach (var courseDTO in courseDTOs)
            {
                courseDTO.TotalRatingCount = _context.Ratings.Count(r => r.CourseId == courseDTO.Id && r.Status == (int)RatingStatus.Show);
            }

            courseDTOs = courseDTOs.OrderByDescending(c => c.TotalRatingCount).ToList();

            return courseDTOs;
        }

        public IEnumerable<object> GetUserCoursesWithFavorite(int userId)
        {
            var enrolledCourses = _context.Enrollments
                .Where(enrollment => enrollment.UserId == userId && 
                                    (enrollment.Status == (int)EnrollmentStatus.InProcessing || enrollment.Status == (int)EnrollmentStatus.Completed))
                .Include(enrollment => enrollment.Course.Specialization)
                .Include(enrollment => enrollment.Course.Mentor)
                //.Include(enrollment => enrollment.Course.LearningProcesses)
                .Include(enrollment => enrollment.Course.Lectures)
                .Select(enrollment => enrollment.Course).ToList();

            var listCourseIdFavorite = _context.FavoriteCourses.Where(f => f.UserId == userId).Select(f => f.FavoriteCourseId).ToList();
            var returnCourses = new List<object>();
            foreach (var course in enrolledCourses)
            {
                var percentComplete = _context.Enrollments.FirstOrDefault(l => l.CourseId == course.Id && l.UserId == userId);
                var returnCourse = new
                {
                    Course = course,
                    TotalRatingCount = _context.Ratings.Count(r => r.CourseId == course.Id && r.Status == (int)RatingStatus.Show),
                    IsFavorite = listCourseIdFavorite.Contains(course.Id),
                    PercentComplete = percentComplete != null ? percentComplete.PercentComplete : 0,
                };
                returnCourses.Add(returnCourse);
            };


            return returnCourses;
        }

        public object GetCourseByUserIdAndCourseId(int userId, int courseId)
        {
            var enrollmentWithInProcessingOrCompletedStatus = _context.Enrollments
                .Where(e => e.CourseId == courseId && e.UserId == userId &&
                            (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed))
                .FirstOrDefault();

            var course = Get(courseId);

            if (course == null)
            {
                return null;
            }

            course.Enrolled = enrollmentWithInProcessingOrCompletedStatus != null;
            return course;
        }

        public object GetCourseDetailsByMentor (int mentorUserId, int courseId)
        {
            var course = _context.Courses
                .Include(c => c.Specialization)
                .Include(c => c.Mentor.User)
                .Where(c => c.Id == courseId && c.Mentor.UserId == mentorUserId)
                .FirstOrDefault();

            if (course == null)
            {
                return null;
            }
            var courseDTOs = _mapper.Map<CourseDTO>(course);
            courseDTOs.MentorName = course.Mentor.User.FullName;
            courseDTOs.SpecializationName = course.Specialization.Name;
            courseDTOs.MentorProfilePictureUrl = course.Mentor.User.ProfilePictureUrl;
            courseDTOs.MentorId = course.Mentor.Id;

            var totalRatingCount = _context.Ratings.Count(r => r.CourseId == courseId && r.Status == (int)RatingStatus.Show);
            courseDTOs.TotalRatingCount = totalRatingCount;

            return courseDTOs;
        }



        public CourseDTO CreateCourse(int userId,
                                      string courseName,
                                      string description,
                                      string shortDescription,
                                      int price,
                                      int lectureCount,
                                      int contentLength,
                                      int specializationId,
                                      IFormFile courseImage)
        {

            // Tìm Mentor có UserId trùng với userId được truyền vào
            var mentor = _context.Mentors.FirstOrDefault(m => m.UserId == userId);

            if (mentor == null)
            {
                // Nếu không tìm thấy mentor, bạn có thể trả về một thông báo lỗi hoặc xử lý lỗi theo ý muốn
                // Ví dụ: Trả về một đối tượng chứa thông báo lỗi
                throw new Exception("No mentor found for the provided UserId.");
            }

            int mentorId = mentor.Id;

            int courseId = GenerateUniqueCourseId();

            var imageName = "Course Image" + courseId;
            var imageUrl = uploadService.Upload(courseImage, imageName, "CourseImage").Result;

            var _object = new CourseDTO
            {
                Name = courseName,
                Description = description,
                ShortDescription = shortDescription,
                ImageUrl = imageUrl,
                Price = price,
                TotalEnrollment = 0,
                LectureCount = lectureCount,
                ContentLength = 0,
                AverageRating = 0,
                CreateDate = DateTime.UtcNow.AddHours(7),
                Status = 1,
                SpecializationId = specializationId,
                MentorId = mentorId
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
                        "New Course",
                        $"Course {course.Name} of mentor {user.FullName} need to be approved",
                        usersStaff
                    );
                }
            }

            return data;
        }

        private int GenerateUniqueCourseId()
        {
            // Cài đặt mã logic để tạo courseId duy nhất, ví dụ: dựa trên thời gian hiện tại
            // Đây là một ví dụ đơn giản:
            return (int)(DateTime.Now - new DateTime(2020, 1, 1)).TotalSeconds;
        }

        public CourseDTO UpdateCourse(int userId, int courseId, string? courseName, string? description,
            string? shortDescription, int price, int lectureCount, int contentLength, 
            int specializationId, IFormFile? courseImage)
        {
            // Tìm khóa học theo courseId
            var existingCourse = _context.Courses
                .Include(c => c.Mentor)
                .FirstOrDefault(c => c.Id == courseId);

            if (existingCourse == null)
            {
                throw new Exception("Course not found for the provided courseId.");
            }

            // Kiểm tra xem mentor của khóa học có user ID trùng với user ID được truyền vào không
            if (existingCourse.Mentor.UserId != userId)
            {
                throw new Exception("You do not have permission to update this course.");
            }

            if (!string.IsNullOrEmpty(courseName))
            {
                existingCourse.Name = courseName;
            }
            if (!string.IsNullOrEmpty(description))
            {
                existingCourse.Description = description;
            }
            if (!string.IsNullOrEmpty(shortDescription))
            {
                existingCourse.ShortDescription = shortDescription;
            }
            if (courseImage != null)
            {
                int courseImgageName = GenerateUniqueCourseId();

                // Cập nhật thông tin khóa học
                var imageName = "Course Image" + courseImgageName;
                var newimageUrl = uploadService.Upload(courseImage, imageName, "CourseImage").Result;
                existingCourse.ImageUrl = newimageUrl;

            }
            if (price != 0)
            {
                existingCourse.Price = price;
            }
            if (lectureCount != 0)
            {
                existingCourse.LectureCount = lectureCount;
            }
            if (contentLength != 0)
            {
                existingCourse.ContentLength = contentLength;
            }
            if (specializationId != 0)
            {
                existingCourse.SpecializationId = specializationId;
            }
            existingCourse.Status = (int)CourseStatus.Pending;
            existingCourse.CreateDate = DateTime.UtcNow.AddHours(7);



            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            return _mapper.Map<CourseDTO>(existingCourse);
        }

        public IEnumerable<CourseDTO> GetListBySpecializationId(int specializationId)
        {
            var _list = _context.Courses.Where(s => s.SpecializationId == specializationId && s.Status == (int)CourseStatus.Active).ToList();
            var _listDTO = _mapper.Map<List<CourseDTO>>(_list);
            return _listDTO;
        }

        public object ProcessCourseRequest(int courseId, bool acceptRequest, string? note)
        {
            try
            {
                var existingCourse = _context.Courses
                    .Include(c => c.Lectures)
                    .Include(c => c.Tests)
                    .Where(c => c.Status == (int)CourseStatus.Pending)
                    .FirstOrDefault(c => c.Id == courseId);

                if (existingCourse == null)
                {
                    throw new Exception("Course not found for the provided courseId.");
                }

                existingCourse.Status = acceptRequest ? (int)CourseStatus.Active : (int)CourseStatus.Reject;
                existingCourse.Note = acceptRequest ? "Approved!" : note;

                foreach (var lecture in existingCourse.Lectures)
                {
                    lecture.Status = acceptRequest ? (int)LectureStatus.Active : (int)LectureStatus.Reject;
                }

                foreach (var test in existingCourse.Tests)
                {
                    test.Status = acceptRequest ? (int)TestStatus.Active: (int)TestStatus.Reject;
                }

                var course = _context.Courses
                    .Include(c => c.Mentor)
                    .FirstOrDefault(c => c.Id == courseId);
                var usersReceive = _context.Users.Where(u => u.Id == course.Mentor.UserId).Select(u => u.Id).ToArray();

                if (usersReceive != null && course != null)
                {
                    string notificationMessage = acceptRequest
                        ? $"Your {course.Name} course has just been approved."
                        : $"Your {course.Name} course request has been rejected. Reason: {note}";

                    notificationRepository.Create(
                        "New Response",
                        notificationMessage,
                        usersReceive
                    );
                }

                if (acceptRequest == true && course != null)
                {
                    var usersStudent = _context.Users.Where(u => u.Role == (int)Roles.Student).Select(u => u.Id).ToArray();

                    if (usersStudent != null)
                    {
                        notificationRepository.Create(
                            "New Course",
                            $"Course {course.Name} has just been created, you can go to Courses to explore and enroll now!",
                            usersStudent
                        );
                    }
                }


                _context.SaveChanges();

                return new
                {
                    Message = acceptRequest
                        ? "Course request accepted successfully!"
                        : "Course request rejected successfully!",
                    Data = new
                    {
                        CourseId = courseId,
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

        /*public string BanCourse(int courseId, bool isBan)
        {
            var course = _context.Courses
                .Where(c => c.Id == courseId && (c.Status == (int)CourseStatus.Active|| c.Status == (int)CourseStatus.Pending || c.Status == (int)CourseStatus.Banned))
                .FirstOrDefault();

            if (course == null)
            {
                return "Course not found";
            }
            course.Status = isBan ? (int)CourseStatus.Banned : (int)CourseStatus.Active;

            *//*var lectures = _context.Lectures.Where(l => l.CourseId == course.Id).ToList();

            foreach (var lecture in lectures)
            {
                lecture.Status = isBan ? (int)LectureStatus.Banned : (int)LectureStatus.Pending;
            }*//*

            _context.SaveChanges();

            return isBan ? "Course banned successfully" : "Course unbanned successfully";
        }*/

        public string BanCourse(int courseId, bool isBan, string note)
        {
            var course = _context.Courses
                .Include(c => c.Mentor)
                .Where(c => c.Id == courseId && (c.Status == (int)CourseStatus.Active || c.Status == (int)CourseStatus.Pending || c.Status == (int)CourseStatus.Banned))
                .FirstOrDefault();

            if (course == null)
            {
                return "Course not found";
            }
            course.Status = isBan ? (int)CourseStatus.Banned : (int)CourseStatus.Active;
            course.Note = isBan ? note : note;

            var lectures = _context.Lectures.Where(l => l.CourseId == course.Id).ToList();

            foreach (var lecture in lectures)
            {
                lecture.Status = isBan ? (int)LectureStatus.Banned : (int)LectureStatus.Pending;
            }
            var usersReceive = _context.Users.Where(u => u.Id == course.Mentor.UserId).Select(u => u.Id).ToArray();

            if (usersReceive != null && course != null)
            {
                string notificationMessage = isBan
                    ? $"Your {course.Name} course has been banned. Reason: {note}. If you have any questions, please contact the support team via email contact.learnconnect@gmail.com"
                    : $"Unban course {course.Name}";

                notificationRepository.Create(
                    "New Notification",
                    notificationMessage,
                    usersReceive
                );
            }


            //_context.SaveChanges();

            return isBan ? "Course banned successfully" : "Course unbanned successfully";
        }

    }
}
