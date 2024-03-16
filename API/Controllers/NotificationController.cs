using DAL.DTO;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing notifications.
    /// </summary>
    [Route("api/notification")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository repo;

        public NotificationController(INotificationRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Create a new notification.
        /// </summary>
        /// <param name="_object">The notification to create.</param>
        /// <returns>The created notification.</returns>
        [HttpPost]
        public IActionResult Create(NotificationDTO _object)
        {
            try
            {
                repo.Add(_object);
                repo.SaveChanges();
                return Created($"api/notification/{_object.Id}", _object);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a list of all notifications.
        /// </summary>
        /// <returns>A list of notifications.</returns>
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

        /// <summary>
        /// Get a notification by ID.
        /// </summary>
        /// <param name="id">The ID of the notification.</param>
        /// <returns>The notification with the specified ID.</returns>
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

        /// <summary>
        /// Update a notification by ID.
        /// </summary>
        /// <param name="id">The ID of the notification to update.</param>
        /// <param name="_object">The updated notification data.</param>
        /// <returns>The updated notification.</returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, NotificationDTO _object)
        {
            try
            {
                var existingNotification = repo.Get(id);
                if (existingNotification == null)
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
        /// Delete a notification by ID.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        /// <returns>No content.</returns>
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

        /// <summary>
        /// Get a list of all notifications by user ID, sorted by ID from highest to lowest.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve notifications.</param>
        /// <returns>A list of notifications by user ID, sorted by ID from highest to lowest.</returns>
        [HttpGet("byUserId/{userId}")]
        public IActionResult GetNotificationsByUserId(int userId)
        {
            try
            {
                var notifications = repo.GetNotificationsByUserId(userId);

                // Sort the notifications by ID in descending order
                //notifications = notifications.OrderByDescending(notification => notification.Id).ToList();

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        /// <summary>
        /// Get a list of notifications by user ID with pagination, sorted by ID from highest to lowest.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve notifications.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="pageSize">The number of items to display on each page.</param>
        /// <returns>A paginated list of notifications by user ID, sorted by ID from highest to lowest.</returns>
        [HttpGet("byUserId-pagination/{userId}")]
        public IActionResult GetNotificationsByUserId(int userId, int currentPage, int pageSize)
        {
            try
            {
                var notifications = repo.GetNotificationsByUserId(userId);
                repo.MarkNotificationsAsRead(userId);

                // Sort the notifications by ID in descending order
                //notifications = notifications.OrderByDescending(notification => notification.Id);

                // Calculate the total count and the number of total pages
                int totalCount = notifications.Count();
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Apply pagination to the results
                var pagedNotifications = notifications.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

                // Create pagination metadata
                var paginationMetadata = new
                {
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = currentPage,
                    TotalPages = totalPages,
                    HasPreviousPage = currentPage > 1,
                    HasNextPage = currentPage < totalPages
                };

                return Ok(new
                {
                    PaginationData = paginationMetadata,
                    Notifications = pagedNotifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
