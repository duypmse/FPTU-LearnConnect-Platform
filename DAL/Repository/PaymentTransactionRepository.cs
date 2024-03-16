using AutoMapper;
using BAL.Models;
using CloudinaryDotNet;
using DAL.DTO;
using DAL.Service;
using Firebase.Auth;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;

namespace DAL.Repository
{
    public interface IPaymentTransactionRepository : IBaseRepository<PaymentTransactionDTO>
    {
        //public string CreateVNPayTransaction(PaymentTransactionDTO paymentTransaction, string ipAddress, string returnUrl);
        //public object GetVNPayTrasaction(int paymentTransactionId, string vnPayTransactionDate);
        public IEnumerable<object> GetByUserId(int userId);
        IEnumerable<object> GetByMentorUserId(int mentorUserId);
        public IEnumerable<object> GetRevenueFilterDate(int userId, DateTime? filterDate);
        public IEnumerable<object> GetRevenueByWeek(int userId);
        public IEnumerable<object> StatisticMentor(int userId, string filterType, string sortBy, string sortOrder);
        public IEnumerable<object> StatisticStaff(string filterType);
        public object StatisticAdmin(string filterType);
        public IEnumerable<object> GetRevenueFilterDateByStaff();
        public IEnumerable<object> GetRevenueFilterDateByStaffByWeek();
        public IEnumerable<object> GetPaymentTransactionFilterDate(DateTime? filterDate, string filterType);
        public IEnumerable<object> GetPaymentTransactionOfMentor(int mentorUserId, DateTime? filterDate);
        public object StatisticRevenueMentor(int mentorUserId, string filterType);


    }
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public PaymentTransactionRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public PaymentTransactionDTO Add(PaymentTransactionDTO _objectDTO)
        {
            var _object = _mapper.Map<PaymentTransaction>(_objectDTO);
            _context.PaymentTransactions.Add(_object);
            return null;
        }

        public PaymentTransactionDTO Get(int id)
        {
            var _object = _context.PaymentTransactions.Find(id);
            var _objectDTO = _mapper.Map<PaymentTransactionDTO>(_object);
            return _objectDTO;
        }

        //Coi lai
        public IEnumerable<object> GetByUserId(int userId)
        {
            var paymentTransactions = _context.PaymentTransactions
                .Include(pt => pt.Enrollment)
                .Where(pt => pt.Enrollment.UserId == userId && pt.MentorId == null && pt.Status == (int)EnrollmentStatus.Pending)
                .ToList();

            if (paymentTransactions.Count != 0)
            {
                foreach (var payment in paymentTransactions)
                {
                    if ((DateTime.UtcNow.AddHours(7) - payment.CreateDate.Value).TotalMinutes > 15)
                    {
                        payment.Enrollment.Status = (int)EnrollmentStatus.Canceled;
                        payment.Status = (int)TransactionStatus.Error;
                        payment.TransactionError = "Payment deadline has expired";

                        _context.SaveChanges();
                    }
                }
            }

            var paymentTransactionsWithCourses = _context.PaymentTransactions
                .Include(pt => pt.Enrollment.Course)
                .Where(pt => pt.Enrollment.UserId == userId && pt.MentorId == null)
                .OrderByDescending(pt => pt.CreateDate)
                .Select(pt =>
                    new 
                    {
                        pt.Id,
                        pt.Total,
                        pt.TransactionId,
                        pt.TransactionError,
                        pt.CreateDate,
                        pt.SuccessDate,
                        pt.PaymentUrl,
                        pt.Status,
                        pt.EnrollmentId,
                        CourseId = pt.Enrollment.Course.Id,
                        CourseName = pt.Enrollment.Course.Name
                    })
                .ToList();

            return paymentTransactionsWithCourses;
        }

        public IEnumerable<object> GetByMentorUserId(int mentorUserId)
        {
            var paymentTransactions = _context.PaymentTransactions
                .Include(pt => pt.Enrollment.Course)
                .Where(pt => pt.Enrollment.Course.MentorId == mentorUserId 
                    && pt.TransactionType == (int)TransactionTypeStatus.Pay || pt.TransactionType == (int)TransactionTypeStatus.SystemCommission)
                .Select(pt =>
                    new
                    {
                        pt.Id,
                        pt.Total,
                        pt.TransactionId,
                        pt.TransactionError,
                        pt.CreateDate,
                        pt.SuccessDate,
                        pt.PaymentUrl,
                        pt.Status,
                        pt.EnrollmentId,
                        CourseId = pt.Enrollment.Course.Id,
                        CourseName = pt.Enrollment.Course.Name
                    })
                .ToList();

            return paymentTransactions;
        }

