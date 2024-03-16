using BAL.Models;
using DAL.DTO;
using DAL.Repository;
using DAL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing verification documents.
    /// </summary>
    [Route("api/verification-document")]
    [ApiController]
    //[Authorize]
    public class VerificationDocumentController : ControllerBase
    {
        private readonly IVerificationDocumentRepository repo;
        private readonly IImageVisionService service;

        public VerificationDocumentController(IVerificationDocumentRepository repo, IImageVisionService service)
        {
            this.repo = repo;
            this.service = service;
        }

        /// <summary>
        /// Create a new verification document.
        /// </summary>
        /// <param name="_object">The verification document information to be submitted.</param>
        /// <returns>The created verification document information.</returns>
        [HttpPost]
        public ActionResult<VerificationDocumentDTO> Create(VerificationDocumentDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/verification-document/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all verification documents.
        /// </summary>
        /// <returns>List of verification documents.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<VerificationDocumentDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get the information of a verification document based on its ID.
        /// </summary>
        /// <param name="id">ID of the verification document to retrieve information for.</param>
        /// <returns>Information of the verification document with the corresponding ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<VerificationDocumentDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update the information of a verification document.
        /// </summary>
        /// <param name="id">ID of the verification document to update.</param>
        /// <param name="_object">New verification document information.</param>
        /// <returns>Updated verification document information, or NotFound if not found.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, VerificationDocumentDTO _object)
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
        /// Delete a verification document based on its ID.
        /// </summary>
        /// <param name="id">ID of the verification document to delete.</param>
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

        [HttpPost("scan-image")]
        public async Task<ActionResult> ScanImage([FromBody]string imagePath)
        {
            try
            {
                var returnData = await service.ScanBack(imagePath);
                return Ok(returnData);
            } catch (Exception ex)
            {
                return StatusCode(500, "Error at ScanImage() in VerificationDocumentController: " + ex.Message);
            }
        }
    }
}
