using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing questions.
    /// </summary>
    [Route("api/question")]
    [ApiController]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionRepository repo;

        public QuestionController(IQuestionRepository repo)
        {
            this.repo = repo;
        }

        
        [HttpPost("create-question")]
        [Authorize(Roles = "2")]
        public IActionResult CreateQuestion(int testId, [FromForm] string questionText)
        {
            try
            {
                var createdQuestion = repo.CreateQuestion(testId, questionText);
                repo.SaveChanges();
                return Ok(createdQuestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all questions.
        /// </summary>
        /// <returns>A list of questions.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var questions = repo.GetList();
                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a question by ID.
        /// </summary>
        /// <param name="id">The ID of the question.</param>
        /// <returns>The question with the specified ID.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var question = repo.Get(id);
                if (question == null)
                {
                    return NotFound();
                }
                return Ok(question);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "2")]
        public IActionResult UpdateQuestion(int id, [FromForm]string questionText)
        {
            try
            {
                var updateQuestion = repo.UpdateQuestion(id, questionText);
                repo.SaveChanges();
                return Ok(updateQuestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a question by ID.
        /// </summary>
        /// <param name="id">The ID of the question to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existingQuestion = repo.Get(id);
                if (existingQuestion == null)
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
    }
}
