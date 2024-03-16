using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing flag details.
    /// </summary>
    [Route("api/flag-detail")]
    [ApiController]
    [Authorize]
    public class FlagDetailController : ControllerBase
    {
        private readonly IFlagDetailRepository repo;

        public FlagDetailController(IFlagDetailRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Create a new flag detail.
        /// </summary>
        /// <param name="_object">The flag detail to create.</param>
        /// <returns>The created flag detail.</returns>
        [HttpPost]
        public ActionResult<FlagDetailDTO> Create(FlagDetailDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/flag-detail/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all flag details.
        /// </summary>
        /// <returns>A list of flag details.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<FlagDetailDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a flag detail by ID.
        /// </summary>
        /// <param name="id">The ID of the flag detail.</param>
        /// <returns>The flag detail with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<FlagDetailDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update a flag detail by ID.
        /// </summary>
        /// <param name="id">The ID of the flag detail to update.</param>
        /// <param name="_object">The updated flag detail data.</param>
        /// <returns>The updated flag detail.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, FlagDetailDTO _object)
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
        /// Delete a flag detail by ID.
        /// </summary>
        /// <param name="id">The ID of the flag detail to delete.</param>
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
    }
}
