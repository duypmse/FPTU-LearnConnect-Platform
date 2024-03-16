using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing learning processes.
    /// </summary>
    [Route("api/learning-process")]
    [ApiController]
    //[Authorize]
    public class LearningProcessController : ControllerBase
    {
        private readonly IEnrollmentRepository repo;

        public LearningProcessController(IEnrollmentRepository repo)
        {
            this.repo = repo;
        }

        [HttpPost("save_process")]
        [Authorize(Roles ="2,3")]
        public ActionResult SaveProcess(int userId, int lectureId, int courseId, int currentTime, int maxTime, int totalTime, int? timeSpent)
        {
            try
            {
                repo.SaveProcess(userId, lectureId, courseId, currentTime, maxTime, totalTime, timeSpent);
            } catch(Exception ex)
            {
                return BadRequest("Error at SaveProcess() in LearningProcessController: " + ex.Message);
            }

            return NoContent();
        }

        [HttpGet("get_lecture_process")]
        [Authorize(Roles = "2,3")]
        public ActionResult<LearningProcessDetailDTO> GetProcessByLectureId(int lectureId, int courseId, int userId)
        {
            try
            {
                var returnData = repo.GetProcessByLectureId(lectureId, courseId, userId);
                return Ok(returnData);
            }
            catch (Exception ex)
            {
                return BadRequest("Error at GetProcessByLectureId() in LearningProcessController: " + ex.Message);
            }
        }

        [HttpGet("get_user_current_lecture")]
        //[Authorize(Roles = "2,3")]
        public ActionResult<object> GetCurrentLecture(int courseId, int userId)
        {
            try
            {
                var returnData = repo.GetCurrentLecture(courseId, userId);
                return Ok(returnData);
            }
            catch (Exception ex)
            {
                return BadRequest("Error at GetCurrentLecture() in LearningProcessController: " + ex.Message);
            }
        }

        [HttpGet("statistics-learning-process")]
        [Authorize(Roles = "2,3")]
        public ActionResult<object> GetStatistic(int userId)
        {
            try
            {
                var returnData = repo.StatisticsLearningProcess(userId);
                return Ok(returnData);
            }
            catch (Exception ex)
            {
                return BadRequest("Error at GetCurrentLecture() in LearningProcessController: " + ex.Message);
            }
        }
    }
}
