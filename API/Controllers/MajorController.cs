using DAL;
using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Xml.Linq;

namespace API.Controllers
{
    [Route("api/major")]
    [ApiController]
    [Authorize]
    public class MajorController : ControllerBase
    {
        private readonly IMajorRepository repo;

        public MajorController(IMajorRepository repo)
        {
            this.repo = repo;
        }

        
        [HttpPost]
        [Authorize(Roles = "1")]
        public IActionResult Create([FromForm] string name, [FromForm] string description)
        {
            try
            {
                var created = repo.Create(name, description);
                repo.SaveChanges();
                return Ok(created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all Majors.
        /// </summary>
        /// <returns>A list of Majors.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<MajorDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a Major by ID.
        /// </summary>
        /// <param name="id">The ID of the Major.</param>
        /// <returns>The Major with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<MajorDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "1")]
        public IActionResult Update(int id, [FromForm] string name, [FromForm] string description)
        {
            try
            {
                var updated = repo.Update(id, name, description);
                repo.SaveChanges();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a Major by ID.
        /// </summary>
        /// <param name="id">The ID of the Major to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
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

        /// <summary>
        /// Update the status of a Major by ID.
        /// </summary>
        /// <param name="MajorId">The ID of the Major to update.</param>
        /// <param name="status">The new status for the course. Choose 0: "Inactive" or 1: "Active".</param>
        /// <returns>A response indicating the status update result.</returns>
        [HttpPut("update-major-status")]
        [Authorize(Roles = "1")]
        public IActionResult UpdateMajorStatus(int MajorId, MajorStatus status)
        {
            try
            {
                var Major = repo.Get(MajorId);

                if (Major == null)
                {
                    return NotFound("Major not found");
                }

                string statusName = Enum.GetName(typeof(MajorStatus), status);

                switch (status)
                {
                    case MajorStatus.Active:
                        Major.IsActive = true;
                        break;
                    case MajorStatus.Inactive:
                        Major.IsActive = false;
                        break;
                    default:
                        return BadRequest("Invalid status: " + statusName);
                }

                repo.Update(MajorId, Major);
                repo.SaveChanges();

                return Ok("Status updated successfully: " + statusName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-majors-not-request-yet/{mentorUserId}")]
        [Authorize(Roles = "2")]
        public ActionResult<IEnumerable<object>> GetMajorsNotRequestYet(int mentorUserId)
        {
            try
            {
                var majorsTaughtCompletely = repo.GetMajorsNotRequestYet(mentorUserId);
                return Ok(majorsTaughtCompletely);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
