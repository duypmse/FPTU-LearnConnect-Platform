using BAL.Models;
using DAL;
using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing courses.
    /// </summary>
    [Route("api/course")]
    [ApiController]
    //[Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseRepository repo;
        private readonly IMentorRepository mentorRepository;

        public CourseController(ICourseRepository repo, IMentorRepository mentorRepository)
        {
            this.repo = repo;
            this.mentorRepository = mentorRepository;
        }

        /// <summary>
        /// Get a list of all courses
        /// </summary>
        /// <returns>A list of courses.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<EnrollmentDTO>> GetAllCourse()
        {
            try
            {
                var list = repo.GetList();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all courses with check user's favorite
        /// </summary>
        /// <returns>A list of courses.</returns>
        [HttpGet("courses-with-favorite")]
        [Authorize]
        public ActionResult<IEnumerable<object>> GetAllCourseWithFavorite(int userId, int currentPage, int pageSize)
        {
            var jwtId = int.Parse(User.FindFirst("Id").Value);
            if (jwtId != userId)
            {
                return BadRequest("Only user has userId = jwtDecode.Id");
            }
            try
            {
                var courses = repo.GetListWithFavorite(userId);
                if (currentPage != 0 && pageSize != 0)
                {
                    var count = courses.Count();
                    int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                    var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPage = currentPage > 1 ? true : false;
                    var nextPage = currentPage < totalPages ? true : false;

                    var paginationMetadata = new
                    {
                        count,
                        pageSize,
                        currentPage,
                        totalPages,
                        previousPage,
                        nextPage
                    };

                    return Ok(new
                    {
                        PaginationData = paginationMetadata,
                        ListCourse = coursesPaged
                    });
                }

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("courses-with-favorite-and-filter")]
        [Authorize]
        public ActionResult<IEnumerable<object>> GetListWithFilterAuthen(int userId,
                                                                   int currentPage,
                                                                   int pageSize,
                                                                   int? specializationId,
                                                                   decimal? priceMin = null,
                                                                   decimal? priceMax = null,
                                                                   decimal? minAverageRating = null,
                                                                   bool orderByLatestCreationDate = false,
                                                                   bool orderByEnrollmentCount = false,
                                                                   string? searchQuery = null)
        {
            var jwtId = int.Parse(User.FindFirst("Id").Value);
            if (jwtId != userId)
            {
                return BadRequest("Only user has userId = jwtDecode.Id");
            }
            try
            {
                    var courses = repo.GetListWithFilterAuthen(userId,
                                                         specializationId,
                                                         priceMin,
                                                         priceMax,
                                                         minAverageRating,
                                                         orderByLatestCreationDate,
                                                         orderByEnrollmentCount,
                                                         searchQuery);
                if (currentPage != 0 && pageSize != 0)
                {
                    var count = courses.Count();
                    int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                    var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPage = currentPage > 1 ? true : false;
                    var nextPage = currentPage < totalPages ? true : false;

                    var paginationMetadata = new
                    {
                        count,
                        pageSize,
                        currentPage,
                        totalPages,
                        previousPage,
                        nextPage
                    };

                    return Ok(new
                    {
                        PaginationData = paginationMetadata,
                        ListCourse = coursesPaged
                    });
                }

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a course by ID.
        /// </summary>
        /// <param name="id">The ID of the course.</param>
        /// <returns>The course with the specified ID.</returns>
        [HttpGet("{id}")]
        public IActionResult GetCourse(int id, int userId)
        {
            try
            {
                var course = repo.GetCourseWithFavorite(id, userId);
                if (course == null)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("get-course-pending/{id}")]
        [Authorize(Roles = "1")]
        public IActionResult GetCoursePending(int id)
        {
            try
            {
                var course = repo.GetCoursePending(id);
                if (course == null)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a course by ID.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "0,1")]
        public IActionResult DeleteCourse(int id)
        {
            try
            {
                var existingCourse = repo.Get(id);
                if (existingCourse == null)
                {
                    return NotFound();
                }

                repo.Delete(id);
                repo.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a course that Student enrolled by UserID.
        /// </summary>
        /// <param name="userId">The UserId of the enrollment.</param>
        /// <returns>The course with the specified UserId.</returns>
        [HttpGet("get-courses-by-userid")]
        [Authorize]
        public IActionResult GetCoursesByUserId(int userId, int currentPage, int pageSize)
        {
            var jwtId = int.Parse(User.FindFirst("Id").Value);
            if (!(jwtId == userId))
            {
                return BadRequest("Only user has userId = jwtDecode.Id");
            }

            try
            {
                //var courses = repo.GetListByUserId(userId);
                var courses = repo.GetListCourseAfterEnroll(userId);
                if (currentPage != 0 && pageSize != 0)
                {
                    var count = courses.Count();
                    int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                    var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPage = currentPage > 1 ? true : false;
                    var nextPage = currentPage < totalPages ? true : false;

                    var paginationMetadata = new
                        {
                            count,
                            pageSize,
                            currentPage,
                            totalPages,
                            previousPage,
                            nextPage
                        };

                    return Ok(new
                    {
                        PaginationData = paginationMetadata,
                        ListCourse = coursesPaged
                    });
                };

                return Ok(courses);
                // Object which we are going to send in header   

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user-courses-with-favorite")]
        [Authorize]
        public IActionResult GetUserCoursesWithFavorite(int userId, int currentPage, int pageSize)
        {
            var jwtId = int.Parse(User.FindFirst("Id").Value);
            if (!(jwtId == userId))
            {
                return BadRequest("Only user has userId = jwtDecode.Id");
            }
            try
            {
                var courses = repo.GetUserCoursesWithFavorite(userId);
                if (currentPage != 0 && pageSize != 0)
                {
                    var count = courses.Count();
                    int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                    var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPage = currentPage > 1 ? true : false;
                    var nextPage = currentPage < totalPages ? true : false;

                    var paginationMetadata = new
                    {
                        count,
                        pageSize,
                        currentPage,
                        totalPages,
                        previousPage,
                        nextPage
                    };

                    return Ok(new
                    {
                        PaginationData = paginationMetadata,
                        ListCourse = coursesPaged
                    });
                };

                return Ok(courses);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// Get a list of all courses with paging.
        /// </summary>
        /// <returns>A list of courses that have paged.</returns>
        [HttpGet("get-courses-paging")]
        public IActionResult GetCoursesPaging(int currentPage, int pageSize)
        {
            try
            {
                var courses = repo.GetList();
                var count = courses.Count();
                int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1 ? true : false;
                var nextPage = currentPage < totalPages ? true : false;

                var paginationMetadata = new
                {
                    count,
                    pageSize,
                    currentPage,
                    totalPages,
                    previousPage,
                    nextPage
                };

                return Ok(new
                {
                    PaginationData = paginationMetadata,
                    ListCourse = coursesPaged
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-courses-paging-with-filter")]
        public IActionResult GetCoursesPaging(int currentPage,
                                              int pageSize,
                                              int? specializationId = null,
                                              decimal? priceMin = null,
                                              decimal? priceMax = null,
                                              decimal? minAverageRating = null,
                                              bool orderByLatestCreationDate = false,
                                              bool orderByEnrollmentCount = false,
                                              string? searchQuery = null)
        {
            try
            {
                var courses = repo.GetListWithFilter(specializationId,
                                                     priceMin,
                                                     priceMax,
                                                     minAverageRating,
                                                     orderByLatestCreationDate,
                                                     orderByEnrollmentCount,
                                                     searchQuery).ToList();

                var count = courses.Count();
                int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1;
                var nextPage = currentPage < totalPages;

                var paginationMetadata = new
                {
                    count,
                    pageSize,
                    currentPage,
                    totalPages,
                    previousPage,
                    nextPage
                };

                return Ok(new
                {
                    PaginationData = paginationMetadata,
                    ListCourse = coursesPaged
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("get-courses-paging-by-user")]
        [Authorize(Roles = "2,3")]
        public IActionResult GetCoursesPagingByUser(int userId, int currentPage, int pageSize)
        {
            var jwtId = int.Parse(User.FindFirst("Id").Value);
            if (!(jwtId == userId))
            {
                return BadRequest("Access denied");
            }
            try
            {
                var courses = repo.GetListCourseContainEnrolled(userId);
                var count = courses.Count();
                int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1 ? true : false;
                var nextPage = currentPage < totalPages ? true : false;

                var paginationMetadata = new
                {
                    count,
                    pageSize,
                    currentPage,
                    totalPages,
                    previousPage,
                    nextPage
                };

                return Ok(new
                {
                    PaginationData = paginationMetadata,
                    ListCourse = coursesPaged
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// Get a list of the top 6 courses with the highest enrollment.
        /// </summary>
        /// <returns>A list of the top 6 courses with the highest enrollment.</returns>
        [HttpGet("get-top-enrolled-courses")]
        public IActionResult GetTopEnrolledCourses(int userId)
        {
            try
            {
                var courses = repo.GetListTop6WithFavorite(userId);

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the status of a course by ID.
        /// </summary>
        /// /// <param name="courseId">The ID of the course to update.</param>
        /// <param name="status">The new status for the course. Choose 0: "Active" or 1: "Inactive".</param>
        /// <returns>A response indicating the status update result.</returns>
        [HttpPut("update-course-status")]
        [Authorize(Roles = "0,1,2")]
        public IActionResult UpdateCourseStatus(int courseId, CourseStatus status)
        {
            try
            {
                var course = repo.Get(courseId);

                string statusName = Enum.GetName(typeof(SpecializationStatus), status);

                if (course == null)
                {
                    return NotFound("Course not found");
                }

                switch (status)
                {
                    case CourseStatus.Active:
                        course.Status = (int)CourseStatus.Active;
                        break;
                    case CourseStatus.Pending:
                        course.Status = (int)CourseStatus.Pending;
                        break;
                    default:
                        return BadRequest("Invalid status: " + statusName);
                }

                repo.Update(courseId, course);
                repo.SaveChanges();

                return Ok("Status updated successfully: " + statusName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /*/// <summary>
        /// Search for courses by name or return all courses if search query is empty.
        /// </summary>
        /// <param name="searchQuery">The search query for course names.</param>
        /// <returns>A list of courses matching the search query or all courses if no query is provided.</returns>
        [HttpGet("search")]
        public IActionResult SearchCourses(string? searchQuery, int currentPage, int pageSize)
        {
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    // If there's no search query, return all courses
                    var courses = repo.GetList();
                    var countNull = courses.Count();
                    int totalPagesNull = (int)Math.Ceiling(countNull / (double)pageSize);
                    var coursesPagedNull = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPageNull = currentPage > 1 ? true : false;
                    var nextPageNull = currentPage < totalPagesNull ? true : false;

                    // Object which we are going to send in header
                    var paginationMetadataNull = new
                    {
                        countNull,
                        pageSize,
                        currentPage,
                        totalPagesNull,
                        previousPageNull,
                        nextPageNull
                    };

                    return Ok(new
                    {
                        PaginationData = paginationMetadataNull,
                        ListCourse = coursesPagedNull
                    });
                }

                // If there is a search query, filter courses by name
                var filteredCourses = repo.GetList()
                    .Where(course => course.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (filteredCourses.Count == 0)
                {
                    return NotFound("No courses found matching the search query.");
                }

                var count = filteredCourses.Count();
                int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var coursesPaged = filteredCourses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1 ? true : false;
                var nextPage = currentPage < totalPages ? true : false;

                // Object which we are going to send in header
                var paginationMetadata = new
                {
                    count,
                    pageSize,
                    currentPage,
                    totalPages,
                    previousPage,
                    nextPage
                };

                return Ok(new
                {
                    PaginationData = paginationMetadata,
                    ListCourse = coursesPaged
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }*/



        /// <summary>
        /// This method retrieves a list of courses created by a mentor.
        /// </summary>
        /// <param name="mentorId">The ID of the mentor for whom to retrieve courses.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="pageSize">The number of items to display on each page.</param>
        /// <returns>
        /// An IActionResult containing a paginated list of courses created by the mentor.
        /// </returns>
        [HttpGet("get-courses-by-mentor")]
        //[Authorize(Roles = "2")]
        public IActionResult GetCoursesCreatedByMentor(int userId, int currentPage, int pageSize)
        {
            try
            {
                var mentor = mentorRepository.GetByUserId(userId);
                if (mentor == null)
                {
                    return NotFound("User is not a mentor.");
                }

                var courses = repo.GetListCourseMentor().Where(course => course.MentorId == mentor.Id && course.Status == (int)CourseStatus.Active).ToList();

                int totalCourses = courses.Count;
                int totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);
                var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1;
                var nextPage = currentPage < totalPages;

                var response = new
                {
                    PaginationData = new
                    {
                        TotalCourses = totalCourses,
                        PageSize = pageSize,
                        CurrentPage = currentPage,
                        TotalPages = totalPages,
                        PreviousPage = previousPage,
                        NextPage = nextPage
                    },
                    Courses = coursesPaged
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// This method retrieves a list of courses created by a mentor.
        /// </summary>
        /// <param name="userId">The ID of the mentor (user) for whom to retrieve courses.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="pageSize">The number of items to display on each page.</param>
        /// <returns>
        /// An IActionResult containing a paginated list of courses created by the mentor.
        /// </returns>
        [HttpGet("get-courses-by-mentorUserId")]
        [Authorize(Roles = "2")]
        public IActionResult GetCoursesCreatedByMentorUserId(int userId, int currentPage, int pageSize)
        {
            try
            {
                var mentor = mentorRepository.GetByUserId(userId);
                if (mentor == null)
                {
                    return NotFound("User is not a mentor.");
                }

                var courses = repo.GetListCourseMentor().Where(course => course.MentorId == mentor.Id).Reverse().ToList();

                // Thực hiện phân trang
                int totalCourses = courses.Count;
                int totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);
                var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1;
                var nextPage = currentPage < totalPages;

                // Tạo dữ liệu phản hồi
                var response = new
                {
                    PaginationData = new
                    {
                        TotalCourses = totalCourses,
                        PageSize = pageSize,
                        CurrentPage = currentPage,
                        TotalPages = totalPages,
                        PreviousPage = previousPage,
                        NextPage = nextPage
                    },
                    Courses = coursesPaged
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-courses-pending")]
        [Authorize(Roles = "1")]
        public IActionResult GetCoursesInactive(int currentPage, int pageSize, CourseStatus courseStatus)
        {
            try
            {
                var courses = repo.GetListCourseByStatus(courseStatus).ToList();

                int totalCourses = courses.Count;
                int totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);
                var coursesPaged = courses.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1;
                var nextPage = currentPage < totalPages;

                var response = new
                {
                    PaginationData = new
                    {
                        TotalCourses = totalCourses,
                        PageSize = pageSize,
                        CurrentPage = currentPage,
                        TotalPages = totalPages,
                        PreviousPage = previousPage,
                        NextPage = nextPage
                    },
                    Courses = coursesPaged
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves information about a course based on userId and courseId.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="courseId">The ID of the course.</param>
        /// <returns>Information about the course or returns NotFound if not found.</returns>
        [HttpGet("user/{userId}/course/{courseId}")]
        [Authorize]
        public IActionResult GetCourseByUserIdAndCourseId(int userId, int courseId)
        {
            var jwtId = int.Parse(User.FindFirst("Id").Value);
            if (!(jwtId == userId))
            {
                return BadRequest("Only user has userId = jwtDecode.Id");
            }
            var courseDetails = repo.GetCourseByUserIdAndCourseId(userId, courseId);

            if (courseDetails == null)
            {
                return NotFound();
            }

            return Ok(courseDetails);
        }

        [HttpGet("get-course-by-mentor/mentorUserId/{mentorUserId}/course/{courseId}")]
        [Authorize]
        public IActionResult GetCourseDetailsByMentor(int mentorUserId, int courseId)
        {
            var jwtId = int.Parse(User.FindFirst("Id").Value);
            if (!(jwtId == mentorUserId))
            {
                return BadRequest("Only user has userId = jwtDecode.Id");
            }
            var courseDetails = repo.GetCourseDetailsByMentor(mentorUserId, courseId);

            if (courseDetails == null)
            {
                return NotFound();
            }

            return Ok(courseDetails);
        }

        /// <summary>
        /// Creates a new course with the specified details.
        /// </summary>
        /// <param name="userId">The ID of the user creating the course.</param>
        /// <param name="courseName">The name of the course.</param>
        /// <param name="description">The detailed description of the course.</param>
        /// <param name="shortDescription">A brief description of the course.</param>
        /// <param name="price">The price of the course.</param>
        /// <param name="lectureCount">The number of lectures in the course.</param>
        /// <param name="contentLength">The length of the course content.</param>
        /// <param name="specializationId">The specialization ID of the course.</param>
        /// <param name="courseImage">An image representing the course (if provided).</param>
        /// <returns>Returns the newly created course as a CourseDTO if successful; otherwise, returns a 500 Internal Server Error with an error message.</returns>
        [HttpPost("create-new-course")]
        [Authorize(Roles = "2")]
        public ActionResult<CourseDTO> Create(int userId, [FromForm] string courseName, [FromForm] string description,
                                    [FromForm] string shortDescription, [FromForm] int price, [FromForm] int lectureCount,
                                    [FromForm] int contentLength, [FromForm] int specializationId, IFormFile courseImage)
        {
            try
            {
                var data = repo.CreateCourse(userId, courseName, description, shortDescription, price,
                                            lectureCount, contentLength, specializationId, courseImage);
                repo.SaveChanges();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        /// <summary>
        /// Updates the details of a course identified by courseId for a specific user (userId).
        /// </summary>
        /// <param name="userId">The ID of the user performing the update.</param>
        /// <param name="courseId">The ID of the course to be updated.</param>
        /// <param name="courseName">The new name of the course.</param>
        /// <param name="description">The updated detailed description of the course.</param>
        /// <param name="shortDescription">The updated brief description of the course.</param>
        /// <param name="price">The updated price of the course.</param>
        /// <param name="lectureCount">The updated number of lectures in the course.</param>
        /// <param name="contentLength">The updated length of the course content.</param>
        /// <param name="specializationId">The updated specialization ID of the course.</param>
        /// <param name="courseImage">An image representing the updated course (if provided).</param>
        /// <returns>Returns the updated course as a CourseDTO if successful; otherwise, returns a 404 Not Found if the course is not found or a 500 Internal Server Error with an error message in case of an exception.</returns>
        [HttpPut("update/{courseId}/{userId}")]
        [Authorize(Roles = "2")]
        public IActionResult UpdateLecture(int userId,
                                           int courseId,
                                           [FromForm] string? courseName,
                                           [FromForm] string? description,
                                           [FromForm] string? shortDescription,
                                           IFormFile? courseImage,
                                           [FromForm] int price = 0,
                                           [FromForm] int lectureCount = 0,
                                           [FromForm] int contentLength = 0,
                                           [FromForm] int specializationId = 0)
        {
            try
            {
                var updatedCourse = repo.UpdateCourse(userId, courseId, courseName, description, shortDescription, price, lectureCount, contentLength, specializationId, courseImage);

                if (updatedCourse == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy khóa học.
                }

                return Ok(updatedCourse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("by-specialization/{specializationId}")]
        public ActionResult<IEnumerable<CourseDTO>> GetCoursesBySpecializationId(int specializationId)
        {
            var list = repo.GetListBySpecializationId(specializationId);
            return Ok(list);
        }

        [HttpPost("process-course-request")]
        [Authorize(Roles = "1")]
        public IActionResult ProcessCourseRequest(int courseId, bool acceptRequest, string? note)
        {
            try
            {
                var result = repo.ProcessCourseRequest(courseId, acceptRequest, note);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /*[HttpPost("ban-course")]
        [Authorize(Roles = "1")]
        public ActionResult<string> SetCourseStatus(int courseId, [FromQuery] bool status)
        {
            try
            {
                var action = repo.BanCourse(courseId, status);
                return Ok(action);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }*/

        [HttpPost("ban-course")]
        [Authorize(Roles = "1")]
        public ActionResult<string> SetCourseStatus(int courseId, [FromForm] bool status, [FromForm] string reason)
        {
            try
            {
                var action = repo.BanCourse(courseId, status, reason);
                repo.SaveChanges();
                return Ok(action);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
