using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing specialization of mentor.
    /// </summary>
    [Route("api/specialization-of-mentor")]
    [ApiController]
    //[Authorize]
    public class SpecializationOfMentorController : ControllerBase
    {
        private readonly ISpecializationOfMentorRepository repo;

        public SpecializationOfMentorController(ISpecializationOfMentorRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Create a new specialization of mentor.
        /// </summary>
        /// <param name="_object">The specialization of mentor to create.</param>
        /// <returns>The created specialization of mentor.</returns>
        [HttpPost]
        [Authorize]
        public ActionResult<SpecializationOfMentorDTO> Create(SpecializationOfMentorDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/specialization-of-mentor/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all specialization of mentor.
        /// </summary>
        /// <returns>A list of specialization of mentor.</returns>
        [HttpGet]
        [Authorize]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<SpecializationOfMentorDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a specialization of mentor by ID.
        /// </summary>
        /// <param name="id">The ID of the specialization of mentor.</param>
        /// <returns>The specialization of mentor with the specified ID.</returns>
        [HttpGet("{id}")]
        [Authorize]
        //[Authorize(Roles = "Admin")]
        public ActionResult<SpecializationOfMentorDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update a specialization of mentor by ID.
        /// </summary>
        /// <param name="id">The ID of the specialization of mentor to update.</param>
        /// <param name="_object">The updated specialization of mentor data.</param>
        /// <returns>The updated specialization of mentor.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult Update(int id, SpecializationOfMentorDTO _object)
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
        /// Delete a specialization of mentor by ID.
        /// </summary>
        /// <param name="id">The ID of the specialization of mentor to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [Authorize]
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

        [HttpGet("by-mentor/{mentorUserId}")]
        [Authorize(Roles = "2")]
        public ActionResult<IEnumerable<object>> GetByMentorId(int mentorUserId)
        {
            try
            {
                var list = repo.GetListByMentorId(mentorUserId);

                if (list.Any())
                {
                    return Ok(list);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpGet("get-specializations-not-request-yet/{mentorUserId}/{majorId}")]
        [Authorize(Roles = "2")]
        public IActionResult GetListWithApprovalCheck(int mentorUserId, int majorId)
        {
            try
            {
                var unapprovedSpecializations = repo.GetSpecializationsNotRequestYet(mentorUserId, majorId);
                return Ok(unapprovedSpecializations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error" + ex.Message);
            }
        }

        [HttpGet("get-specialization-and-mentor/{specializationOfMentorId}")]
        public IActionResult GetSpecializationsAndMentors(int specializationOfMentorId)
        {
            try
            {
                var result = repo.GetSpecializationAndMentorRequest(specializationOfMentorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error" + ex.Message);
            }
        }

        [HttpGet("get-all-specializations-and-mentors")]
        [Authorize(Roles = "1")]
        public IActionResult GetAllSpecializationsAndMentors(string requestType)
        {
            try
            {
                var result = repo.GetAllSpecializationAndMentorRequest(requestType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error" + ex.Message);
            }
        }

    }
}
