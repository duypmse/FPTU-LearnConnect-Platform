using DAL;
using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing mentors.
    /// </summary>
    [Route("api/mentor")]
    [ApiController]
    //[Authorize]
    public class MentorController : ControllerBase
    {
        private readonly IMentorRepository repo;
        private readonly IUserRepository userRepository;
        private readonly ISpecializationOfMentorRepository specializationOfMentorRepository;

        public MentorController(IMentorRepository repo, IUserRepository userRepository, ISpecializationOfMentorRepository specializationOfMentorRepository)
        {
            this.repo = repo;
            this.userRepository = userRepository;
            this.specializationOfMentorRepository = specializationOfMentorRepository;
        }

        /// <summary>
        /// Create a new mentor record.
        /// </summary>
        /// <param name="_object">The mentor record to create.</param>
        /// <returns>The created mentor record.</returns>
        [HttpPost]
        public ActionResult<MentorDTO> Create(MentorDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/mentor/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all mentor records.
        /// </summary>
        /// <returns>A list of mentor records.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<MentorDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a mentor record by ID.
        /// </summary>
        /// <param name="id">The ID of the mentor record.</param>
        /// <returns>The mentor record with the specified ID.</returns>
        /*[HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<MentorDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }*/

        [HttpGet("{mentorId}")]
        public ActionResult<UserDTO> GetUserByMentor(int mentorId)
        {
            // Check if the mentor exists
            var mentor = repo.Get(mentorId);
            if (mentor == null)
            {
                return NotFound("Mentor not found");
            }

            // Assuming you have a UserRepository with methods similar to MentorRepository
            var user = userRepository.Get(mentor.UserId);

            var mentorDetails = new
            {
                Mentor = mentor,
                User = user
            };

            return Ok(mentorDetails);
        }

        [HttpGet("get-info/{mentorUserId}")]
        public ActionResult<UserDTO> GetUserByMentorUserId(int mentorUserId)
        {
            var mentor = repo.GetByMentorUserId(mentorUserId);

            if (mentor == null)
            {
                return NotFound("Mentor not found");
            }

            var user = userRepository.Get(mentorUserId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var specOfMentor = specializationOfMentorRepository.GetListByMentorId(mentorUserId);

            var mentorDetails = new
            {
                Mentor = mentor,
                User = user,
                Specialization = specOfMentor
            };

            return Ok(mentorDetails);
        }



        /// <summary>
        /// Update a mentor record by ID.
        /// </summary>
        /// <param name="id">The ID of the mentor record to update.</param>
        /// <param name="_object">The updated mentor record data.</param>
        /// <returns>The updated mentor record.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, MentorDTO _object)
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
        /// Delete a mentor record by ID.
        /// </summary>
        /// <param name="id">The ID of the mentor record to delete.</param>
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

        [HttpPost("process-mentor-request")]
        [Authorize(Roles = "1")]
        public IActionResult ProcessMentorRequest(int staffUserId, int mentorUserId, int specializationId, bool acceptRequest, string? rejectReason)
        {
            try
            {
                var result = repo.ProcessMentorRequest(staffUserId, mentorUserId, specializationId, acceptRequest, rejectReason);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("get-mentors")]
        public ActionResult<IEnumerable<object>> GetMentors(int currentPage, int pageSize)
        {
            try
            {
                var mentorInfo = repo.GetMentorsInfo();

                if (currentPage != 0 && pageSize != 0)
                {
                    var count = mentorInfo.Count();
                    int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                    var mentorsPaged = mentorInfo.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPage = currentPage > 1 ? true : false;
                    var nextPage = currentPage < totalPages ? true : false;

                    var paginationMetadata = new
                    {
                        count,
                        pageSize,
                        currentPage,
                        totalPages,
                        previousPage,
                        nextPage
                    };

                    return Ok(new
                    {
                        PaginationData = paginationMetadata,
                        ListMentor = mentorsPaged
                    });
                };

                return Ok(mentorInfo);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPut("update-mentor-status")]
        [Authorize(Roles = "0,1")]
        public ActionResult UpdateMentorStatus(int id, MentorStatus status)
        {
            try
            {
                var updatedMentor = repo.UpdateMentorStatus(id, status);
                if (updatedMentor == null)
                {
                    return NotFound();
                }

                return Ok(updatedMentor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("top-3-mentors")]
        public ActionResult<IEnumerable<object>> GetTop3MentorsByRating()
        {
            try
            {
                var top3Mentors = repo.GetTop3MentorsByRating();

                return Ok(top3Mentors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("become-a-mentor")]
        [Authorize(Roles = "3")]
        public IActionResult BecomeAMentor(int userId,
                                           int specializationId,
                                           string description,
                                           string reason,
                                           string accountNumber,
                                           string bankName,
                                           [FromForm] string identityCardFrontDescription,
                                           IFormFile identityCardFrontUrl,
                                           [FromForm] string identityCardBackDescription,
                                           IFormFile identityCardBackUrl,
                                           [FromForm] string descriptionDocument,
                                           IFormFile verificationDocument)
        {
            
            try
            {
                var mentorInfo = repo.BecomeMentor(userId,
                                                    specializationId,
                                                    description,
                                                    reason,
                                                    accountNumber,
                                                    bankName,
                                                    identityCardFrontDescription,
                                                    identityCardFrontUrl,
                                                    identityCardBackDescription,
                                                    identityCardBackUrl,
                                                    descriptionDocument,
                                                    verificationDocument);

                repo.SaveChanges();

                return Ok(mentorInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }


        [HttpPost("add-specialization-by-mentor")]
        [Authorize(Roles = "2")]
        public IActionResult AddSpecializationByMentor (int userId,
                                           int specializationId,
                                           [FromForm] string reason,
                                           [FromForm] string descriptionDocument,
                                           IFormFile verificationDocument)
        {

            try
            {
                var mentorInfo = repo.AddSpecializeByMentor(userId,
                                                    specializationId,
                                                    reason,
                                                    descriptionDocument,
                                                    verificationDocument);

                repo.SaveChanges();

                return Ok(mentorInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("specializations-request/{mentorUserId}/{status}")]
        [Authorize(Roles = "2")]
        public ActionResult<object> GetSpecializationsRequest(int mentorUserId, SpecializationOfMentorStatus status)
        {
            var userId = int.Parse(User.FindFirst("Id").Value);
            if (!(userId == mentorUserId))
            {
                return BadRequest("Access Denined!");
            }
            try
            {
                var result = repo.GetSpecializationsRequest(mentorUserId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ban-mentor")]
        [Authorize(Roles = "1")]
        public ActionResult<string> SetCourseStatus(int mentorUserId, [FromForm] bool status, [FromForm] string reason)
        {
            try
            {
                var action = repo.BanMentor(mentorUserId, status, reason);
                repo.SaveChanges();
                return Ok(action);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
