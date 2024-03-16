using BAL.Models;
using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Xml.Linq;
using DAL.Repository;
using CloudinaryDotNet;
using Firebase.Auth;

namespace DAL.Service
{
    public interface IVNPayService
    {
        public string EnrollCourse(int userId, int courseId, string returnUrl);
        public object QueryVNPayTransaction(int paymentTransactionId, string vnPayTransactionDate);
        Task<object> PayRevenue(int mentorId, int totalRevenue, int enrollmentId);
        public Task<object> RePay(int transactionId, int enrollmentId);

    }
    public class VNPayService : IVNPayService
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMailService mailService;
        private readonly INotificationRepository notificationRepository;

        public VNPayService(LearnConnectDBContext context, IMailService mailService, INotificationRepository notificationRepository)
        {
            _context = context;
            this.mailService = mailService;
            this.notificationRepository = notificationRepository;
        }

        //        public string EnrollCourse(int userId, int courseId, string returnUrl)
        //        {
        //            var enrollmentTmp = _context.Enrollments.FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId);
        //            if (enrollmentTmp != null)
        //            {
        //                if (enrollmentTmp.Status == (int)EnrollmentStatus.InProcessing)
        //                {
        //                    throw new Exception("The user has already enrolled this course");
        //                }
        //                else if (enrollmentTmp.Status == (int)EnrollmentStatus.Completed)
        //                {
        //                    throw new Exception("The user has already completed this course");
        //                }
        //                else if (enrollmentTmp.Status == (int)EnrollmentStatus.Pending)
        //                {
        //                    var paymentTransaction = _context.PaymentTransactions.Find(enrollmentTmp.PaymentTransactionId);
        //                    if (paymentTransaction.Status == (int)TransactionStatus.Pending)
        //                    {
        //                        return paymentTransaction.PaymentUrl;
        //                    }
        //                    else
        //                    {

        //                    }
        //                }
        //            }

        //            var vnPayUrl = (string)null;
        //            var course = _context.Courses
        //                .Where(c => c.Status == (int)CourseStatus.Active)
        //                .FirstOrDefault(c => c.Id == courseId);

        //            if(course == null)
        //            {
        //                throw new Exception("The course is not active, please reload the website and enroll again!");
        //            }

        //            var enrollment = new Enrollment
        //            {
        //                IsFree = course.Price > 0 ? false : true,
        //                UserId = userId,
        //                CourseId = courseId,
        //                Status = (int)EnrollmentStatus.Pending,
        //            };

        //            if (enrollment.IsFree)
        //            {
        //                enrollment.EnrollmentDate = DateTime.UtcNow.AddHours(7);
        //                enrollment.Status = (int)EnrollmentStatus.InProcessing;

        //                var paymentTransactionTmp = new PaymentTransaction
        //                {
        //                    Total = 0,
        //                    CreateDate = DateTime.UtcNow.AddHours(7),
        //                    SuccessDate = DateTime.UtcNow.AddHours(7),
        //                    Status = (int)TransactionStatus.Success,
        //                };
        //                var paymentTransaction = _context.PaymentTransactions.Add(paymentTransactionTmp).Entity;
        //                _context.SaveChanges();

        //                var learningPerformanceTmp = new LearningPerformance
        //                {
        //                    Score = 0,
        //                    TimeSpent = 0,
        //                    UserId = userId,
        //                    CourseId = courseId,
        //                };
        //                _context.LearningPerformances.Add(learningPerformanceTmp);

        //                var learningProcessTmp = new LearningProcess
        //                {
        //                    PercentComplete = 0,
        //                    Status = 1,
        //                    CourseId = courseId,
        //                    UserId = userId
        //                };
        //                _context.LearningProcesses.Add(learningProcessTmp);

        //                enrollment.PaymentTransactionId = paymentTransaction.Id;

