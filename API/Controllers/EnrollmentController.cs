using DAL.DTO;
using DAL.Repository;
using DAL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing enrollments.
    /// </summary>
    [Route("api/enrollment")]
    [ApiController]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentRepository repo;
        private readonly IVNPayService service;

        public EnrollmentController(IEnrollmentRepository repo, IVNPayService service)
        {
            this.repo = repo;
            this.service = service;
        }



        /// <summary>
        /// Create a new enrollment.
        /// </summary>
        /// <param name="_object">The enrollment to create.</param>
        /// <returns>The created enrollment.</returns>
        [HttpPost]
        public ActionResult<EnrollmentDTO> Create(EnrollmentDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/enrollment/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all enrollments.
        /// </summary>
        /// <returns>A list of enrollments.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<EnrollmentDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get an enrollment by ID.
        /// </summary>
        /// <param name="id">The ID of the enrollment.</param>
        /// <returns>The enrollment with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<EnrollmentDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update an enrollment by ID.
        /// </summary>
        /// <param name="id">The ID of the enrollment to update.</param>
        /// <param name="_object">The updated enrollment data.</param>
        /// <returns>The updated enrollment.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, EnrollmentDTO _object)
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
        /// Delete an enrollment by ID.
        /// </summary>
        /// <param name="id">The ID of the enrollment to delete.</param>
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


        /// <summary>
        /// Handle user enroll course and forward VNPay payment page
        /// </summary>
        /// <returns>VNpay Url</returns>
        [HttpPost("Enroll")]
        [Authorize(Roles = "3,2")]
        public ActionResult EnrollCourse(int userId, int courseId, string returnUrl)
        {
            try
            {
                var vnPayUrl = service.EnrollCourse(userId,courseId,returnUrl);

                return Ok(vnPayUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("learn-course-result")]
        [Authorize(Roles ="3,2")]
        public ActionResult GetLearnCourseResult(int courseId, int userId)
        {
            try
            {
                var returnData = repo.GetLearnCourseResult(courseId,userId);

                return Ok(returnData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }
    }
}
