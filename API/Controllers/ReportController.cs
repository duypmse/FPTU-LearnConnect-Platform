using BAL.Models;
using DAL.DTO;
using DAL.Repository;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing operations related to reports.
    /// </summary>
    [Route("api/report")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository repo;
        private readonly ICourseRepository courseRepository;

        public ReportController(IReportRepository repo, ICourseRepository courseRepository)
        {
            this.repo = repo;
            this.courseRepository = courseRepository;
        }

        /// <summary>
        /// Create a new report.
        /// </summary>
        /// <param name="_object">The report information to be submitted.</param>
        /// <returns>The created report information.</returns>
        [HttpPost]
        [Authorize(Roles = "2,3")]
        public ActionResult<ReportDTO> Create(ReportDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/report/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all reports.
        /// </summary>
        /// <returns>List of reports.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<ReportDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get the information of a report based on its ID.
        /// </summary>
        /// <param name="id">ID of the report to retrieve information for.</param>
        /// <returns>Information of the report with the corresponding ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<ReportDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update the information of a report.
        /// </summary>
        /// <param name="id">ID of the report to update.</param>
        /// <param name="_object">New report information.</param>
        /// <returns>Updated report information, or NotFound if not found.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, ReportDTO _object)
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
        /// Delete a report based on its ID.
        /// </summary>
        /// <param name="id">ID of the report to delete.</param>
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

        /*[HttpPost("report-course")]
        [Authorize(Roles = "3")]
        public async Task<ActionResult<ReportDTO>> Report(int userId, int courseId, [FromForm] string reportReason, [FromForm] string reportComment, IFormFile reportImage)
        {
            try
            {
                var message = new Message
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = "Reported success",
                        Body = $"You have reported the course '{courseRepository.Get(courseId).Name}'"
                    }
                };
                var deviceTokens = GetDeviceTokens(userId);
                foreach (var deviceToken in deviceTokens)
                {
                    message.Token = deviceToken;

                    var messaging = FirebaseMessaging.DefaultInstance;
                    await messaging.SendAsync(message);
                }

                var data = repo.Report(userId, courseId, reportReason, reportComment, reportImage);
                repo.SaveChanges();

                return Ok(new
                {
                    Message = message,
                    Data = data
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }*/

        [HttpPost("report-course")]
        [Authorize(Roles = "2,3")]
        public ActionResult<ReportDTO> ReportCourse(int userId, int courseId, [FromForm] string reportReason, [FromForm] string? reportComment, IFormFile? reportImage)
        {
            try
            {
                var data = repo.ReportCourse(userId, courseId, reportReason, reportComment, reportImage);
                repo.SaveChanges();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("report-mentor")]
        [Authorize(Roles = "2,3")]
        public ActionResult<ReportDTO> ReportMentor(int userId, int mentorId, [FromForm] string reportReason, [FromForm] string? reportComment, IFormFile? reportImage)
        {
            try
            {
                var data = repo.ReportMentor(userId, mentorId, reportReason, reportComment, reportImage);
                repo.SaveChanges();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("all-list-reports")]
        [Authorize(Roles = "1")]
        public ActionResult<IEnumerable<object>> GetAllListRatings(string reportType)
        {
            try
            {
                var list = repo.GetAllReports(reportType);

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-reports")]
        [Authorize(Roles = "1")]
        public ActionResult<IEnumerable<object>> GetReportsByCourseIdOrMentorId(int targetId, string reportType)
        {
            try
            {
                var list = repo.GetReportsByCourseIdOrMentorId(targetId, reportType);

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private List<string> GetDeviceTokens(int userId)
        {
            
            List<string> deviceTokens = new List<string>();

            //code

            return deviceTokens;
        }

    }
}