        //                _context.Enrollments.Add(enrollment);
        //                _context.SaveChanges();
        //                var enrollmentData = _context.Enrollments
        //                                .Include(e => e.User)
        //                                .Include(e => e.Course)
        //                                .ThenInclude(c => c.Mentor)
        //                                .Include(e => e.PaymentTransaction)
        //                                .FirstOrDefault(e => e.PaymentTransactionId == enrollment.PaymentTransactionId);
        //                if (enrollmentData != null)
        //                {
        //                    var usersReceive = _context.Users.Where(u => u.Id == enrollmentData.Course.Mentor.UserId).Select(u => u.Id).ToArray();
        //                    var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        //                    if (user != null && course != null)
        //                    {
        //                        notificationRepository.Create(
        //                            "New Enrollment",
        //                            $"Great news! A new student {user.FullName} has enrolled in your course {course.Name}.",
        //                            usersReceive
        //                        );
        //                    }
        //                }

        //                var statusText = string.Empty;

        //                switch (enrollmentData.PaymentTransaction.Status)
        //                {
        //                    case 1:
        //                        statusText = "Error";
        //                        break;
        //                    default:
        //                        statusText = "Success";
        //                        break;
        //                }

        //                var emailContent = $@"
        //<table style='width: 95%; max-width: 600px; border-collapse: collapse; margin: 20px auto; border: 2px solid rgb(40, 231, 97); border-radius: 10px; overflow: hidden; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); background-color: #f9fdf7;'>
        //    <tr>
        //        <th colspan='2' style='border-top-left-radius: 10px; border-top-right-radius: 10px; background-color: green; color: white; text-align: center; font-size: 22px; padding: 20px; border-top: 2px solid #ddd; border-bottom: 2px solid #ddd;'>Payment Transaction Details</th>
        //    </tr>
        //    <tr>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Course Name</strong></td>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollment.Course.Name}</td>
        //    </tr>
        //    <tr>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Price</strong></td>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.Total}</td>
        //    </tr>
        //    <tr>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Payment Status</strong></td>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'><span style='color: {(statusText == "Error" ? "#e53935" : (statusText == "Success" ? "#43a047" : "#333"))};'>{statusText}</span></td>
        //    </tr>"; if (!string.IsNullOrEmpty(enrollmentData.PaymentTransaction.TransactionError)) { emailContent += $@"
        //    <tr>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: red;'>Transaction Error</strong></td>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.TransactionError}</td>
        //    </tr>"; }
        //                emailContent += $@"
        //    <tr>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Trans. Date, Time</strong></td>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.CreateDate}</td>
        //    </tr>"; if (enrollmentData.PaymentTransaction.SuccessDate != null) { emailContent += $@"
        //    <tr>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: green;'>Success Date</strong></td>
        //        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.SuccessDate}</td>
        //    </tr>"; }
        //                emailContent += @"
        //    <tr>
        //        <td colspan='2' style='border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; padding: 20px;'>
        //            <p style='margin: 15px 0; font-size: 16px; color: #555;'>Thank you for your purchase!</p>
        //        </td>
        //    </tr>
        //</table>";

        //                mailService.SendMail(new List<string> { enrollment.User.Email }, $"Receipt for Your Payment - Course: {enrollment.Course.Name}", emailContent);
        //            }
        //            if (!enrollment.IsFree)
        //            {
        //                var paymentTransactionTmp = new PaymentTransaction
        //                {
        //                    Total = (int)course.Price,
        //                    CreateDate = DateTime.UtcNow.AddHours(7),
        //                    Status = (int)TransactionStatus.Pending,
        //                };
        //                var paymentTransaction = _context.PaymentTransactions.Add(paymentTransactionTmp).Entity;
        //                _context.SaveChanges();

        //                enrollment.PaymentTransactionId = paymentTransaction.Id;

        //                vnPayUrl = CreateVNPayTransaction(paymentTransaction, "::1", returnUrl);

        //                paymentTransaction.PaymentUrl = vnPayUrl;
        //                _context.SaveChanges();
        //                _context.Enrollments.Add(enrollment);

        //            }

        //            var totalEnroll = _context.Enrollments
        //                    .Where(e => e.CourseId == courseId && e.PaymentTransaction.Status == (int)TransactionStatus.Success).ToList().Count;

