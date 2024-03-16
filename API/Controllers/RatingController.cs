using DAL;
using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing ratings.
    /// </summary>
    [Route("api/rating")]
    [ApiController]
    //[Authorize]
    public class RatingController : ControllerBase
    {
        private readonly IRatingRepository repo;

        public RatingController(IRatingRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Create a new rating.
        /// </summary>
        /// <param name="_object">The rating information to be submitted.</param>
        /// <returns>The created rating information.</returns>
        [HttpPost]
        [Authorize(Roles = "2,3")]
        public ActionResult<RatingDTO> Create(RatingDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/rating/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all ratings.
        /// </summary>
        /// <returns>List of ratings.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<RatingDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get the information of a rating based on its ID.
        /// </summary>
        /// <param name="id">ID of the rating to retrieve information for.</param>
        /// <returns>Information of the rating with the corresponding ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<RatingDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update the information of a rating.
        /// </summary>
        /// <param name="id">ID of the rating to update.</param>
        /// <param name="_object">New rating information.</param>
        /// <returns>Updated rating information, or NotFound if not found.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, RatingDTO _object)
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
        /// Delete a rating based on its ID.
        /// </summary>
        /// <param name="id">ID of the rating to delete.</param>
        /// <returns>NoContent if deletion is successful, or NotFound if not found.</returns>
        [HttpDelete("{id}")]
        //[Authorize]
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

        [HttpPost("rating-course")]
        [Authorize(Roles = "2,3")]
        public ActionResult<RatingDTO> RatingCourse(int userId, int courseId, [FromForm] decimal rating, [FromForm] string? comment)
        {
            try
            {
                var data = repo.RatingCourse(userId, courseId, rating, comment);
                repo.SaveChanges();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("rating-mentor")]
        [Authorize(Roles = "2,3")]
        public ActionResult<RatingDTO> RatingMentor(int userId, int mentorId, [FromForm] decimal rating, [FromForm] string? comment)
        {
            try
            {
                var data = repo.RatingMentor(userId, mentorId, rating, comment);
                repo.SaveChanges();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("listRatingOfCourse/{courseId}")]
        public ActionResult<IEnumerable<object>> GetListRatingByCourseId(int courseId)
        {
            try
            {
                var list = repo.GetListRatingOfCourse(courseId);

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("listRatingOfMentor/{userMentorId}")]
        public ActionResult<IEnumerable<object>> GetListRatingByMentorUserId(int userMentorId)
        {
            try
            {
                var list = repo.GetListRatingOfMentor(userMentorId);

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("allListRatings")]
        [Authorize(Roles = "1")]
        public ActionResult<IEnumerable<object>> GetAllListRatings(string ratingType)
        {
            try
            {
                var list = repo.GetAllRatings(ratingType);

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update-rating-status")]
        [Authorize(Roles = "1,2,3")]
        public ActionResult UpdateRatingStatus(int id, RatingStatus status)
        {
            try
            {
                var updatedRating = repo.UpdateRatingStatus(id, status);
                if (updatedRating == null)
                {
                    return NotFound();
                }

                return Ok(updatedRating);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
