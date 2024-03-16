using BAL.Models;
using DAL;
using DAL.DTO;
using DAL.Repository;
using DAL.Service;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository repo;
        private IConfiguration Configuration { get; }
        private readonly IMentorRepository mentorRepository;
        private readonly IFirebaseService firebaseService;

        public UserController(IUserRepository repo, IConfiguration configuration, IMentorRepository mentorRepository, IFirebaseService firebaseService)
        {
            this.repo = repo;
            Configuration = configuration;
            this.mentorRepository = mentorRepository;
            this.firebaseService = firebaseService;
        }

        /// <summary>
        /// Authenticate and log in a user using Google Firebase.
        /// </summary>
        /// <param name="accessToken">Access token for Firebase authentication.</param>
        /// <returns>Result of the login operation.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody]string accessToken)
        {
            var userRecord1 = firebaseService.LoginAsync(accessToken).Result;
            if (userRecord1 == null)
            {
                return BadRequest("Fail to login with Google!");          
            }
            if (userRecord1 is string)
            {
                return Unauthorized(userRecord1);
            }
            UserRecord userRecord = (UserRecord)userRecord1;
           var user = repo.GetByEmail(userRecord.Email);
            if (user == null)
            {
                //Create new user from userRecord 
                var name = userRecord.DisplayName.Split(" (")[0];

                var newUser = new UserDTO { 
                    Email = userRecord.Email,
                    Role = (int) Roles.Student,
                    FullName = name,
                    Password = "1",
                    Gender = 3,
                    RegistrationDate = DateTime.UtcNow.AddHours(7),
                    LastLoginDate = DateTime.UtcNow.AddHours(7),
                    ProfilePictureUrl = userRecord.PhotoUrl,
                    Status = (int) UserStatus.Active
                };

                repo.Add(newUser);
                repo.SaveChanges();
            }

            user = repo.GetByEmail(userRecord.Email);

            var isRequestBecomeMentor = repo.IsRequestBecomeMentor(user.Id);

            return Ok(new
            {
                Message = "Authentication success: "+ userRecord.DisplayName,
                Data = repo.GenerateToken(user),
                IsRequestBecomeMentor = isRequestBecomeMentor
            });
        }


        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="_object">The user information to be created.</param>
        /// <returns>The created user information.</returns>
        [HttpPost]
        [Authorize(Roles = "0")]
        public ActionResult<UserDTO> Create(UserDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/user/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all users.
        /// </summary>
        /// <returns>List of user information.</returns>
        [HttpGet]
        [Authorize(Roles = "0")]
        public ActionResult<IEnumerable<UserDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get the information of a user based on their ID.
        /// </summary>
        /// <param name="id">ID of the user to retrieve information for.</param>
        /// <returns>Information of the user with the corresponding ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        [Authorize]
        //[Authorize(Roles = "Admin")]
        public ActionResult<UserDTO> Get(int id)
        {
            var userId = int.Parse(User.FindFirst("Id").Value);
            if (!(User.IsInRole("0") || userId == id))
            {
                return BadRequest("Only role Admin or userId = jwtDecode.Id");
            }
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update the information of a user.
        /// </summary>
        /// <param name="id">ID of the user to update.</param>
        /// <param name="_object">New user information.</param>
        /// <returns>Updated user information, or NotFound if not found.</returns>
        [HttpPut("{id}")]
        //[Authorize(Roles = "0")]
        [Authorize]
        public ActionResult Update(int id, UserDTO _object)
        {
            var userId = int.Parse(User.FindFirst("Id").Value);
            if (!(User.IsInRole("0") || userId == id))
            {
                return BadRequest("Only role Admin or userId = jwtDecode.Id");
            }

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

        [HttpPut("update-info-user")]
        [Authorize]
        public IActionResult Update(int id, [FromForm] int? gender, [FromForm] string? phoneNumber, [FromForm] string? biography, [FromForm] string? paypalId, [FromForm] string? paypalAddress)
        {
            var userId = int.Parse(User.FindFirst("Id").Value);
            if (!(User.IsInRole("0") || userId == id))
            {
                return BadRequest("Only role Admin or userId = jwtDecode.Id");
            }

            try
            {
                var updated = repo.Update(id, gender, phoneNumber, biography, paypalId, paypalAddress);

                if (updated == null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a user based on their ID.
        /// </summary>
        /// <param name="id">ID of the user to delete.</param>
        /// <returns>NoContent if deletion is successful, or NotFound if not found.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "0")]
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
        /// Updates the role of a user by their ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The new role to assign to the user.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item>
        ///     <term>200 OK</term>
        ///     <description>If the role update is successful.</description>
        ///   </item>
        ///   <item>
        ///     <term>400 Bad Request</term>
        ///     <description>If the provided role is invalid or the user is already an admin.</description>
        ///   </item>
        ///   <item>
        ///     <term>404 Not Found</term>
        ///     <description>If the user with the specified ID is not found.</description>
        ///   </item>
        ///   <item>
        ///     <term>500 Internal Server Error</term>
        ///     <description>If an unexpected error occurs during the role update process.</description>
        /// </list>
        /// </returns>
        [HttpPut("update-user-role")]
        [Authorize(Roles = "0,1")]
        public IActionResult UpdateUserRole(int userId, Roles role)
        {
            try
            {
                var user = repo.Get(userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                string roleName = Enum.GetName(typeof(Roles), role);

                if (role == Roles.Admin && user.Role == (int)Roles.Admin)
                {
                    return BadRequest("You are already an admin");
                }

                switch (role)
                {
                    case Roles.Admin:
                        user.Role = (int)Roles.Admin;
                        break;
                    case Roles.Staff:
                        user.Role = (int)Roles.Staff;
                        break;
                    case Roles.Mentor:
                        user.Role = (int)Roles.Mentor;
                        break;
                    case Roles.Student:
                        user.Role = (int)Roles.Student;
                        break;
                    default:
                        return BadRequest("Invalid role: " + roleName);
                }

                repo.Update(userId, user);
                repo.SaveChanges();

                return Ok("Role updated successfully: " + roleName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// API endpoint for searching and retrieving a list of users.
        /// </summary>
        /// <param name="searchQuery">The search query to filter users by name (optional).</param>
        /// <param name="currentPage">The current page number.</param>
        /// <param name="pageSize">The number of users to return per page.</param>
        /// <returns>Returns a list of users matching the search criteria, along with pagination metadata.</returns>
        [HttpGet("search")]
        [Authorize(Roles = "0")]
        public IActionResult SearchUsers(string? searchQuery, int currentPage, int pageSize)
        {
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    // If there's no search query, return all users
                    var users = repo.GetList();
                    var countNull = users.Count();
                    int totalPagesNull = (int)Math.Ceiling(countNull / (double)pageSize);
                    var usersPagedNull = users.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    var previousPageNull = currentPage > 1 ? true : false;
                    var nextPageNull = currentPage < totalPagesNull ? true : false;

                    // Object which we are going to send in header
                    var paginationMetadataNull = new
                    {
                        countNull,
                        pageSize,
                        currentPage,
                        totalPagesNull,
                        previousPageNull,
                        nextPageNull
                    };

                    return Ok(new
                    {
                        PaginationData = paginationMetadataNull,
                        ListUser = usersPagedNull
                    });
                }

                // If there is a search query, filter users by name
                var filteredUsers = repo.GetList()
                    .Where(user => user.FullName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (filteredUsers.Count == 0)
                {
                    return NotFound("No users found matching the search query.");
                }

                var count = filteredUsers.Count();
                int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var usersPaged = filteredUsers.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var previousPage = currentPage > 1 ? true : false;
                var nextPage = currentPage < totalPages ? true : false;

                // Object which we are going to send in header
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
                    ListUser = usersPaged
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// API endpoint to allow a user to log in using email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>Returns a response indicating whether the login attempt was successful, including an authentication token.</returns>
        [HttpPost("login-by-email")]
        public async Task<IActionResult> LoginByEmailAsync([FromForm] string email, [FromForm] string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Email và mật khẩu là bắt buộc.");
            }

            var user = repo.GetByEmail(email);

            if (user == null || user.Password != password)
            {
                return BadRequest("Email hoặc mật khẩu không hợp lệ.");
            }

            /*if (user.Role != 0 && user.Role != 1 && user.Role != 2)
            {
                return Unauthorized("Truy cập bị từ chối. Chỉ người dùng có vai trò 0 hoặc 1 được phép.");
            }*/

            var token = repo.GenerateToken(user);

            var isRequestBecomeMentor = repo.IsRequestBecomeMentor(user.Id);

            return Ok(new
            {
                Message = "Xác thực thành công",
                Data = token,
                IsRequestBecomeMentor = isRequestBecomeMentor
            });
        }

        [HttpPost("create-account-staff")]
        [Authorize(Roles = "0")]
        public IActionResult CreateStaffAccount([FromForm] string fullName, [FromForm] string email, [FromForm] string password)
        {
            try
            {
                var createStaffAccount = repo.CreateStaffAccount(fullName, email, password);
                repo.SaveChanges();
                return Ok(createStaffAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




    }
}
