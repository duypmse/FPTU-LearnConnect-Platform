using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/schedule")]
    [ApiController]
    //[Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository repo;

        public ScheduleController(IScheduleRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Create a new promotion.
        /// </summary>
        /// <param name="_object">The promotion to create.</param>
        /// <returns>The created promotion.</returns>
        [HttpPost]
        //[Authorize(Roles = "2,3")]
        public ActionResult<ScheduleDTO> Create(ScheduleDTO _object)
        {
            try
            {
                _object.Id = 0;
                _object.Status = 0;
                var returnObject = repo.Add(_object);
                return Created($"api/schedule/{returnObject.Id}", returnObject);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message});
            }            
        }

        /// <summary>
        /// Get a list of all promotions.
        /// </summary>
        /// <returns>A list of promotions.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<ScheduleDTO>> GetAll()
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
        //[Authorize(Roles = "Admin")]
        public ActionResult<ScheduleDTO> Get(int id)
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
        //[Authorize]
        public ActionResult Update(int id, ScheduleDTO _object)
        {
            if (_object.Id != id)
            {
                return BadRequest();
            }
            var tmpObject = repo.Get(id);
            if (tmpObject == null)
            {
                return NotFound();
            }

            repo.Update(id, _object);
            repo.SaveChanges();

            return Ok(_object);
        }

        /// <summary>
        /// Delete a promotion by ID.
        /// </summary>
        /// <param name="id">The ID of the promotion to delete.</param>
        /// <returns>No content.</returns>
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

        [HttpGet("get-by-userid")]
        //[Authorize(Roles = "2,3")]
        public ActionResult<IEnumerable<ScheduleDTO>> GetAllByUser(int userId)
        {
            try
            {
                var list = repo.GetAllByUser(userId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
            
        }
    }
}
