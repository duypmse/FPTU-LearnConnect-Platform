using BAL.Models;
using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/test")]
    [ApiController]
    //[Authorize]
    public class TestController : ControllerBase
    {
        private readonly ITestRepository repo;

        public TestController(ITestRepository repo)
        {
            this.repo = repo;
        }


        [HttpPost("create-test")]
        [Authorize(Roles = "2")]
        public IActionResult CreateTest(int courseId,[FromForm] string title, [FromForm] string description)
        {
            try
            {
                var createdTest = repo.CreateTest(courseId, title, description);

                return Ok(createdTest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all test items.
        /// </summary>
        /// <returns>List of test items.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var tests = repo.GetList();
                return Ok(tests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the information of a test item based on its ID.
        /// </summary>
        /// <param name="id">ID of the test item to retrieve information for.</param>
        /// <returns>Information of the test item with the corresponding ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var test = repo.Get(id);
                if (test == null)
                {
                    return NotFound();
                }
                return Ok(test);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "2")]
        public IActionResult UpdateTest(int id, [FromForm] string title, [FromForm] string description)
        {
            try
            {
                var updateTest = repo.UpdateTest(id, title, description);
                if (updateTest == null)
                {
                    return NotFound();
                }
                return Ok(updateTest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message});
            }
        }

        /// <summary>
        /// Delete a test item based on its ID.
        /// </summary>
        /// <param name="id">ID of the test item to delete.</param>
        /// <returns>NoContent if deletion is successful, or NotFound if not found.</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existingTest = repo.Get(id);
                if (existingTest == null)
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

        /// <summary>
        /// Get a list of tests for a specific course.
        /// </summary>
        /// <param name="courseId">ID of the course for which tests are requested.</param>
        /// <returns>List of tests for the specified course.</returns>
        [HttpGet("get-tests-by-course")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetTestsByCourse(int courseId)
        {
            try
            {
                var testsWithQuestions = repo.GetTestByCourseId(courseId);

                return Ok(testsWithQuestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("update-status-test")]
        [Authorize(Roles = "2")]
        public ActionResult<object> SetTestStatus(int testId, [FromQuery] bool status)
        {
            try
            {
                var action = repo.UpdateStatusTest(testId, status);
                repo.SaveChanges();
                return Ok(action);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("process-test-request")]
        [Authorize(Roles = "1")]
        public IActionResult ProcessLectureRequest(int testId, bool acceptRequest, string? note)
        {
            try
            {
                var result = repo.ProcessRequest(testId, acceptRequest, note);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message});
            }
        }

        [HttpGet("get-test-by-course")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetTestByCourse(int courseId, int testId)
        {
            try
            {
                var testsWithQuestions = repo.GetTest(courseId, testId);

                return Ok(testsWithQuestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
