using DAL;
using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace API.Controllers
{
    [Route("api/specialization")]
    [ApiController]
    //[Authorize]
    public class SpecializationController : ControllerBase
    {
        private readonly ISpecializationRepository repo;

        public SpecializationController(ISpecializationRepository repo)
        {
            this.repo = repo;
        }

        
        [HttpPost]
        [Authorize(Roles = "1")]
        public IActionResult Create([FromForm] int majorId, [FromForm] string name, [FromForm] string description)
        {
            try
            {
                var created = repo.Create(majorId, name, description);
                repo.SaveChanges();
                return Ok(created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all specializations.
        /// </summary>
        /// <returns>A list of specializations.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<SpecializationDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a specialization by ID.
        /// </summary>
        /// <param name="id">The ID of the specialization.</param>
        /// <returns>The specialization with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "1")]
        public ActionResult<SpecializationDTO> Get(int id)
        {
            var _object = repo.GetById(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1")]
        public IActionResult Update(int id, [FromForm] string name, [FromForm] string description, [FromForm] int majordId)
        {
            try
            {
                var updated = repo.Update(id, name, description, majordId);
                repo.SaveChanges();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a specialization by ID.
        /// </summary>
        /// <param name="id">The ID of the specialization to delete.</param>
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
        /// Update the status of a specialization by ID.
        /// </summary>
        /// <param name="specializationId">The ID of the specialization to update.</param>
        /// <param name="status">The new status for the course. Choose 0: "Inactive" or 1: "Active".</param>
        /// <returns>A response indicating the status update result.</returns>
        [HttpPut("update-specialization-status")]
        [Authorize(Roles = "1")]
        public IActionResult UpdateSpecializationStatus(int specializationId, SpecializationStatus status)
        {
            try
            {
                var specialization = repo.Get(specializationId);

                if (specialization == null)
                {
                    return NotFound("Specialization not found");
                }

                string statusName = Enum.GetName(typeof(SpecializationStatus), status);

                switch (status)
                {
                    case SpecializationStatus.Active:
                        specialization.IsActive = true;
                        break;
                    case SpecializationStatus.Inactive:
                        specialization.IsActive = false;
                        break;
                    default:
                        return BadRequest("Invalid status: " + statusName);
                }

                repo.Update(specializationId, specialization);
                repo.SaveChanges();

                return Ok("Status updated successfully: " + statusName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("by-major/{majorId}")]
        public ActionResult<IEnumerable<SpecializationDTO>> GetByMajorId(int majorId)
        {
            var list = repo.GetListByMajorId(majorId);
            return Ok(list);
        }

    }
}
