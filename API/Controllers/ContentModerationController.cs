using DAL.DTO;
using DAL.Repository;
using DAL.Service;
using Google.Cloud.VideoIntelligence.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing content moderation.
    /// </summary>
    [Route("api/content-moderation")]
    [ApiController]
    //[Authorize]
    public class ContentModerationController : ControllerBase
    {
        private readonly IContentModerationRepository repo;
        private readonly IVideoIntelligenceService service;

        public ContentModerationController(IContentModerationRepository repo, IVideoIntelligenceService service)
        {
            this.repo = repo;
            this.service = service;
        }

        /// <summary>
        /// Create a new content moderation entry.
        /// </summary>
        /// <param name="_object">The content moderation entry to create.</param>
        /// <returns>The created content moderation entry.</returns>
        [HttpPost]
        public ActionResult<ContentModerationDTO> Create(ContentModerationDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/content-moderation/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all content moderation entries.
        /// </summary>
        /// <returns>A list of content moderation entries.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<ContentModerationDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a content moderation entry by ID.
        /// </summary>
        /// <param name="id">The ID of the content moderation entry.</param>
        /// <returns>The content moderation entry with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<ContentModerationDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update a content moderation entry by ID.
        /// </summary>
        /// <param name="id">The ID of the content moderation entry to update.</param>
        /// <param name="_object">The updated content moderation entry data.</param>
        /// <returns>The updated content moderation entry.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, ContentModerationDTO _object)
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
        /// Delete a content moderation entry by ID.
        /// </summary>
        /// <param name="id">The ID of the content moderation entry to delete.</param>
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

        [HttpGet("moderation")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> Moderation(int lectureId)
        {
            try
            {
                var returnData = await service.VideoModeration(lectureId);

                return Ok(returnData);
            } catch (Exception ex)
            {
                return StatusCode(500, "Error at Moderation() in ContentModerationController: " + ex.Message);
            }
        }

        [HttpGet("get-moderation")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<object> GetModeration(int lectureId)
        {
            try
            {
                var returnData = repo.GetModeration(lectureId);
                if (returnData is string)
                {
                    return BadRequest(returnData);
                }
                return Ok(returnData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error at GetModeration() in ContentModerationController: " + ex.Message);
            }
        }
    }
}
