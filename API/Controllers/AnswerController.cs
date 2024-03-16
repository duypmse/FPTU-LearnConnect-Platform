using BAL.Models;
using DAL.DTO;
using DAL.Repository;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing answers.
    /// </summary>
    [Route("api/answer")]
    [ApiController]
    [Authorize]
    public class AnswerController : ControllerBase
    {
        private readonly IAnswerRepository repo;

        public AnswerController(IAnswerRepository repo)
        {
            this.repo = repo;
        }

        [HttpPost("create-answer")]
        [Authorize(Roles = "2")]
        public IActionResult CreateAnswer(int questionId, [FromForm] string answerText, [FromForm] bool isCorrect)
        {
            try
            {
                var createdAnswer = repo.CreateAnswer(questionId, answerText, isCorrect);
                repo.SaveChanges();
                return Ok(createdAnswer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all answers.
        /// </summary>
        /// <returns>A list of answers.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var answers = repo.GetList();
                return Ok(new
                {
                    Message = "duy 123",
                    Data = answers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get an answer by ID.
        /// </summary>
        /// <param name="id">The ID of the answer.</param>
        /// <returns>The answer with the specified ID.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var answer = repo.Get(id);
                if (answer == null)
                {
                    return NotFound();
                }
                return Ok(answer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "2")]
        public IActionResult UpdateAnswer(int id, [FromForm]string answerText, [FromForm] bool isCorrect)
        {
            try
            {
                var updateAnswer = repo.UpdateAnswer(id, answerText, isCorrect);
                repo.SaveChanges();
                return Ok(updateAnswer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete an answer by ID.
        /// </summary>
        /// <param name="id">The ID of the answer to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existingAnswer = repo.Get(id);
                if (existingAnswer == null)
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
