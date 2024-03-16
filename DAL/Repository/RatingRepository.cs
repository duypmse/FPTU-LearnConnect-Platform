using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IRatingRepository : IBaseRepository<RatingDTO> 
    {
        public RatingDTO RatingMentor(int userId, int mentorId, decimal rating, string comment);
        public RatingDTO RatingCourse(int userId, int courseId, decimal rating, string comment);
        public IEnumerable<object> GetListRatingOfCourse(int courseId);
        public IEnumerable<object> GetListRatingOfMentor(int userId);
        public IEnumerable<object> GetAllRatings(string ratingType);
        public RatingDTO UpdateRatingStatus(int ratingId, RatingStatus status);


    }
    public class RatingRepository : IRatingRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationRepository notificationRepository;

        public RatingRepository(LearnConnectDBContext context, IMapper mapper, INotificationRepository notificationRepository)
        {
            _context = context;
            _mapper = mapper;
            this.notificationRepository = notificationRepository;
        }
        public RatingDTO Add(RatingDTO _objectDTO)
        {
            var _object = _mapper.Map<Rating>(_objectDTO);
            _context.Ratings.Add(_object);
            _context.SaveChanges();
            _objectDTO.Id = _object.Id;
            return _objectDTO;
        }


        public RatingDTO Get(int id)
        {
            var _object = _context.Ratings.Find(id);
            var _objectDTO = _mapper.Map<RatingDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<RatingDTO> GetList()
        {
            var _list = _context.Ratings.ToList();
            var _listDTO = _mapper.Map<List<RatingDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, RatingDTO _objectDTO)
        {
            var _object = _context.Ratings.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.Ratings.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            Rating _object = _context.Ratings.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Ratings.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Ratings.Any(e => e.Id == id);
            return _isExist;
        }

        public RatingDTO RatingCourse(int userId, int courseId, decimal rating, string comment)
        {
            if (_context.Ratings.Any(r => r.RatingBy == userId && r.CourseId == courseId))
            {
                throw new Exception("The user has already rating this course");
            }

            var _object = new RatingDTO
            {
                RatingBy = userId,
                CourseId = courseId,
                Rating1 = rating,
                Comment = comment,
                Status = (int)RatingStatus.Show,
                TimeStamp = DateTime.UtcNow.AddHours(7)
            };

            var data = Add(_object);
            //_context.SaveChanges();

            var courseRatings = _context.Ratings.Where(r => r.CourseId == courseId && r.Status == (int)RatingStatus.Show).ToList();
            Decimal totalRating = 0;

            foreach (var r in courseRatings)
            {
                totalRating += r.Rating1;
            }
            var averageRating = totalRating / courseRatings.Count();

            var course = _context.Courses
                .Include(c => c.Mentor)
                .FirstOrDefault(c => c.Id == courseId);
            course.AverageRating = averageRating;

            if (data != null)
            {
                var usersReceive = _context.Users.Where(u => u.Id == course.Mentor.UserId).Select(u => u.Id).ToArray();
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null && course != null)
                {
                    notificationRepository.Create(
                        "New Rating",
                        $"Your course {course.Name} has a new rating from student {user.FullName}",
                        usersReceive
                    );
                }
            }

            _context.SaveChanges();


            return data;
        }

        public RatingDTO RatingMentor(int userId, int mentorId, decimal rating, string comment)
        {
            var mentorRating = _context.Mentors
                .Include(m => m.User)
                .Where(m => m.Status == (int)MentorStatus.Active &&  m.UserId == mentorId).FirstOrDefault();
            
            if (mentorRating == null)
            {
                throw new Exception("Mentor not found");
            }

            if (_context.Ratings.Any(r => r.RatingBy == userId && r.MentorId == mentorRating.Id))
            {
                throw new Exception("The user has already rating this mentor");
            }
            var _object = new RatingDTO
            {
                RatingBy = userId,
                MentorId = mentorRating.Id,
                Rating1 = rating,
                Comment = comment,
                Status = (int)RatingStatus.Show,
                TimeStamp = DateTime.UtcNow.AddHours(7)
            };
            var data = Add(_object);

            var mentorRatings = _context.Ratings.Where(r => r.MentorId == mentorRating.Id && r.Status == (int)RatingStatus.Show).ToList();
            Decimal totalRating = 0;

            foreach (var r in mentorRatings)
            {
                totalRating += r.Rating1;
            }
            var averageRating = totalRating / mentorRatings.Count();

            mentorRating.AverageRating = averageRating;

            if (data != null)
            {
                var usersReceive = _context.Users.Where(u => u.Id == mentorId).Select(u => u.Id).ToArray();
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null && mentorRating != null)
                {
                    notificationRepository.Create(
                        "New Rating",
                        $"You have a new rating from student {user.FullName}",
                        usersReceive
                    );
                }
            }

            _context.SaveChanges();


            return data;
        }

        public IEnumerable<object> GetListRatingOfCourse(int courseId)
        {
            var courseRatings = _context.Ratings.Where(r => r.CourseId == courseId && r.Status == (int)RatingStatus.Show).ToList();

            var listRatingOfCourse = _mapper.Map<List<RatingDTO>>(courseRatings);

            var users = _context.Users.ToList();

            var _listUserDTO = _mapper.Map<List<UserDTO>>(users);

            var _listRatingMentorDTO = listRatingOfCourse.Join(_listUserDTO, rating => rating.RatingBy, user => user.Id, (rating, user) => new
            {
                RatingCourseInfo = rating,
                UserRatingInfo = new
                {
                    FullName = user.FullName,
                    ImageUser = user.ProfilePictureUrl
                }
            });

            return _listRatingMentorDTO;
        }

        public IEnumerable<object> GetListRatingOfMentor(int userId)
        {
            int mentorId = _context.Mentors
                    .Where(m => m.UserId == userId)
                    .Select(m => m.Id)
                    .FirstOrDefault();

            var mentorRatings = _context.Ratings.Where(r => r.MentorId == mentorId && r.Status == (int)RatingStatus.Show).ToList();
            var listRatingOfMentor = _mapper.Map<List<RatingDTO>>(mentorRatings);

            var users = _context.Users.ToList();

            var _listUserDTO = _mapper.Map<List<UserDTO>>(users);

            var _listRatingMentorDTO = listRatingOfMentor.Join(_listUserDTO, rating => rating.RatingBy, user => user.Id, (rating, user) => new
            {
                RatingMentorInfo = rating,
                UserRatingInfo = new
                {
                    FullName = user.FullName,
                    ImageUser = user.ProfilePictureUrl
                }
            });

            return _listRatingMentorDTO;
        }

        public IEnumerable<object> GetAllRatings(string ratingType)
        {
            var allRatings = _context.Ratings
                .Include(r => r.Course)
                .Include(r => r.Mentor)
                .ToList();

            if (ratingType.Equals("course", StringComparison.OrdinalIgnoreCase))
            {
                allRatings = allRatings.Where(r => r.CourseId != null).ToList();
            }
            else if (ratingType.Equals("mentor", StringComparison.OrdinalIgnoreCase))
            {
                allRatings = allRatings.Where(r => r.MentorId != null).ToList();
            }

            var listRatingOfAllItems = _mapper.Map<List<RatingDTO>>(allRatings);

            var userIds = listRatingOfAllItems.Select(rating => rating.RatingBy).Distinct().ToList();
            var users = _context.Users
                .Where(user => userIds.Contains(user.Id))
                .ToList();

            var _listUserDTO = _mapper.Map<List<UserDTO>>(users);

            var mentors = _context.Mentors
                .Include(m => m.User)
                .ToList();
            var courses = _context.Courses.ToList();

            var _listRatingMentorDTO = listRatingOfAllItems
                .Join(_listUserDTO,
                    rating => rating.RatingBy,
                    user => user.Id,
                    (rating, user) => new
                    {
                        RatingInfo = new
                        {
                            rating.Id,
                            rating.Rating1,
                            rating.Comment,
                            rating.TimeStamp,
                            rating.Status,
                            rating.RatingBy,
                            rating.CourseId,
                            CourseName = courses.FirstOrDefault(c => c.Id == rating.CourseId)?.Name,
                            CourseImage = courses.FirstOrDefault(c => c.Id == rating.CourseId)?.ImageUrl,
                            rating.MentorId,
                            MentorName = mentors.FirstOrDefault(m => m.Id == rating.MentorId)?.User.FullName,
                            MentorImage = mentors.FirstOrDefault(m => m.Id == rating.MentorId)?.User.ProfilePictureUrl
                        },
                        UserRatingInfo = new
                        {
                            FullName = user.FullName,
                            ImageUser = user.ProfilePictureUrl,
                            Email = user.Email
                        }
                    }).OrderByDescending(item => item.RatingInfo.TimeStamp);

            return _listRatingMentorDTO;
        }

        public RatingDTO UpdateRatingStatus(int ratingId, RatingStatus status)
        {
            var rating = _context.Ratings.Find(ratingId);

            if (rating == null)
            {
                throw new Exception("Rating not found");
            }

            // Update rating status
            rating.Status = (int)status;

            _context.SaveChanges();

            // Mapping to RatingDTO if needed
            var ratingDTO = _mapper.Map<RatingDTO>(rating);

            // Optionally, you can return the updated RatingDTO
            return ratingDTO;
        }



    }
}
