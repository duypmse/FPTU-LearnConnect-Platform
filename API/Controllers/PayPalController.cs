using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DAL.Repository;
using DAL;
using BAL.Models;
using Microsoft.EntityFrameworkCore;
using DAL.DTO;
using DAL.Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalController : ControllerBase
    {
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly LearnConnectDBContext _context;
        private readonly IVNPayService _vNPayService;

        public PayPalController(IPaymentTransactionRepository paymentTransactionRepository, LearnConnectDBContext context, IVNPayService vNPayService)
        {
            _paymentTransactionRepository = paymentTransactionRepository;
            _context = context;
            _vNPayService = vNPayService;
        }


        [HttpPost("pay")]
        public async Task<IActionResult> Pay(int mentorId, int amount, int enrollmentId)
        {
            try
            {
                var pay = await _vNPayService.PayRevenue(mentorId, amount, enrollmentId);
                _context.SaveChanges();
                return Ok(pay);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        private readonly string _clientId = "AeMi9ws-M39AJLFE4G37RuIPv2E6MkdjpvwFOOmVoGaFvoMev7EwVdPZKKtz2t3Naxwf02UF1VEZUPIA";
        private readonly string _secret = "EExqdfogevQfZ8bMImEigTjS_IWSQkvNVIKn7CU8XSDyHb0OsFZuEjHz8vYKzn7qyEzuUcvbunj5DE1z";
        private readonly string _payoutUrl = "https://api.sandbox.paypal.com/v1/payments/payouts";
        private readonly string _authUrl = "https://api-m.sandbox.paypal.com/v1/oauth2/token";

        [HttpPost("pay-revenue")]
        public async Task<IActionResult> PayRevenue(int mentorId)
        {
            DateTime filterDate = DateTime.Today;

            var mentors = _context.Mentors
                .FirstOrDefault(m => m.Id == mentorId);


            var dataCoursePayment = _context.Courses
                .Where(c => c.Status == (int)CourseStatus.Active && c.MentorId == mentorId
                    && c.Enrollments.Any(enrollment => enrollment.PaymentTransactions.FirstOrDefault().SuccessDate.Value.Date == filterDate.Date))
                .Select(course => new
                {
                    TotalRevenueCourse = (course.Enrollments
                        .Where(enrollment => enrollment.PaymentTransactions.FirstOrDefault().SuccessDate.Value.Date == filterDate.Date
                            && enrollment.PaymentTransactions.FirstOrDefault().Status == (int)TransactionStatus.Success)
                        .Sum(enrollment => enrollment.PaymentTransactions.FirstOrDefault().Total)) * 0.95,
                })
                .ToList();

            var totalRevenue = dataCoursePayment.Sum(item => item.TotalRevenueCourse);

            try
            {
                double exchangeRate = await GetExchangeRate("USD");

                if (exchangeRate <= 0)
                {
                    return BadRequest("Failed to fetch exchange rate.");
                }

                var amountToPay = totalRevenue / exchangeRate;
                amountToPay = Math.Round(amountToPay, 2);

                string formattedAmount = amountToPay.ToString("F2");

                string accessToken;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("Accept-Language", "en_US");

                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}"));
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                    var requestData = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                    var response = await client.PostAsync(_authUrl, requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        accessToken = (string)JObject.Parse(responseContent)["access_token"];

                    }
                    else
                    {
                        return null;
                    }
                }

                if (string.IsNullOrWhiteSpace(mentors.PaypalAddress) || amountToPay <= 0)
                {
                    return BadRequest("Invalid request. Please provide a valid email and a positive amount.");
                }

                var payoutRequest = new
                {
                    sender_batch_header = new
                    {
                        sender_batch_id = Guid.NewGuid().ToString(),
                        email_subject = "Payment from Your App",
                    },
                    items = new[]
                    {
                        new
                        {
                            recipient_type = "EMAIL",
                            amount = new
                            {
                                value = formattedAmount,
                                currency = "USD",
                            },
                            receiver = mentors.PaypalAddress,
                            note = "Payment of revenue for courses sold today on LearnConnect",
                        }
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(payoutRequest);

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                    var paymentTransaction = new PaymentTransaction
                    {
                        Total = (int)totalRevenue,
                        TransactionType = (int)TransactionTypeStatus.Pay,
                        CreateDate = DateTime.UtcNow.AddHours(7),
                        Status = (int)TransactionStatus.Pending,
                        MentorId = mentorId,
                        //TransactionId = (int)transactionIDPayPal,

                    };
                    _context.PaymentTransactions.Add(paymentTransaction);
                    _context.SaveChanges();
                    var response = await httpClient.PostAsync(_payoutUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var paypalError = JObject.Parse(responseContent);
                    var errorDetails = paypalError["details"]?[0]?["issue"]?.ToString() ?? "Unknown error details";

                    //var transactionIDPayPal = (string)JObject.Parse(responseContent)["batch_header"]["payout_batch_id"];


                    if (response.IsSuccessStatusCode)
                    {
                        paymentTransaction.Status = (int)TransactionStatus.Success;
                        paymentTransaction.SuccessDate = DateTime.UtcNow.AddHours(7);
                        _context.SaveChanges();
                        return Ok("Payment sent successfully");
                    }
                    else
                    {
                        paymentTransaction.Status = (int)TransactionStatus.Success;
                        paymentTransaction.TransactionError = errorDetails;
                        _context.SaveChanges();
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return BadRequest($"Failed to send payment. {errorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<double> GetExchangeRate(string targetCurrency)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://open.er-api.com/v6/latest/{targetCurrency}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var exchangeRate = (double)JObject.Parse(responseContent)["rates"]["VND"];

                    return exchangeRate;
                }
                return -1;
            }
        }
    }

    
}