        //            var courseEnroll = _context.Courses.Find(courseId);
        //            courseEnroll.TotalEnrollment = totalEnroll + 1;

        //            _context.SaveChanges();
        //            return vnPayUrl;
        //        }

        public string EnrollCourse(int userId, int courseId, string returnUrl)
        {
            var enrollmentTmp = _context.Enrollments.FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId);
            if (enrollmentTmp != null)
            {
                if (enrollmentTmp.Status == (int)EnrollmentStatus.InProcessing)
                {
                    throw new Exception("The user has already enrolled this course");
                }
                else if (enrollmentTmp.Status == (int)EnrollmentStatus.Completed)
                {
                    throw new Exception("The user has already completed this course");
                }
                else if (enrollmentTmp.Status == (int)EnrollmentStatus.Pending)
                {
                    var paymentTransaction = _context.PaymentTransactions.First(p => p.EnrollmentId == enrollmentTmp.Id);
                    if (paymentTransaction.Status == (int)TransactionStatus.Pending)
                    {
                        return paymentTransaction.PaymentUrl;
                    }
                    else
                    {

                    }
                }
            }

            var vnPayUrl = (string)null;
            var course = _context.Courses.Find(courseId);

            var enrollment = new Enrollment
            {
                IsFree = course.Price > 0 ? false : true,
                UserId = userId,
                CourseId = courseId,
                Status = (int)EnrollmentStatus.Pending,
            };

            if (enrollment.IsFree)
            {
                enrollment.EnrollmentDate = DateTime.UtcNow.AddHours(7);
                enrollment.Status = (int)EnrollmentStatus.InProcessing;

                _context.Enrollments.Add(enrollment);
                _context.SaveChanges();

                var paymentTransactionTmp = new PaymentTransaction
                {
                    Total = 0,
                    CreateDate = DateTime.UtcNow.AddHours(7),
                    SuccessDate = DateTime.UtcNow.AddHours(7),
                    Status = (int)TransactionStatus.Success,
                    EnrollmentId = enrollment.Id
                };
                var paymentTransaction = _context.PaymentTransactions.Add(paymentTransactionTmp).Entity;
                _context.SaveChanges();

                //var learningPerformanceTmp = new LearningPerformance
                //{
                //    Score = 0,
                //    TimeSpent = 0,
                //    UserId = userId,
                //    CourseId = courseId,
                //};
                //_context.LearningPerformances.Add(learningPerformanceTmp);
                //SaveChanges();

                var statusText = string.Empty;

                switch (paymentTransaction.Status)
                {
                    case 1:
                        statusText = "Error";
                        break;
                    default:
                        statusText = "Success";
                        break;
                }

                var emailContent = $@"
<table style='width: 95%; max-width: 600px; border-collapse: collapse; margin: 20px auto; border: 2px solid rgb(40, 231, 97); border-radius: 10px; overflow: hidden; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); background-color: #f9fdf7;'>
    <tr>
        <th colspan='2' style='border-top-left-radius: 10px; border-top-right-radius: 10px; background-color: green; color: white; text-align: center; font-size: 22px; padding: 20px; border-top: 2px solid #ddd; border-bottom: 2px solid #ddd;'>Payment Transaction Details</th>
    </tr>
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Course Name</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollment.Course.Name}</td>
    </tr>
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Price</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.Total}</td>
    </tr>
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Payment Status</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'><span style='color: {(statusText == "Error" ? "#e53935" : (statusText == "Success" ? "#43a047" : "#333"))};'>{statusText}</span></td>
    </tr>"; if (!string.IsNullOrEmpty(paymentTransaction.TransactionError)) { emailContent += $@"
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: red;'>Transaction Error</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.TransactionError}</td>
    </tr>"; }
                emailContent += $@"
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Trans. Date, Time</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.CreateDate}</td>
    </tr>"; if (paymentTransaction.SuccessDate != null) { emailContent += $@"
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: green;'>Success Date</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.SuccessDate}</td>
    </tr>"; }
                emailContent += @"
    <tr>
        <td colspan='2' style='border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; padding: 20px;'>
            <p style='margin: 15px 0; font-size: 16px; color: #555;'>Thank you for your purchase!</p>
        </td>
    </tr>
