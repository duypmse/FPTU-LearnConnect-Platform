using DAL;
using DAL.DTO;
using DAL.Repository;
using DAL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing lectures.
    /// </summary>
    [Route("api/lecture")]
    [ApiController]
    //[Authorize]
    public class LectureController : ControllerBase
    {
        private readonly ILectureRepository repo;
        private readonly IVideoIntelligenceService videoService;

        public LectureController(ILectureRepository repo, IVideoIntelligenceService service)
        {
            this.repo = repo;
            this.videoService = service;
        }

        /// <summary>
        /// Get a list of all lecture records.
        /// </summary>
        /// <returns>A list of lecture records.</returns>
        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<LectureDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a lecture record by ID.
        /// </summary>
        /// <param name="id">The ID of the lecture record.</param>
        /// <returns>The lecture record with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<LectureDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Get a list of lecture records by course ID.
        /// </summary>
        /// <param name="courseId">The ID of the course for which you want to get lectures.</param>
        /// <returns>A list of lecture records for the specified course.</returns>
        [HttpGet("by-course/{courseId}")]
        public ActionResult<IEnumerable<LectureDTO>> GetLecturesByCourse(int courseId)
        {
            var lectures = repo.GetLecturesByCourseId(courseId);

            if (lectures == null || !lectures.Any())
            {
                return NotFound($"No lectures found with course ID: {courseId}");
            }

            return Ok(lectures);
        }

        [HttpGet("by-user-course")]
        public ActionResult<IEnumerable<LectureDTO>> GetLecturesByCourseId(int? userId, int courseId)
        {
            var lectures = repo.GetLecturesByCourseId(userId, courseId);

            if (lectures == null || !lectures.Any())
            {
                return NotFound($"No lectures found with course ID: {courseId}");
            }

            return Ok(lectures);
        }

        /// <summary>
        /// Get details of a lecture by course ID and lecture ID.
        /// </summary>
        /// <param name="courseId">The ID of the course.</param>
        /// <param name="lectureId">The ID of the lecture.</param>
        /// <returns>The details of the specified lecture for the specified course.</returns>
        [HttpGet("by-course/{courseId}/lecture/{lectureId}")]
        //[Authorize(Roles = "2,3")]
        public ActionResult<LectureDTO> GetLectureDetail(int courseId, int lectureId)
        {
            var lecture = repo.GetLectureDetailByCourseId(courseId, lectureId);

            if (lecture == null)
            {
                return NotFound($"No lecture found with course ID: {courseId} and lecture ID: {lectureId}");
            }

            return Ok(lecture);
        }

        

        /// <summary>
        /// Delete a lecture record by ID.
        /// </summary>
        /// <param name="id">The ID of the lecture record to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        //[Authorize(Roles="2")]
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
        /// Update the status of a lecture by ID.
        /// </summary>
        /// <param name="lectureId">The ID of the lecture to update.</param>
        /// <param name="status">The new status for the lecture. Choose 0: "Active" or 1: "Inactive".</param>
        /// <returns>A response indicating the status update result.</returns>
        [HttpPut("update-lecture-status")]
        [Authorize(Roles = "1,2")]
        public IActionResult UpdateLectureStatus(int lectureId, LectureStatus status)
        {
            try
            {
                var lecture = repo.Get(lectureId);

                if (lecture == null)
                {
                    return NotFound("Lecture not found");
                }

                string statusName = Enum.GetName(typeof(LectureStatus), status);

                switch (status)
                {
                    case LectureStatus.Active:
                        lecture.Status = (int)LectureStatus.Active;
                        break;
                    case LectureStatus.Pending:
                        lecture.Status = (int)LectureStatus.Pending;
                        break;
                    case LectureStatus.Reject:
                        lecture.Status = (int)LectureStatus.Reject;
                        break;
                    case LectureStatus.Banned:
                        lecture.Status = (int)LectureStatus.Banned;
                        break;
                    default:
                        return BadRequest("Invalid status: " + statusName);
                }

                repo.Update(lectureId, lecture);
                repo.SaveChanges();

                return Ok("Status updated successfully: " + statusName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("create-new-lecture")]
        //[Authorize(Roles = "2")]
        public async Task<ActionResult<LectureDTO>> Create(int userId, int courseId, [FromForm] string title, [FromForm] string content, IFormFile? contentFile, [FromForm] int contentType, [FromForm] string? contentUrl)
        {
            try
            {
                var data = repo.CreateLecture(userId, courseId, title, content, contentFile, contentType, contentUrl);
                if(data != null)
                {
                   await videoService.VideoModeration(data.Id);
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update/{courseId}/{userId}")]
        [Authorize(Roles = "2")]
        public IActionResult UpdateLecture(int userId,
                                           int courseId,
                                           int lectureId,
                                           [FromForm] string? title,
                                           [FromForm] string? content,
                                           IFormFile? contentFile,
                                           [FromForm] string? contentUrl,
                                           [FromForm] int contentType = -1)
        {
            try
            {
                var updatedLecture = repo.UpdateLecture(userId, courseId, lectureId, title, content, contentFile, contentUrl, contentType);

                if (updatedLecture == null)
                {
                    return NotFound();
                }

                return Ok(updatedLecture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("process-lecture-request")]
        [Authorize(Roles = "1")]
        public IActionResult ProcessLectureRequest(int lectureId, bool acceptRequest, string? note)
        {
            try
            {
                var result = repo.ProcessLectureRequest(lectureId, acceptRequest, note);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
