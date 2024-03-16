using BAL.Models;
using DAL;
using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing favorite courses.
    /// </summary>
    [Route("api/favorite-course")]
    [ApiController]
    [Authorize]
    public class FavoriteCourseController : ControllerBase
    {
        private readonly IFavoriteCourseRepository repo;
        private readonly ICourseRepository courseRepository;

        public FavoriteCourseController(IFavoriteCourseRepository repo, ICourseRepository courseRepository)
        {
            this.repo = repo;
            this.courseRepository = courseRepository;
        }

        /// <summary>
        /// Create a new favorite course.
        /// </summary>
        /// <param name="_object">The favorite course to create.</param>
        /// <returns>The created favorite course.</returns>
        [HttpPost]
        public ActionResult<FavoriteCourseDTO> Create(FavoriteCourseDTO _object)
        {
            var existingFavorite = repo.GetList().FirstOrDefault(fc => fc.UserId == _object.UserId && fc.FavoriteCourseId == _object.FavoriteCourseId);

            if (existingFavorite != null)
            {
                return BadRequest("This course is already in the user's favorites.");
            }
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/favorite-course/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all favorite courses.
        /// </summary>
        /// <returns>A list of favorite courses.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<FavoriteCourseDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a favorite course by ID.
        /// </summary>
        /// <param name="id">The ID of the favorite course.</param>
        /// <returns>The favorite course with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<FavoriteCourseDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update a favorite course by ID.
        /// </summary>
        /// <param name="id">The ID of the favorite course to update.</param>
        /// <param name="_object">The updated favorite course data.</param>
        /// <returns>The updated favorite course.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, FavoriteCourseDTO _object)
        {
            if (_object.Id != id)
            {
                return BadRequest();
            }
            if (repo.Update(id, _object) == 0)
            {
                return NotFound();
            }

            repo.SaveChanges();

            return Ok(_object);
        }

        /// <summary>
        /// Delete a favorite course by ID.
        /// </summary>
        /// <param name="id">The ID of the favorite course to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "2,3")]
        public ActionResult Delete(int id)
        {
            var tmpObject = repo.Get(id);
            if (tmpObject == null)
            {
                return NotFound();
            }
            repo.Delete(id);
            repo.SaveChanges();

            return NoContent();
        }

        [HttpDelete("un-set-favorite")]
        [Authorize(Roles = "2,3")]
        public ActionResult UnSetFavoriteCourse(int userId, int courseId)
        {
            try
            {
                repo.UnSetFavoriteCourse(userId, courseId);
                repo.SaveChanges();
                return NoContent();
            }
            catch (Exception er)
            {
                return BadRequest("Error at UnSetFavoriteCourse in FavoriteCourseController: " + er);
            }
            

            
        }

        /// <summary>
        /// Get favorite courses by user ID.
        /// </summary>
        /// <param name="userId">The UserID of the user to get favorite courses for.</param>
        /// <returns>A list of favorite courses for the specified user with CourseName.</returns>
        [HttpGet("get-favorite-courses-by-user")]
        [Authorize(Roles = "2,3")]
        public ActionResult GetFavoriteCoursesByUser(int userId, int currentPage, int pageSize)
        {
            try
            {
                var favoriteCoursesWithCourseInfo = repo.GetFavoriteCoursesByUser(userId);

                if (currentPage != 0 && pageSize != 0)
                {
                    var count = favoriteCoursesWithCourseInfo.Count();
                    int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                    var favoriteCoursesPaged = favoriteCoursesWithCourseInfo.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPage = currentPage > 1 ? true : false;
                    var nextPage = currentPage < totalPages ? true : false;

                    // Object to be sent in the header
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
                        ListFavoriteCourses = favoriteCoursesPaged
                    });
                } else
                {
                    return Ok(favoriteCoursesWithCourseInfo);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




    }
}
