using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/test-resutl")]
    [ApiController]
    [Authorize]
    public class TestResultController : ControllerBase
    {
        private readonly ITestResultRepository repo;

        public TestResultController(ITestResultRepository repo)
        {
            this.repo = repo;
        }


        [HttpPost]
        public IActionResult Create(TestResultDTO _object)
        {
            try
            {
                repo.Add(_object);
                repo.SaveChanges();
                return Created($"api/test-resutl/{_object.Id}", _object);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var notifications = repo.GetList();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var notification = repo.Get(id);
                if (notification == null)
                {
                    return NotFound();
                }
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        public IActionResult UpdateTestResult(int userId, int testId, [FromForm] int? score, [FromForm] int? timeSpent)
        {
            try
            {
                var UpdateTestResult = repo.UpdateTestResult(userId, testId, score, timeSpent);
                return Ok(UpdateTestResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existingNotification = repo.Get(id);
                if (existingNotification == null)
                {
                    return NotFound();
                }

                repo.Delete(id);
                repo.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-tests-result")]
        public IActionResult GetTestsResult(int userId, int courseId)
        {
            try
            {
                var testResult = repo.GetTestsResult(userId, courseId);
                if (testResult == null)
                {
                    return NotFound("TestResult not found");
                }
                return Ok(testResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}