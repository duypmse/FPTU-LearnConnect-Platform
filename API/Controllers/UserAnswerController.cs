using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/user-answer")]
    [ApiController]
    [Authorize]
    public class UserAnswerController : ControllerBase
    {
        private readonly IUserAnswerRepository repo;

        public UserAnswerController(IUserAnswerRepository repo)
        {
            this.repo = repo;
        }

        [HttpPost]
        //[Authorize(Roles = "0")]
        public IActionResult Create(int userId, int testId, int[] answerIds)
        {
            try
            {
                repo.Create(userId, testId, answerIds);
                repo.SaveChanges();

                DateTime currentTime = DateTime.UtcNow.AddHours(7);

                var response = new
                {
                    Message = "Saving answer successful!",
                    TimeSubmit = currentTime
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error at Create() in UserAnswerController: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all user answers.
        /// </summary>
        /// <returns>List of user answers.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var userAnswers = repo.GetList();
                return Ok(userAnswers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-list-answer-by-test")]
        public IActionResult GetListAnswerByCourse(int userId, int testId)
        {
            try
            {
                var userAnswers = repo.Get(userId, testId);
                return Ok(userAnswers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the information of a user answer based on its ID.
        /// </summary>
        /// <param name="id">ID of the user answer to retrieve information for.</param>
        /// <returns>Information of the user answer with the corresponding ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var userAnswer = repo.Get(id);
                if (userAnswer == null)
                {
                    return NotFound();
                }
                return Ok(userAnswer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the information of a user answer.
        /// </summary>
        /// <param name="id">ID of the user answer to update.</param>
        /// <param name="_object">New user answer information.</param>
        /// <returns>Updated user answer information, or NotFound if not found.</returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, UserAnswerDTO _object)
        {
            try
            {
                var existingUserAnswer = repo.Get(id);
                if (existingUserAnswer == null)
                {
                    return NotFound();
                }

                repo.Update(id, _object);
                repo.SaveChanges();

                return Ok(_object);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a user answer based on its ID.
        /// </summary>
        /// <param name="id">ID of the user answer to delete.</param>
        /// <returns>NoContent if deletion is successful, or NotFound if not found.</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existingUserAnswer = repo.Get(id);
                if (existingUserAnswer == null)
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