</table>";
                var enrollmentData = _context.Enrollments
                                                .Include(e => e.User)
                                                .Include(e => e.Course)
                                                .ThenInclude(c => c.Mentor)
                                                .FirstOrDefault(e => e.Id == enrollment.Id);
                try
                {
                    if (enrollmentData != null)
                    {
                        var message = new MailMessage();
                        message.From = new MailAddress("minhduy1511@gmail.com");
                        message.To.Add(enrollmentData.User.Email);
                        message.Subject = $"Receipt for Your Payment - Course: {enrollmentData.Course.Name}";
                        message.Body = emailContent;
                        message.IsBodyHtml = true;

                        var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                        smtpClient.Credentials = new NetworkCredential("minhduy1511@gmail.com", "dxhuwemtdtkobzoj");
                        smtpClient.EnableSsl = true;

                        smtpClient.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to send email" + ex.Message);
                }
            }
            if (!enrollment.IsFree)
            {
                _context.Enrollments.Add(enrollment);
                _context.SaveChanges();

                var paymentTransactionTmp = new PaymentTransaction
                {
                    Total = (int)course.Price,
                    CreateDate = DateTime.UtcNow.AddHours(7),
                    Status = (int)TransactionStatus.Pending,
                    EnrollmentId = enrollment.Id,
                };
                var paymentTransaction = _context.PaymentTransactions.Add(paymentTransactionTmp).Entity;
                _context.SaveChanges();

                vnPayUrl = CreateVNPayTransaction(paymentTransaction, "::1", returnUrl);

                paymentTransaction.PaymentUrl = vnPayUrl;
                _context.SaveChanges();

            }

            var totalEnroll = _context.Enrollments
                    .Where(e => e.CourseId == courseId && (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed)).ToList().Count;

            var courseEnroll = _context.Courses.Find(courseId);
            courseEnroll.TotalEnrollment = totalEnroll + 1;

            _context.SaveChanges();
            return vnPayUrl;
        }

        public string CreateVNPayTransaction(PaymentTransaction paymentTransaction, string ipAddress, string returnUrl)
        {
            //Get Config Info
            string vnp_Returnurl = returnUrl; //URL nhan ket qua tra ve 
            string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = "ZKQGUFKT"; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = "ADEXUMJMCQXZCJFEIQGIFBTRXUSXRMWF"; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (paymentTransaction.Total * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_BankCode", "VNBANK"); // "VNPAYQR", "VNBANK", "INTCARD"
            vnpay.AddRequestData("vnp_CreateDate", ((DateTime)paymentTransaction.CreateDate).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn"); //en, vn
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + paymentTransaction.Id);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", paymentTransaction.Id.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing
            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return paymentUrl;
        }

        public object QueryVNPayTransaction(int paymentTransactionId, string vnPayTransactionDate)
        {
            var vnp_Api = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
            var vnp_HashSecret = "ADEXUMJMCQXZCJFEIQGIFBTRXUSXRMWF"; //Secret KEy
            var vnp_TmnCode = "ZKQGUFKT"; // Terminal Id

            var vnp_RequestId = DateTime.Now.Ticks.ToString(); //Mã hệ thống merchant tự sinh ứng với mỗi yêu cầu truy vấn giao dịch. Mã này là duy nhất dùng để phân biệt các yêu cầu truy vấn giao dịch. Không được trùng lặp trong ngày.
            var vnp_Version = VnPayLibrary.VERSION; //2.1.0
            var vnp_Command = "querydr";
            var vnp_TxnRef = paymentTransactionId; // Mã giao dịch thanh toán tham chiếu
            var vnp_OrderInfo = "Truy van giao dich:" + paymentTransactionId;
            var vnp_TransactionDate = vnPayTransactionDate;
            var vnp_CreateDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
            var vnp_IpAddr = "::1";

            var signData = vnp_RequestId + "|" + vnp_Version + "|" + vnp_Command + "|" + vnp_TmnCode + "|" + vnp_TxnRef + "|" + vnp_TransactionDate + "|" + vnp_CreateDate + "|" + vnp_IpAddr + "|" + vnp_OrderInfo;
            var vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);

            var qdrData = new
            {
                vnp_RequestId = vnp_RequestId,
                vnp_Version = vnp_Version,
                vnp_Command = vnp_Command,
                vnp_TmnCode = vnp_TmnCode,
                vnp_TxnRef = vnp_TxnRef,
                vnp_OrderInfo = vnp_OrderInfo,
                vnp_TransactionDate = vnp_TransactionDate,
                vnp_CreateDate = vnp_CreateDate,
                vnp_IpAddr = vnp_IpAddr,
                vnp_SecureHash = vnp_SecureHash
            };
            var jsonData = JsonConvert.SerializeObject(qdrData);

            var paymentTrasactionDTO = new PaymentTransaction();
            var enrollment = new Enrollment();
            var paymentSuccess = false;

            try
            {
                var httpClient = new HttpClient();
                StringContent httpContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                var response = httpClient.PostAsync(vnp_Api, httpContent).Result;
                var strData = response.Content.ReadAsStringAsync().Result;
                var jObject = JObject.Parse(strData);

                var querySuccess = (string)jObject["vnp_ResponseCode"];
                if (querySuccess.Equals("00"))
                {
                    paymentTrasactionDTO = _context.PaymentTransactions.Find(paymentTransactionId);
                    enrollment = _context.Enrollments
                            //.Include(e => e.PaymentTransactions.First())
                            .Include(e => e.Course)
                            .FirstOrDefault(x => x.Id == paymentTrasactionDTO.EnrollmentId);


                    var vnpTransactionStatus = (string)jObject["vnp_TransactionStatus"];
                    paymentTrasactionDTO.TransactionId = (string)jObject["vnp_TransactionNo"];

                    if (vnpTransactionStatus.Equals("00"))
                    {
                        paymentSuccess = true;

                        paymentTrasactionDTO.SuccessDate = DateTime.ParseExact((string)jObject["vnp_PayDate"], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        paymentTrasactionDTO.Status = (int)TransactionStatus.Success;

                        enrollment.Status = (int)EnrollmentStatus.InProcessing;
                        enrollment.EnrollmentDate = DateTime.UtcNow.AddHours(7);
                        enrollment.PercentComplete = 0;
                    }
                    else
                    {
                        paymentTrasactionDTO.Status = (int)TransactionStatus.Error;
                        enrollment.Status = (int)EnrollmentStatus.Canceled;

                        switch (vnpTransactionStatus)
                        {
                            case "01":
                                paymentTrasactionDTO.TransactionError = "Transaction not completed"; //Giao dịch chưa hoàn tất
                                break;
                            case "02":
                                paymentTrasactionDTO.TransactionError = "Transaction error"; //Giao dịch bị lỗi
                                break;
                            case "04":
                                paymentTrasactionDTO.TransactionError = "Irrelevant transaction (Customer has had money deducted at the Bank but the transaction has not been successful at VNPAY)"; //Giao dịch đảo (Khách hàng đã bị trừ tiền tại Ngân hàng nhưng GD chưa thành công ở VNPAY)
                                break;
                            case "05":
                                paymentTrasactionDTO.TransactionError = "VNPAY is processing this transaction (refund transaction)"; //VNPAY đang xử lý giao dịch này (GD hoàn tiền)
                                break;
                            case "06":
                                paymentTrasactionDTO.TransactionError = "VNPAY has sent a refund request to the Bank (Refund transaction)"; //VNPAY đã gửi yêu cầu hoàn tiền sang Ngân hàng (GD hoàn tiền)
                                break;
                            case "07":
                                paymentTrasactionDTO.TransactionError = "Transaction suspected of fraud"; //Giao dịch bị nghi ngờ gian lận
                                break;
                            case "09":
                                paymentTrasactionDTO.TransactionError = "Transaction Refund refused"; //GD Hoàn trả bị từ chối
                                break;
                            case "11":
                                paymentTrasactionDTO.TransactionError = "Payment deadline has expired"; //GD Đã hết hạn chờ thanh toán
                                break;
                            default:
                                paymentTrasactionDTO.TransactionError = "Transaction failed"; //GD không thành công
                                break;
                        }
                    }
                }
                else
                {
                    throw new Exception("Query VNPayTransaction error: " + jObject["vnp_Message"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
            _context.PaymentTransactions.Update(paymentTrasactionDTO);
            _context.Enrollments.Update(enrollment);
            _context.SaveChanges();

            var enrollmentData = _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .ThenInclude(c => c.Mentor)
                //.Include(e => e.PaymentTransactions.First())
                .FirstOrDefault(e => e.Id == enrollment.Id);

            if (enrollmentData != null)
            {
                var usersReceive = _context.Users.Where(u => u.Id == enrollmentData.Course.Mentor.UserId).Select(u => u.Id).ToArray();
                var user = _context.Users.FirstOrDefault(u => u.Id == enrollmentData.UserId);

                if (user != null && enrollmentData.Course != null)
                {
                    notificationRepository.Create(
                    "New Enrollment",
                        $"Great news! A new student {user.FullName} has enrolled in your course {enrollmentData.Course.Name}.",
                        usersReceive
                    );
                }
            }

            var statusText = string.Empty;

            switch (paymentTrasactionDTO.Status)
            {
                case 1:
                    statusText = "Error";
                    break;
                default:
                    statusText = "Success";
                    break;
            }
            #region Send Mail
            var emailContent = $@"
<table style='width: 95%; max-width: 600px; border-collapse: collapse; margin: 20px auto; border: 2px solid rgb(40, 231, 97); border-radius: 10px; overflow: hidden; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); background-color: #f9fdf7;'>
    <tr>
        <th colspan='2' style='border-top-left-radius: 10px; border-top-right-radius: 10px; background-color: green; color: white; text-align: center; font-size: 22px; padding: 20px; border-top: 2px solid #ddd; border-bottom: 2px solid #ddd;'>Payment Transaction Details</th>
    </tr>
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Course Name</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollment.Course.Name}</td>
    </tr>
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Price</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTrasactionDTO.Total}</td>
    </tr>
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Payment Status</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'><span style='color: {(statusText == "Error" ? "#e53935" : (statusText == "Success" ? "#43a047" : "#333"))};'>{statusText}</span></td>
    </tr>
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Order Number</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTrasactionDTO.TransactionId}</td>
    </tr>"; if (!string.IsNullOrEmpty(paymentTrasactionDTO.TransactionError)) { emailContent += $@"
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: red;'>Transaction Error</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTrasactionDTO.TransactionError}</td>
    </tr>"; }
            emailContent += $@"
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Trans. Date, Time</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTrasactionDTO.CreateDate}</td>
    </tr>"; if (paymentTrasactionDTO.SuccessDate != null) { emailContent += $@"
    <tr>
        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: green;'>Success Date</strong></td>
        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTrasactionDTO.SuccessDate}</td>
    </tr>"; }
            emailContent += @"
    <tr>
        <td colspan='2' style='border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; padding: 20px;'>
            <p style='margin: 15px 0; font-size: 16px; color: #555;'>Thank you for your purchase!</p>
        </td>
    </tr>
</table>";
            #endregion

            mailService.SendMail(new List<string> { enrollment.User.Email }, $"Receipt for Your Payment - Course: {enrollment.Course.Name}", emailContent);

            //var courseName = _context.Courses.Find(enrollment.CourseId).Name;
            var course = _context.Courses
                .Include(c => c.Mentor)
                .Where(c => c.Id == enrollment.CourseId).FirstOrDefault();
            return new
            {
                IsSucces = paymentSuccess,
                PaymentTransaction = paymentTrasactionDTO,
                CourseId = enrollment.CourseId,
                CourseName = course.Name,
                EnrollmentDate = enrollment.EnrollmentDate,
                MentorId = course.Mentor.Id
            };
        }

        //PayPal
        private readonly string _clientId = "AeMi9ws-M39AJLFE4G37RuIPv2E6MkdjpvwFOOmVoGaFvoMev7EwVdPZKKtz2t3Naxwf02UF1VEZUPIA";
        private readonly string _secret = "EExqdfogevQfZ8bMImEigTjS_IWSQkvNVIKn7CU8XSDyHb0OsFZuEjHz8vYKzn7qyEzuUcvbunj5DE1z";
        private readonly string _payoutUrl = "https://api.sandbox.paypal.com/v1/payments/payouts";
        private readonly string _authUrl = "https://api-m.sandbox.paypal.com/v1/oauth2/token";

        public async Task<double> GetExchangeRate(string targetCurrency)
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

        public async Task<object> RePay(int transactionId, int enrollmentId)
        {
            var transaction = _context.PaymentTransactions
                .Include(t => t.Enrollment.Course)
                .Where(t => t.Id == transactionId)
                .FirstOrDefault();
            if(transaction == null)
            {
                throw new Exception("Not found transaction");
            }
            transaction.Status = (int)TransactionStatus.Handled;
            var mentorId = transaction.MentorId;
            if (mentorId == null)
            {
                throw new Exception("Not found mentor");
            }
            var amount = transaction.Enrollment.Course.Price;

            var repay = await PayRevenue((int)mentorId, (int)amount, transaction.EnrollmentId);

            //_context.PaymentTransactions.Remove(transaction);

            return repay;
        }

        public async Task<object> PayRevenue(int mentorId, int totalRevenue, int enrollmentId)
        {
            var mentors = _context.Mentors
                .Include(m => m.User)
                .FirstOrDefault(m => m.Id == mentorId);
            try
            {
                double exchangeRate = await GetExchangeRate("USD");

                if (exchangeRate <= 0)
                {
                    throw new Exception("Failed to fetch exchange rate.");
                }

                var amountToPay = totalRevenue / exchangeRate * 0.95;
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
                    throw new Exception("Invalid request. Please provide a valid email and a positive amount.");
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

                    var checkError = _context.PaymentTransactions.Where(p => p.EnrollmentId == enrollmentId && p.Status != (int)TransactionStatus.Success).ToList();
                    var studentEnrollSuccess = _context.PaymentTransactions
                        .Include(p => p.Enrollment)
                        .Where(p => p.EnrollmentId == enrollmentId && p.TransactionType == (int)TransactionTypeStatus.Receive
                        && p.Status == (int)TransactionStatus.Success).ToList();
                    var paymentTransaction = new PaymentTransaction
                    {
                        Total = (int)(totalRevenue * 0.95),
                        TransactionType = (int)TransactionTypeStatus.Pay,
                        CreateDate = DateTime.UtcNow.AddHours(7),
                        Status = (int)TransactionStatus.Pending,
                        MentorId = mentorId,
                        EnrollmentId = enrollmentId
                        //TransactionId = (int)transactionIDPayPal,

                    };
                    if (studentEnrollSuccess.Any())
                    {
                        _context.PaymentTransactions.Add(paymentTransaction);
                        _context.SaveChanges();
                        var response = await httpClient.PostAsync(_payoutUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var paypalError = JObject.Parse(responseContent);

                        var errorDetails = "";

                        var statusCode = (int)response.StatusCode;

                        if (studentEnrollSuccess.Any())
                        {
                            if (checkError.Any())
                            {
                                var lastTransaction = checkError.LastOrDefault();
                                if (statusCode == 201)
                                {
                                    //errorDetails = paypalError["details"]?[0]?["issue"]?.ToString() ?? "Unknown error details";
                                    errorDetails = "Handle for Transaction ID: " + lastTransaction.Id;
                                }
                                else if (statusCode == 400)
                                {
                                    //errorDetails = paypalError["details"]?[0]?["issue"]?.ToString() ?? "Unknown error details";
                                    errorDetails = "Handle for Transaction ID: " + lastTransaction.Id + ". Error: Recipient information does not exist";
                                }
                                else //if(statusCode == 422)
                                {
                                    //errorDetails = paypalError["message"]?.ToString() ?? "Unknown error details";
                                    errorDetails = "Handle for Transaction ID: " + lastTransaction.Id + ". Error: System error when transferring money";
                                }
                            }
                            else
                            {
                                if (statusCode == 201)
                                {
                                    //errorDetails = paypalError["details"]?[0]?["issue"]?.ToString() ?? "Unknown error details";
                                }
                                else if (statusCode == 400)
                                {
                                    //errorDetails = paypalError["details"]?[0]?["issue"]?.ToString() ?? "Unknown error details";
                                    errorDetails = "Recipient information does not exist";
                                }
                                else //if(statusCode == 422)
                                {
                                    //errorDetails = paypalError["message"]?.ToString() ?? "Unknown error details";
                                    errorDetails = "System error when transferring money";
                                }
                            }
                        }





                        if (response.IsSuccessStatusCode)
                        {
                            var transactionIDPayPal = (string)JObject.Parse(responseContent)["batch_header"]["payout_batch_id"];
                            transactionIDPayPal = "MPA-" + transactionIDPayPal;

                            paymentTransaction.Status = (int)TransactionStatus.Success;
                            paymentTransaction.SuccessDate = DateTime.UtcNow.AddHours(7);
                            paymentTransaction.TransactionId = transactionIDPayPal;
                            paymentTransaction.PaymentUrl = "https://www.sandbox.paypal.com/activity/masspay/" + transactionIDPayPal;
                            paymentTransaction.TransactionError = errorDetails;
                        }
                        else
                        {
                            if (statusCode == 400)
                            {
                                paymentTransaction.Status = (int)TransactionStatus.Error;

                                var emailContent = $"<p>Dear {mentors.User.FullName.Split(" ").Reverse().ToArray()[0]},</p><br>";
                                emailContent += "<p>We hope this message finds you well. We regret to inform you that the funds transfer from our account to yours was unsuccessful. Below are some details related to the unsuccessful transaction:</p><br>";
                                emailContent += $"<p>Trans. Date, Time: {DateTime.UtcNow.AddHours(7)}</p>";
                                emailContent += $"<p>Amount: {paymentTransaction.Total}</p>";
                                emailContent += $"<p>Email Address(PayPal): <b>{mentors.PaypalAddress}</b></p>";
                                emailContent += $"<p>PayPal ID: <b>{mentors.PaypalId}</b></p><br>";
                                emailContent += "<p>Please review and provide the correct receiving information for the payment system to reprocess.</p>";
                                emailContent += "<p>Thank you for your prompt attention to this matter.</p><br>";
                                emailContent += "<p><i>Best regards,</i></p>";
                                emailContent += "<p>LearnConnect</p>";

                                mailService.SendMail(new List<string> { mentors.User.Email }, $"Payment transaction failed!", emailContent);

                                if (mentors != null)
                                {
                                    var usersReceive = _context.Users.Where(u => u.Id == mentors.UserId).Select(u => u.Id).ToArray();

                                    if (usersReceive != null)
                                    {
                                        notificationRepository.Create(
                                            "New Notification",
                                            $"Payment transaction failed. Please review and provide the correct receiving information for the payment system to reprocess. Check your email to view details.",
                                            usersReceive
                                        );
                                    }
                                }
                            }
                            else
                            {
                                paymentTransaction.Status = (int)TransactionStatus.Pending;

                                if (paymentTransaction != null)
                                {
                                    var usersReceive = _context.Users.Where(u => u.Role == (int)Roles.Staff).Select(u => u.Id).ToArray();

                                    if (usersReceive != null)
                                    {
                                        notificationRepository.Create(
                                            "New Notification",
                                            $"An error occurred while pay for mentor. Go to payment transaction to check details.",
                                            usersReceive
                                        );
                                    }
                                }
                            }
                            paymentTransaction.TransactionError = errorDetails;
                            paymentTransaction.SuccessDate = DateTime.UtcNow.AddHours(7);


                        }
                        _context.SaveChanges();

                    }
                    return paymentTransaction;

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Internal server error: " + ex.Message);
            }
        }
    }
}
