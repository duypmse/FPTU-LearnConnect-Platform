using BAL.Models;
using DAL.DTO;
using DAL.Repository;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/comment")]
    [ApiController]
    [Authorize]

    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository repo;

        public CommentController(ICommentRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Create a new promotion.
        /// </summary>
        /// <param name="_object">The promotion to create.</param>
        /// <returns>The created promotion.</returns>
        [HttpPost]
        [Authorize(Roles ="2,3")]
        public ActionResult<CommentDTO> Create(int userId, int lectureId, int? parentCommentId, [FromForm] string comment)
        {
            var _object = new CommentDTO
            {
                UserId = userId,
                LectureId = lectureId,
                ParentCommentId = parentCommentId,
                Comment1 = comment,
                CommentTime = DateTime.UtcNow.AddHours(7),
                Status = parentCommentId == null ? 0 : 1,
            };
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/comment/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all promotions.
        /// </summary>
        /// <returns>A list of promotions.</returns>
        [HttpGet]
        [Authorize(Roles = "2,3")]
        public ActionResult<IEnumerable<CommentDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a promotion by ID.
        /// </summary>
        /// <param name="id">The ID of the promotion.</param>
        /// <returns>The promotion with the specified ID.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "2,3")]
        public ActionResult<CommentDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update a promotion by ID.
        /// </summary>
        /// <param name="id">The ID of the promotion to update.</param>
        /// <param name="_object">The updated promotion data.</param>
        /// <returns>The updated promotion.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "2,3")]
        public ActionResult Update(int id, CommentDTO _object)
        {
            if (_object.Id != id)
            {
                return BadRequest();
            }
            var tmpObject = repo.Update(id, _object);
            if (tmpObject == 0)
            {
                return NotFound();
            }
            repo.SaveChanges();

            return Ok(_object);
        }

        /// <summary>
        /// Delete a promotion by ID.
        /// </summary>
        /// <param name="id">The ID of the promotion to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "2,3")]
        public ActionResult Delete(int id)
        {
            var tmpObject = repo.Delete(id);
            if (tmpObject == 0)
            {
                return NotFound();
            }
            repo.SaveChanges();

            return NoContent();
        }

        [HttpGet("get-comments-by-lectureId/{lectureId}")]
        [Authorize(Roles = "2,3")]
        public IActionResult GetCommentsByLectureId(int lectureId)
        {
            try
            {
                var comments = repo.GetByLectureId(lectureId);

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