        public IEnumerable<PaymentTransactionDTO> GetList()
        {
            var _list = _context.PaymentTransactions.ToList();
            var _listDTO = _mapper.Map<List<PaymentTransactionDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, PaymentTransactionDTO _objectDTO)
        {
            var _object = _context.PaymentTransactions.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.PaymentTransactions.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            PaymentTransaction _object = _context.PaymentTransactions.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.PaymentTransactions.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.PaymentTransactions.Any(e => e.Id == id);
            return _isExist;
        }

        public string CreateVNPayTransaction(PaymentTransactionDTO paymentTransaction, string ipAddress, string returnUrl)
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

//        public object GetVNPayTrasaction(int paymentTransactionId, string vnPayTransactionDate)
//        {
//            var vnp_Api = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
//            var vnp_HashSecret = "ADEXUMJMCQXZCJFEIQGIFBTRXUSXRMWF"; //Secret KEy
//            var vnp_TmnCode = "ZKQGUFKT"; // Terminal Id

//            var vnp_RequestId = DateTime.Now.Ticks.ToString(); //Mã hệ thống merchant tự sinh ứng với mỗi yêu cầu truy vấn giao dịch. Mã này là duy nhất dùng để phân biệt các yêu cầu truy vấn giao dịch. Không được trùng lặp trong ngày.
//            var vnp_Version = VnPayLibrary.VERSION; //2.1.0
//            var vnp_Command = "querydr";
//            var vnp_TxnRef = paymentTransactionId; // Mã giao dịch thanh toán tham chiếu
//            var vnp_OrderInfo = "Truy van giao dich:" + paymentTransactionId;
//            var vnp_TransactionDate = vnPayTransactionDate;
//            var vnp_CreateDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
//            var vnp_IpAddr = "::1";

//            var signData = vnp_RequestId + "|" + vnp_Version + "|" + vnp_Command + "|" + vnp_TmnCode + "|" + vnp_TxnRef + "|" + vnp_TransactionDate + "|" + vnp_CreateDate + "|" + vnp_IpAddr + "|" + vnp_OrderInfo;
//            var vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);

//            var qdrData = new
//            {
//                vnp_RequestId = vnp_RequestId,
//                vnp_Version = vnp_Version,
//                vnp_Command = vnp_Command,
//                vnp_TmnCode = vnp_TmnCode,
//                vnp_TxnRef = vnp_TxnRef,
//                vnp_OrderInfo = vnp_OrderInfo,
//                vnp_TransactionDate = vnp_TransactionDate,
//                vnp_CreateDate = vnp_CreateDate,
//                vnp_IpAddr = vnp_IpAddr,
//                vnp_SecureHash = vnp_SecureHash
//            };
//            var jsonData = JsonConvert.SerializeObject(qdrData);

//            var paymentTrasactionDTO = new PaymentTransactionDTO();
//            var enrollment = new Enrollment();
//            var paymentSuccess = false;

//            try
//            {
//                var httpClient = new HttpClient();
//                StringContent httpContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
//                var response = httpClient.PostAsync(vnp_Api, httpContent).Result;
//                var strData = response.Content.ReadAsStringAsync().Result;
//                var jObject = JObject.Parse(strData);

//                var querySuccess = (string)jObject["vnp_ResponseCode"];
//                if (querySuccess.Equals("00"))
//                {   
//                    paymentTrasactionDTO = Get(paymentTransactionId);
//                    enrollment = _context.Enrollments.FirstOrDefault(x => x.PaymentTransactionId == paymentTransactionId);


//                    var vnpTransactionStatus = (string)jObject["vnp_TransactionStatus"];
//                    paymentTrasactionDTO.TransactionId = (string)jObject["vnp_TransactionNo"];

//                    if(vnpTransactionStatus.Equals("00"))
//                    {
//                        paymentSuccess = true;

//                        paymentTrasactionDTO.SuccessDate = DateTime.ParseExact((string)jObject["vnp_PayDate"], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
//                        paymentTrasactionDTO.Status = (int)TransactionStatus.Success;

//                        enrollment.Status = (int)EnrollmentStatus.InProcessing;
//                        enrollment.EnrollmentDate = DateTime.UtcNow.AddHours(7);

//                        //tạo 2 bảng ở đây
                        
//                        var learningPerformanceTmp = new LearningPerformance
//                        {
//                            Score = 0,
//                            TimeSpent = 0,
//                            UserId = enrollment.UserId,
//                            CourseId = enrollment.CourseId,
//                        };
//                        var learningPerformance = _context.LearningPerformances.Add(learningPerformanceTmp).Entity;
//                        SaveChanges();

//                        var learningProcessTmp = new LearningProcess
//                        {
//                            PercentComplete = 0,
//                            Status = 1,
//                            UserId = enrollment.UserId,
//                            CourseId = enrollment.CourseId,
//                        };
//                        var learningProcess = _context.LearningProcesses.Add(learningProcessTmp).Entity;
//                        SaveChanges();

//                        /*//chuyen tien mentor
//                        var moneyPay = enrollment.PaymentTransaction.Total * 0.95;
//                        PayRevenue(enrollment.Course.MentorId, (int)moneyPay);*/
//                    }
//                    else
//                    {
//                        paymentTrasactionDTO.Status = (int)TransactionStatus.Error;
//                        enrollment.Status = (int)EnrollmentStatus.Canceled;

//                        switch (vnpTransactionStatus){
//                            case "01": 
//                                paymentTrasactionDTO.TransactionError = "Transaction not completed"; //Giao dịch chưa hoàn tất
//                                break;
//                            case "02":
//                                paymentTrasactionDTO.TransactionError = "Transaction error"; //Giao dịch bị lỗi
//                                break;
//                            case "04":
//                                paymentTrasactionDTO.TransactionError = "Irrelevant transaction (Customer has had money deducted at the Bank but the transaction has not been successful at VNPAY)"; //Giao dịch đảo (Khách hàng đã bị trừ tiền tại Ngân hàng nhưng GD chưa thành công ở VNPAY)
//                                break;
//                            case "05":
//                                paymentTrasactionDTO.TransactionError = "VNPAY is processing this transaction (refund transaction)"; //VNPAY đang xử lý giao dịch này (GD hoàn tiền)
//                                break;
//                            case "06":
//                                paymentTrasactionDTO.TransactionError = "VNPAY has sent a refund request to the Bank (Refund transaction)"; //VNPAY đã gửi yêu cầu hoàn tiền sang Ngân hàng (GD hoàn tiền)
//                                break;
//                            case "07":
//                                paymentTrasactionDTO.TransactionError = "Transaction suspected of fraud"; //Giao dịch bị nghi ngờ gian lận
//                                break;
//                            case "09":
//                                paymentTrasactionDTO.TransactionError = "Transaction Refund refused"; //GD Hoàn trả bị từ chối
//                                break;
//                            case "11":
//                                paymentTrasactionDTO.TransactionError = "Payment deadline has expired"; //GD Đã hết hạn chờ thanh toán
//                                break;
//                            default:
//                                paymentTrasactionDTO.TransactionError = "Transaction failed"; //GD không thành công
//                                break;
//                        }
//                    }
//                } 
//                else
//                {
//                    throw new Exception("Query VNPayTransaction error: " + jObject["vnp_Message"]);
//                }
//            } 
//            catch(Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                return ex.Message;
//            }
//            Update(paymentTransactionId, paymentTrasactionDTO);
//            _context.Enrollments.Update(enrollment);
//            SaveChanges();

//            var enrollmentData = _context.Enrollments
//                .Include(e => e.User)
//                .Include(e => e.Course)
//                .Include(e => e.PaymentTransaction)
//                .FirstOrDefault(e => e.Id == enrollment.Id);

//            var statusText = string.Empty;

//            switch (enrollmentData.PaymentTransaction.Status)
//            {
//                case 1:
//                    statusText = "Error";
//                    break;
//                default:
//                    statusText = "Success";
//                    break;
//            }

//            var emailContent = $@"
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
//    </tr>
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Order Number</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.TransactionId}</td>
//    </tr>"; if (!string.IsNullOrEmpty(enrollmentData.PaymentTransaction.TransactionError)) { emailContent += $@"
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: red;'>Transaction Error</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.TransactionError}</td>
//    </tr>"; }
//            emailContent += $@"
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Trans. Date, Time</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.CreateDate}</td>
//    </tr>"; if (enrollmentData.PaymentTransaction.SuccessDate != null) { emailContent += $@"
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: green;'>Success Date</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{enrollmentData.PaymentTransaction.SuccessDate}</td>
//    </tr>"; }
//            emailContent += @"
//    <tr>
//        <td colspan='2' style='border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; padding: 20px;'>
//            <p style='margin: 15px 0; font-size: 16px; color: #555;'>Thank you for your purchase!</p>
//        </td>
//    </tr>
//</table>";




