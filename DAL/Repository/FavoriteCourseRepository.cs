using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IFavoriteCourseRepository : IBaseRepository<FavoriteCourseDTO> {
        public int UnSetFavoriteCourse(int userId, int courseId);
        public IEnumerable<object> GetFavoriteCoursesByUser(int userId);
    }
    public class FavoriteCourseRepository : IFavoriteCourseRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public FavoriteCourseRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public FavoriteCourseDTO Add(FavoriteCourseDTO _objectDTO)
        {
            var _object = _mapper.Map<FavoriteCourse>(_objectDTO);
            _object.Id = 0;
            _context.FavoriteCourses.Add(_object);
            return null;
        }

        public FavoriteCourseDTO Get(int id)
        {
            var _object = _context.FavoriteCourses.Find(id);
            var _objectDTO = _mapper.Map<FavoriteCourseDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<FavoriteCourseDTO> GetList()
        {
            var _list = _context.FavoriteCourses.ToList();
            var _listDTO = _mapper.Map<List<FavoriteCourseDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, FavoriteCourseDTO _objectDTO)
        {
            var _object = _context.FavoriteCourses.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.FavoriteCourses.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            FavoriteCourse _object = _context.FavoriteCourses.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.FavoriteCourses.Remove(_object);
            return 1;
        }

        public int UnSetFavoriteCourse(int userId, int courseId)
        {
            FavoriteCourse _object = _context.FavoriteCourses.First(fc => fc.UserId == userId && fc.FavoriteCourseId == courseId);

            _context.FavoriteCourses.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.FavoriteCourses.Any(e => e.Id == id);
            return _isExist;
        }

        public IEnumerable<object> GetFavoriteCoursesByUser(int userId)
        {
            var favoriteCourses = _context.FavoriteCourses.Where(fc => fc.UserId == userId).ToList();
            var courses = _context.Courses
                .Include(c => c.Specialization)
                .Include(c => c.Enrollments);

            var favoriteCoursesWithCourseInfo = favoriteCourses
                .Join(courses, fc => fc.FavoriteCourseId, course => course.Id, (fc, course) => new
                {
                    Favorite = fc,
                    Course = new CourseDTO
                    {
                        Id = course.Id,
                        Name = course.Name,
                        Description = course.Description,
                        ShortDescription = course.ShortDescription,
                        ImageUrl = course.ImageUrl,
                        Price = course.Price,
                        TotalEnrollment = course.TotalEnrollment,
                        LectureCount = course.LectureCount,
                        ContentLength = course.ContentLength,
                        AverageRating = course.AverageRating,
                        CreateDate = course.CreateDate,
                        Status = course.Status,
                        SpecializationId = course.SpecializationId,
                        MentorId = course.MentorId,
                        SpecializationName = course.Specialization.Name,
                        TotalRatingCount = _context.Ratings.Count(r => r.CourseId == course.Id && r.Status == (int)RatingStatus.Show),
                        Enrolled = course.Enrollments.Any(e => e.UserId == userId && (e.Status == (int)EnrollmentStatus.Completed || e.Status == (int)EnrollmentStatus.InProcessing)),
                    }
                }).ToList();
            return favoriteCoursesWithCourseInfo;
        }
    }
}
