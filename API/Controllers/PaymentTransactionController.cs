using DAL.DTO;
using DAL.Repository;
using DAL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing payment transactions.
    /// </summary>
    [Route("api/payment-transaction")]
    [ApiController]
    //[Authorize]
    public class PaymentTransactionController : ControllerBase
    {
        private readonly IPaymentTransactionRepository repo;
        private readonly IVNPayService service;

        public PaymentTransactionController(IPaymentTransactionRepository repo, IVNPayService service)
        {
            this.repo = repo;
            this.service = service;
        }

        /// <summary>
        /// Create a new payment transaction.
        /// </summary>
        /// <param name="_object">The payment transaction to create.</param>
        /// <returns>The created payment transaction.</returns>
        [HttpPost]
        public ActionResult<PaymentTransactionDTO> Create(PaymentTransactionDTO _object)
        {
            repo.Add(_object);
            repo.SaveChanges();

            return Created($"api/payment-transaction/{_object.Id}", _object);
        }

        /// <summary>
        /// Get a list of all payment transactions.
        /// </summary>
        /// <returns>A list of payment transactions.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<PaymentTransactionDTO>> GetAll()
        {
            var list = repo.GetList();
            return Ok(list);
        }

        /// <summary>
        /// Get a payment transaction by ID.
        /// </summary>
        /// <param name="id">The ID of the payment transaction.</param>
        /// <returns>The payment transaction with the specified ID.</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult<PaymentTransactionDTO> Get(int id)
        {
            var _object = repo.Get(id);
            if (_object == null)
            {
                return NotFound();
            }

            return Ok(_object);
        }

        /// <summary>
        /// Update a payment transaction by ID.
        /// </summary>
        /// <param name="id">The ID of the payment transaction to update.</param>
        /// <param name="_object">The updated payment transaction data.</param>
        /// <returns>The updated payment transaction.</returns>
        [HttpPut("{id}")]
        //[Authorize]
        public ActionResult Update(int id, PaymentTransactionDTO _object)
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
        /// Delete a payment transaction by ID.
        /// </summary>
        /// <param name="id">The ID of the payment transaction to delete.</param>
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

        /// <summary>
        /// Query data transaction from VNPay API and save to database
        /// </summary>
        /// <returns>PaymentTransaction object(may be null when payment error)</returns>
        [HttpGet("query-vnpay-transaction")]
        [Authorize(Roles = "2,3")]
        public async Task<ActionResult> QueryVNPayTransaction(int vnp_TxnRef, string vnp_PayDate)
        {
            dynamic data = service.QueryVNPayTransaction(vnp_TxnRef, vnp_PayDate);
            if (data.GetType() == typeof(string))
            {
                return BadRequest(data);
            }
            var mentorId = data.GetType().GetProperty("MentorId").GetValue(data);
            var amount = data.GetType().GetProperty("PaymentTransaction").GetValue(data).Total;
            var enrollmentId = data.GetType().GetProperty("PaymentTransaction").GetValue(data).EnrollmentId;
            await service.PayRevenue(mentorId, amount, enrollmentId);
            return Ok(data);
        }

        /// <summary>
        /// Get payment transactions by User ID.
        /// </summary>
        /// <param name="userId">The ID of the user for which you want to get payment transactions.</param>
        /// <returns>A list of payment transactions for the specified user.</returns>
        [HttpGet("by-user/{userId}")]
        [Authorize(Roles = "2,3")]
        public IActionResult GetByUserId(int userId, int currentPage, int pageSize)
        {
            try
            {
                var paymentTransactions = repo.GetByUserId(userId);
                var count = paymentTransactions.Count();
                int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var paymentTransactionsPaged = paymentTransactions.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
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
                    PaymentTransactions = paymentTransactionsPaged
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payment transactions by Mentor's User ID.
        /// </summary>
        /// <param name="userId">The ID of the mentor for which you want to get payment transactions.</param>
        /// <param name="currentPage">The current page of results to retrieve.</param>
        /// <param name="pageSize">The number of items to retrieve per page.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>An HTTP 200 OK response with the list of payment transactions associated with the specified mentor.</description>
        ///     </item>
        ///     <item>
        ///       <description>An HTTP 500 Internal Server Error response in case of an exception with an error message.</description>
        ///     </item>
        ///   </list>
        /// </returns>
        [HttpGet("by-mentor/{userId}")]
        [Authorize(Roles = "2")]
        public IActionResult GetByMentorUserId(int userId, int currentPage, int pageSize)
        {
            try
            {
                var paymentTransactions = repo.GetByMentorUserId(userId);
                var count = paymentTransactions.Count();
                int totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var paymentTransactionsPaged = paymentTransactions.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
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
                    PaymentTransactions = paymentTransactionsPaged
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("revenue-mentor")]
        [Authorize(Roles = "2")]
        public ActionResult<IEnumerable<object>> GetRevenueFilterDate(int mentorUserId, DateTime? filterDate)
        {
            try
            {
                var list = repo.GetRevenueFilterDate(mentorUserId, filterDate);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("revenue-mentor-by-week")]
        [Authorize(Roles = "2")]
        public ActionResult<IEnumerable<object>> GetRevenueByWeek(int mentorUserId)
        {
            try
            {
                var list = repo.GetRevenueByWeek(mentorUserId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("revenue-web-by-week")]
        [Authorize(Roles = "1")]
        public ActionResult<IEnumerable<object>> GetRevenueFilterDateByStaff()
        {
            try
            {
                var list = repo.GetRevenueFilterDateByStaffByWeek();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("aoumt-to-pay-of-mentors-today")]
        [Authorize(Roles = "1")]
        public ActionResult<IEnumerable<object>> GetRevenueByStaffByWeek()
        {
            try
            {
                var list = repo.GetRevenueFilterDateByStaff();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get statistics for a mentor based on various filters.
        /// </summary>
        /// <param name="mentorUserId">The user ID of the mentor.</param>
        /// <param name="filterType">The type of filter to apply (e.g., "month", "week", "day").</param>
        /// <param name="sortBy">The field to use for sorting "totalenroll" or "revenue".</param>
        /// <param name="sortOrder">The order of sorting, either "asc" (ascending) or "desc" (descending) (optional).</param>
        /// <returns>Returns a list of statistics for the mentor.</returns>
        [HttpGet("statistic-mentor")]
        [Authorize(Roles = "2")]
        public ActionResult<IEnumerable<object>> StatisticMentor(int mentorUserId, string filterType, string? sortBy, string? sortOrder)
        {
            try
            {
                var list = repo.StatisticMentor(mentorUserId, filterType, sortBy, sortOrder);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get statistics for staff based on specified filter type.
        /// </summary>
        /// <param name="filterType">The type of filter to apply (e.g., "month", "week", "day").</param>
        /// <returns>
        /// Returns a collection of objects containing statistics for staff, including total statistics and daily statistics.
        /// </returns>
        /// <remarks>
        /// The API calculates various statistics for staff, such as new courses, new mentors, new enrollments, new revenue, total active courses,
        /// total mentors, total enrollments, and total revenue. The filterType parameter determines the time range for which the statistics are calculated.
        /// </remarks>
        /// <response code="200">Returns the statistics for staff.</response>
        /// <response code="500">Internal server error. Returns an error message if the server encounters an exception.</response>
        [HttpGet("statistic-staff")]
        [Authorize(Roles = "0,1")]
        public ActionResult<IEnumerable<object>> StatisticStaff(string filterType)
        {
            try
            {
                var list = repo.StatisticStaff(filterType);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("statistic-admin")]
        [Authorize(Roles = "0")]
        public ActionResult<IEnumerable<object>> StatisticAdmin(string filterType)
        {
            try
            {
                var list = repo.StatisticAdmin(filterType);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("transaction-history-staff")]
        [Authorize(Roles = "1")]
        public ActionResult<IEnumerable<object>> GetPaymentTransactionFilterDate(DateTime? filterDate, string filterType)
        {
            try
            {
                var list = repo.GetPaymentTransactionFilterDate(filterDate, filterType);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("re-pay")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> RePay(int transactionId, int enrollmentId)
        {
            try
            {
                var rePay = await service.RePay(transactionId, enrollmentId);
                repo.SaveChanges();
                return Ok(rePay);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("transaction-history-mentor")]
        [Authorize(Roles = "2")]
        public IActionResult GetPaymentTransactionOfMentor(int mentorUserId, DateTime? filterDate)
        {
            try
            {
                var rePay = repo.GetPaymentTransactionOfMentor(mentorUserId, filterDate);
                return Ok(rePay);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("statistic-revenue-mentor")]
        [Authorize(Roles = "2")]
        public IActionResult StatisticRevenueMentor(int mentorUserId, string filterType)
        {
            try
            {
                var rePay = repo.StatisticRevenueMentor(mentorUserId, filterType);
                return Ok(rePay);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