//            try
//            {
//                var message = new MailMessage();
//                message.From = new MailAddress("minhduy1511@gmail.com");
//                message.To.Add(enrollment.User.Email);
//                message.Subject = $"Receipt for Your Payment - Course: {enrollment.Course.Name}";
//                message.Body = emailContent;
//                message.IsBodyHtml = true; 

//                var smtpClient = new SmtpClient("smtp.gmail.com", 587);
//                smtpClient.Credentials = new NetworkCredential("minhduy1511@gmail.com", "dxhuwemtdtkobzoj");
//                smtpClient.EnableSsl = true;

//                smtpClient.Send(message);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Failed to send email" + ex.Message);
//            }

//            var courseName = _context.Courses.Find(enrollment.CourseId).Name;
//            return new
//            {
//                IsSucces = paymentSuccess,
//                PaymentTransaction = paymentTrasactionDTO,
//                CourseId = enrollment.CourseId,
//                CourseName = courseName,
//                EnrollmentDate = enrollment.EnrollmentDate
//            };
//        }

        public IEnumerable<object> GetRevenueFilterDate(int userId, DateTime? filterDate)
        {
            var mentor = _context.Mentors.FirstOrDefault(m => m.UserId == userId);

            DateTime startDate;

            if (filterDate == null)
            {
                startDate = DateTime.Now.AddDays(-30);
            }
            else
            {
                startDate = filterDate.Value.Date;
            }

            if (mentor != null)
            {
                var mentorId = mentor.Id;
                var dataCoursePayment = _context.Courses
                    .Where(c => c.Status == (int)CourseStatus.Active && c.MentorId == mentorId
                        && c.Enrollments.Any(enrollment => (filterDate == null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date >= startDate) ||
                                                           (filterDate != null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == startDate)))
                    .Select(course => new
                    {
                        CourseName = course.Name,
                        CourseImage = course.ImageUrl,
                        TotalRevenueCourse = (course.Enrollments
                            .Where(enrollment => (filterDate == null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date >= startDate) ||
                                                 (filterDate != null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == startDate)
                                && enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success)
                            .Sum(enrollment => enrollment.PaymentTransactions.First().Total)) * 0.95,
                        TotalEnroll = course.Enrollments.Count(enrollment => (filterDate == null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date >= startDate) ||
                                                                             (filterDate != null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == startDate)),
                        UsersEnroll = course.Enrollments
                            .Where(enrollment => (filterDate == null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date >= startDate) ||
                                                 (filterDate != null && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == startDate)
                                && enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success)
                            .Select(enrollment => new
                            {
                                UserId = enrollment.UserId,
                                UserName = enrollment.User.FullName,
                                UserImage = enrollment.User.ProfilePictureUrl,
                                EnrollmentDate = enrollment.PaymentTransactions.First().SuccessDate
                            }),
                    })
                    .ToList();

                var resultList = new List<object>
        {
            new
            {
                Date = startDate,
                DayOfWeek = startDate.DayOfWeek.ToString(),
                RevenueDate = (dataCoursePayment.Sum(item => item.TotalRevenueCourse)),
                RevenueCourse = dataCoursePayment
            }
        };

                return resultList;
            }
            else
            {
                return Enumerable.Empty<object>();
            }
        }


        public IEnumerable<object> GetPaymentTransactionFilterDate(DateTime? filterDate, string filterType)
        {
            DateTime startDate;

            if (filterDate == null)
            {
                startDate = DateTime.Now.AddDays(-30);
            }
            else
            {
                startDate = filterDate.Value.Date;
            }

            if (filterType == "revenue")
            {
                var dataTransactionRevenueByDate = _context.Enrollments
                    .Include(enroll => enroll.User)
                    .Include(enroll => enroll.PaymentTransactions)
                    .Include(enroll => enroll.Course)
                    .Where(enroll =>
                        (filterDate == null && enroll.PaymentTransactions.First().CreateDate.Value.Date >= startDate && enroll.PaymentTransactions.First().MentorId == null) ||
                        (filterDate != null && enroll.PaymentTransactions.First().CreateDate.Value.Date == startDate && enroll.PaymentTransactions.First().MentorId == null))
                    .OrderByDescending(enroll => enroll.PaymentTransactions.First().CreateDate)
                    .Select(data => new
                    {
                        PaymentTransactionId = data.PaymentTransactions.First().Id,
                        UserBuy = data.User.FullName,
                        UserIdBuy = data.User.Id,
                        CourseName = data.Course.Name,
                        CourseId = data.Course.Id,
                        Price = data.PaymentTransactions.First().Total,
                        TransactionId = data.PaymentTransactions.First().TransactionId,
                        CreateDate = data.PaymentTransactions.First().CreateDate,
                        SuccessDate = data.PaymentTransactions.First().SuccessDate,
                        PaymentUrl = data.PaymentTransactions.First().PaymentUrl,
                        TransactionError = data.PaymentTransactions.First().TransactionError,
                        Status = data.PaymentTransactions.First().Status,
                        EnrollmentId = data.PaymentTransactions.First().EnrollmentId,
                    }).ToList();

                return dataTransactionRevenueByDate;
            }
            else if (filterType == "pay")
            {
                var dataTransactionPayByDate = _context.PaymentTransactions
                    .Include(payment => payment.Mentor.User)
                    .Include(payment => payment.Enrollment)
                    .Where(payment =>
                        (filterDate == null && payment.CreateDate.Value.Date >= startDate && payment.MentorId != null) ||
                        (filterDate != null && payment.CreateDate.Value.Date == startDate && payment.MentorId != null))
                    .OrderByDescending(payment => payment.CreateDate)
                    .Select(data => new
                    {
                        PaymentTransactionId = data.Id,
                        MentorPay = data.Mentor.User.FullName,
                        MentorIdPay = data.Mentor.User.Id,
                        CourseName = data.Enrollment.Course.Name,
                        CourseId = data.Enrollment.Course.Id,
                        CoursePrice = data.Enrollment.Course.Price,
                        PlatformFee = (double)data.Enrollment.Course.Price * 0.05,
                        Amount = data.Total,
                        TransactionId = data.TransactionId,
                        CreateDate = data.CreateDate,
                        SuccessDate = data.SuccessDate,
                        TransactionError = data.TransactionError,
                        PaymentUrl = data.PaymentUrl,
                        Status = data.Status,
                        EnrollmentId = data.EnrollmentId
                    }).ToList();

                return dataTransactionPayByDate;
            }
            else if (filterType == "profit")
            {
                var dataTransactionPayByDate = _context.PaymentTransactions
                    .Include(payment => payment.Mentor.User)
                    .Include(payment => payment.Enrollment)
                    .Where(payment =>
                        (filterDate == null && payment.CreateDate.Value.Date >= startDate && payment.MentorId != null) ||
                        (filterDate != null && payment.CreateDate.Value.Date == startDate && payment.MentorId != null))
                    .OrderByDescending(payment => payment.CreateDate)
                    .Select(data => new
                    {
                        CourseName = data.Enrollment.Course.Name,
                        UserEnroll = data.Enrollment.User.FullName,
                        MentorPay = data.Mentor.User.FullName,
                        Revenue = data.Enrollment.Course.Price,
                        PayForMentor = data.Total,
                        Profit = (double)data.Enrollment.Course.Price * 0.05,
                        Date = data.CreateDate,
                        EnrollmentId = data.EnrollmentId
                    }).ToList();

                return dataTransactionPayByDate;
            }
            else
            {
                throw new ArgumentException("Invalid filterType");
            }
        }

        public IEnumerable<object> GetPaymentTransactionOfMentor(int mentorUserId, DateTime? filterDate)
        {
            var startDate = (filterDate == null) ? DateTime.Now.AddDays(-30) : filterDate.Value.Date;

            var dataTransactionPayByDate = _context.PaymentTransactions
                    .Include(payment => payment.Enrollment.Course)
                    .Include(payment => payment.Enrollment.User)
                    .Where(payment =>
                        (filterDate == null && payment.CreateDate.Value.Date >= startDate && payment.Mentor.UserId == mentorUserId) ||
                        (filterDate != null && payment.CreateDate.Value.Date == startDate && payment.Mentor.UserId == mentorUserId))
                    .OrderByDescending(payment => payment.CreateDate)
                    .Select(data => new
                    {
                        PaymentTransactionId = data.Id,
                        EnrollmentId = data.Enrollment.Id,
                        CourseName = data.Enrollment.Course.Name,
                        CourseId = data.Enrollment.Course.Id,
                        UserEnrroll = data.Enrollment.User.FullName,
                        UserIdEnrroll = data.Enrollment.User.Id,
                        CoursePrice = data.Enrollment.Course.Price,
                        PlatformFee = (double)data.Enrollment.Course.Price * 0.05,
                        Revenue = data.Total,
                        TransactionId = data.TransactionId,
                        CreateDate = data.CreateDate,
                        SuccessDate = data.SuccessDate,
                        PaymentUrl = data.PaymentUrl,
                        TransactionError = data.TransactionError,
                        Status = data.Status
                    }).ToList();

            return dataTransactionPayByDate;

        }





        public IEnumerable<object> GetRevenueByWeek(int userId)
        {
            var mentor = _context.Mentors.FirstOrDefault(m => m.UserId == userId);
            var filterDate = DateTime.UtcNow.AddHours(7);

            if (mentor != null)
            {
                var mentorId = mentor.Id;

                // Collect revenue data for the past 30 days
                var resultList = new List<object>();
                for (int i = 0; i < 7; i++)
                {
                    var currentDate = filterDate.AddDays(-i);
                    var dataCoursePayment = _context.Courses
                        .Where(c => c.Status == (int)CourseStatus.Active && c.MentorId == mentorId
                            && c.Enrollments.Any(enrollment => enrollment.PaymentTransactions.First().SuccessDate.Value.Date == currentDate.Date))
                        .Select(course => new
                        {
                            CourseName = course.Name,
                            TotalEnroll = course.Enrollments.Count(enrollment => enrollment.PaymentTransactions.First().SuccessDate.Value.Date == currentDate.Date),
                            TotalRevenueCourse = course.Enrollments
                                .Where(enrollment => enrollment.PaymentTransactions.First().SuccessDate.Value.Date == currentDate.Date
                                    && enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success)
                                .Sum(enrollment => enrollment.PaymentTransactions.First().Total),
                        })
                        .ToList();

                    var totalRevenueDate = dataCoursePayment.Sum(item => item.TotalRevenueCourse);

                    resultList.Add(new
                    {
                        Date = currentDate.Date,
                        DayOfWeek = currentDate.DayOfWeek.ToString(),
                        RevenueDate = totalRevenueDate * 0.95,
                        RevenueCourse = dataCoursePayment
                    });
                }

                return resultList;
            }
            else
            {
                return Enumerable.Empty<object>();
            }
        }

        public IEnumerable<object> StatisticMentor(int userId, string filterType, string sortBy, string sortOrder)
        {
            var mentor = _context.Mentors.FirstOrDefault(m => m.UserId == userId);
            var filterDate = DateTime.UtcNow.AddHours(7);

            if (mentor != null)
            {
                var mentorId = mentor.Id;

                var resultList = new List<object>();
                int daysToSubtract = 0;

                switch (filterType.ToLower())
                {
                    case "month":
                        daysToSubtract = DateTime.DaysInMonth(filterDate.Year, filterDate.Month);
                        break;

                    case "week":
                        daysToSubtract = 7;
                        break;

                    case "day":
                        daysToSubtract = 1;
                        break;

                    default:
                        daysToSubtract = 1;
                        break;
                }

                var dataCoursePayment = _context.Courses
                    .Where(c => c.MentorId == mentorId && c.Status == (int)CourseStatus.Active)
                    .Include(c => c.Enrollments)
                    .ThenInclude(e => e.PaymentTransactions)
                    .Select(course => new
                    {
                        CourseName = course.Name,
                        Enrollments = course.Enrollments.FirstOrDefault().PaymentTransactions
                            .Where(enrollment => enrollment.SuccessDate >= filterDate.AddDays(-daysToSubtract)),
                        Revenue = course.Enrollments.FirstOrDefault().PaymentTransactions
                            .Where(enrollment => enrollment.SuccessDate >= filterDate.AddDays(-daysToSubtract)
                                && enrollment.Status == (int)TransactionStatus.Success)
                            .Sum(enrollment => enrollment.Total),
                    })
                    .ToList()
                    .Select(courseData => new
                    {
                        CourseName = courseData.CourseName,
                        TotalEnroll = courseData.Enrollments.Count(),
                        Revenue = courseData.Revenue * 0.95,
                    })
                    .GroupBy(courseData => courseData.CourseName)
                    .Select(groupedData => new
                    {
                        CourseName = groupedData.Key,
                        TotalEnroll = groupedData.Sum(courseData => courseData.TotalEnroll),
                        Revenue = groupedData.Sum(courseData => courseData.Revenue),
                    })
                    .ToList();

                var allCourses = _context.Courses.Where(c => c.MentorId == mentorId && c.Status == (int)CourseStatus.Active).Select(c => c.Name).ToList();

                var missingCourses = allCourses.Except(dataCoursePayment.Select(c => c.CourseName));

                dataCoursePayment.AddRange(missingCourses.Select(courseName => new
                {
                    CourseName = courseName,
                    TotalEnroll = 0,
                    Revenue = 0.0,
                }));

                var totalRevenue = _context.Courses
                            .Where(c => c.Status == (int)CourseStatus.Active && c.MentorId == mentorId)
                            .SelectMany(course => course.Enrollments
                                .Where(enrollment => enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success)
                                .Select(enrollment => enrollment.PaymentTransactions.First().Total))
                            .Sum() * 0.95;

                var totalActiveCourses = _context.Courses
                    .Count(c => c.Status == (int)CourseStatus.Active && c.MentorId == mentorId);

                var totalEnrollments = _context.Enrollments
                    .Count(enrollment => enrollment.Course.MentorId == mentorId);

                if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
                {
                    switch (sortBy.ToLower())
                    {
                        case "totalenroll":
                            dataCoursePayment = sortOrder.ToLower() == "asc"
                                ? dataCoursePayment.OrderBy(courseData => courseData.TotalEnroll).ToList()
                                : dataCoursePayment.OrderByDescending(courseData => courseData.TotalEnroll).ToList();
                            break;

                        case "revenue":
                            dataCoursePayment = sortOrder.ToLower() == "asc"
                                ? dataCoursePayment.OrderBy(courseData => courseData.Revenue).ToList()
                                : dataCoursePayment.OrderByDescending(courseData => courseData.Revenue).ToList();
                            break;
                    }
                }

                var result = new
                {
                    TotalStatisticInfo = new
                    {
                        TotalRevenue = totalRevenue,
                        TotalActiveCourses = totalActiveCourses,
                        TotalEnrollments = totalEnrollments
                    },
                    CourseStatistics = dataCoursePayment
                };

                return new List<object> { result };
            }
            else
            {
                return Enumerable.Empty<object>();
            }
        }

        public IEnumerable<object> StatisticStaff(string filterType)
        {
            var filterDate = DateTime.UtcNow.AddHours(7);

            var resultList = new List<object>();
            int daysToSubtract = 0;

            switch (filterType.ToLower())
            {
                case "month":
                    daysToSubtract = 29;
                    break;

                case "week":
                    daysToSubtract = 6;
                    break;

                case "day":
                    daysToSubtract = 0;
                    break;

                default:
                    daysToSubtract = 1;
                    break;
            }

            var startDate = filterDate.AddDays(-daysToSubtract);

            var dailyStatistics = Enumerable.Range(0, daysToSubtract + 1)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => new
                {
                    Date = date,
                    NewCoursesFee = _context.Courses
                        .Count(c => c.Status == (int)CourseStatus.Active && c.CreateDate.Date == date.Date && c.Price > 0),
                    NewCoursesFree = _context.Courses
                        .Count(c => c.Status == (int)CourseStatus.Active && c.CreateDate.Date == date.Date && c.Price == 0),
                    NewMentors = _context.SpecializationOfMentors
                        .Where(specOfMentor => specOfMentor.Status == (int)SpecializationOfMentorStatus.Approve && specOfMentor.VerificationDate.Value.Date == date.Date)
                        .Select(specOfMentor => specOfMentor.MentorId)
                        .Distinct()
                        .Count(),
                    NewEnrollments = _context.Enrollments
                        .Count(enrollment => enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == date.Date),
                    NewRevenue = _context.Courses
                        .Where(c => c.Status == (int)CourseStatus.Active)
                        .SelectMany(course => course.Enrollments
                            .Where(enrollment => enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == date.Date)
                            .Select(enrollment => enrollment.PaymentTransactions.First().Total))
                        .Sum(),
                })
                .ToList();

            var totalRevenue = _context.PaymentTransactions
                        .Where(payment => payment.Status == (int)TransactionStatus.Success)
                        .Select(payment => payment.Total)
                        .ToList()
                        .Sum();

            var totalActiveCourses = _context.Courses
                .Count(c => c.Status == (int)CourseStatus.Active);

            var totalEnrollments = _context.Enrollments
                .Count(enrollment => enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success);

            var totalMentor = _context.Mentors
                .Count(mentor => mentor.Status == (int)MentorStatus.Active);

            var result = new
            {
                TotalStatisticInfo = new
                {
                    TotalActiveCourses = totalActiveCourses,
                    TotalMentors = totalMentor,
                    TotalEnrollments = totalEnrollments,
                    TotalRevenue = totalRevenue,
                },
                TotalCourseStatusStatistic = new
                {
                    CoursesActive = _context.Courses
                        .Count(c => c.Status == (int)CourseStatus.Active),
                    CoursesPending = _context.Courses
                        .Count(c => c.Status == (int)CourseStatus.Pending),
                    CoursesReject = _context.Courses
                        .Count(c => c.Status == (int)CourseStatus.Reject),
                    CoursesBan = _context.Courses
                        .Count(c => c.Status == (int)CourseStatus.Banned),
                },
                StatisticsByDate = dailyStatistics,
            };

            return new List<object> { result };
        }

        public object StatisticRevenueMentor(int mentorUserId, string filterType)
        {
            var filterDate = DateTime.UtcNow.AddHours(7);

            //var resultList = new List<object>();
            int daysToSubtract = 0;

            switch (filterType.ToLower())
            {
                case "month":
                    daysToSubtract = 29;
                    break;

                case "week":
                    daysToSubtract = 6;
                    break;

                case "day":
                    daysToSubtract = 0;
                    break;

                default:
                    daysToSubtract = 1;
                    break;
            }

            var startDate = filterDate.AddDays(-daysToSubtract);

            var dailyStatistics = Enumerable.Range(0, daysToSubtract + 1)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => new
                {
                    Date = date,
                    NewEnrollments = _context.Enrollments
                        .Count(enrollment => enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success 
                        && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == date.Date
                        && enrollment.Course.Mentor.UserId == mentorUserId),
                    NewRevenue = _context.Courses
                        .Where(c => c.Status == (int)CourseStatus.Active && c.Mentor.UserId == mentorUserId)
                        .SelectMany(course => course.Enrollments
                            .Where(enrollment => enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success && enrollment.PaymentTransactions.First().SuccessDate.Value.Date == date.Date)
                            .Select(enrollment => (enrollment.PaymentTransactions.First().Total) * 0.95))
                        .Sum(),
                })
                .ToList();

            var result = new
            {
                StatisticsByDate = dailyStatistics,
            };

            return result;
        }

        public object StatisticAdmin(string filterType)
        {
            var filterDate = DateTime.UtcNow.AddHours(7);

            int daysToSubtract = 0;

            switch (filterType.ToLower())
            {
                case "month":
                    daysToSubtract = 29;
                    break;

                case "week":
                    daysToSubtract = 6;
                    break;

                case "day":
                    daysToSubtract = 0;
                    break;

                default:
                    daysToSubtract = 1;
                    break;
            }

            var startDate = filterDate.AddDays(-daysToSubtract);

            var dailyStatistics = Enumerable.Range(0, daysToSubtract + 1)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => new
                {
                    Date = date,
                    NewUser = _context.Users
                        .Count(u => u.Status == (int)UserStatus.Active && u.RegistrationDate.Date == date.Date),
                })
                .ToList();

            var result = new
            {
                StatisticsByDate = dailyStatistics,
            };

            return result;
        }

        public IEnumerable<object> GetRevenueFilterDateByStaff()
        {
            DateTime filterDate = DateTime.Today;

            var mentors = _context.Mentors
                .Include(m => m.User)
                .ToList();

            var resultList = new List<object>();

            foreach (var mentor in mentors)
            {
                var mentorId = mentor.Id;
                var dataCoursePayment = _context.Courses
                    .Where(c => c.Status == (int)CourseStatus.Active && c.MentorId == mentorId
                        && c.Enrollments.Any(enrollment => enrollment.PaymentTransactions.First().SuccessDate.Value.Date == filterDate.Date))
                    .Select(course => new
                    {
                        TotalRevenueCourse = course.Enrollments
                            .Where(enrollment => enrollment.PaymentTransactions.First().SuccessDate.Value.Date == filterDate.Date
                                && enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success)
                            .Sum(enrollment => enrollment.PaymentTransactions.First().Total),
                    })
                    .ToList();

                var totalRevenue = dataCoursePayment.Sum(item => item.TotalRevenueCourse);

                var mentorPaid = _context.PaymentTransactions
                                .FirstOrDefault(p => p.MentorId == mentorId 
                                && p.Status == (int)TransactionStatus.Success 
                                && p.SuccessDate.Value.Date == filterDate);

                bool isPaid = mentorPaid != null;

                if (totalRevenue != 0)
                {
                    resultList.Add(new
                    {
                        Date = filterDate.Date,
                        DayOfWeek = filterDate.DayOfWeek.ToString(),
                        MentorUserId = mentor.UserId,
                        MentorName = mentor.User.FullName,
                        Bank = mentor.PaypalAddress,
                        AccountNumber = mentor.PaypalId,
                        Revenue = totalRevenue,
                        AmountToPay = totalRevenue * 0.95,
                        IsPaid = isPaid
                    });
                }
            }

            return resultList;
        }

        public IEnumerable<object> GetRevenueFilterDateByStaffByWeek()
        {
            var resultList = new List<object>();

            DateTime filterDate = DateTime.Today;

            var mentors = _context.Mentors
                .Include(m => m.User)
                .ToList();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = filterDate.AddDays(-i);
                var totalRevenueDate = 0.0;

                foreach (var mentor in mentors)
                {
                    var mentorId = mentor.Id;

                    var dataCoursePayment = _context.Courses
                        .Where(c => c.Status == (int)CourseStatus.Active && c.MentorId == mentorId
                            && c.Enrollments.Any(enrollment => enrollment.PaymentTransactions.First().SuccessDate.Value.Date == currentDate.Date))
                        .Select(course => course.Enrollments
                            .Where(enrollment => enrollment.PaymentTransactions.First().SuccessDate.Value.Date == currentDate.Date
                                && enrollment.PaymentTransactions.First().Status == (int)TransactionStatus.Success)
                            .Sum(enrollment => enrollment.PaymentTransactions.First().Total))
                        .ToList();

                    totalRevenueDate += dataCoursePayment.Sum();
                }

                resultList.Add(new
                {
                    Date = currentDate.Date,
                    TotalRevenue = totalRevenueDate
                });
            }

            return resultList;
        }

    }
}
