using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Timers;

namespace DAL.Repository
{
    public interface IEnrollmentRepository : IBaseRepository<EnrollmentDTO>
    {
        //public string EnrollCourse(int userId, int courseId, string returnUrl);
        public void SaveProcess(int userId, int lectureId, int courseId, int currentTime, int maxTime, int totalTime, int? timeSpent);
        public LearningProcessDetailDTO GetProcessByLectureId(int lectureId, int courseId, int userId);
        public object GetCurrentLecture(int courseId, int userId);
        public object StatisticsLearningProcess(int userId);
        public object GetLearnCourseResult(int courseId, int userId);
    }
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;
        private readonly IPaymentTransactionRepository paymentTrasactionRepo;

        public EnrollmentRepository(LearnConnectDBContext context, IMapper mapper, IPaymentTransactionRepository paymentTrasactionRepo)
        {
            _context = context;
            _mapper = mapper;
            this.paymentTrasactionRepo = paymentTrasactionRepo;
        }
        public EnrollmentDTO Add(EnrollmentDTO _objectDTO)
        {
            var _object = _mapper.Map<Enrollment>(_objectDTO);
            _context.Enrollments.Add(_object);
            return null;
        }

        public EnrollmentDTO Get(int id)
        {
            var _object = _context.Enrollments.Find(id);
            var _objectDTO = _mapper.Map<EnrollmentDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<EnrollmentDTO> GetList()
        {
            var _list = _context.Enrollments.ToList();
            var _listDTO = _mapper.Map<List<EnrollmentDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, EnrollmentDTO _objectDTO)
        {
            var _object = _context.Enrollments.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.Enrollments.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            Enrollment _object = _context.Enrollments.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Enrollments.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Enrollments.Any(e => e.Id == id);
            return _isExist;
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
//                    var paymentTransaction = _context.PaymentTransactions.First(p => p.EnrollmentId == enrollmentTmp.Id);
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
//            var course = _context.Courses.Find(courseId);

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

//                _context.Enrollments.Add(enrollment);
//                SaveChanges();

//                var paymentTransactionTmp = new PaymentTransaction
//                {
//                    Total = 0, 
//                    CreateDate = DateTime.UtcNow.AddHours(7),
//                    SuccessDate= DateTime.UtcNow.AddHours(7),
//                    Status = (int)TransactionStatus.Success, 
//                    EnrollmentId = enrollment.Id
//                };
//                var paymentTransaction = _context.PaymentTransactions.Add(paymentTransactionTmp).Entity;
//                SaveChanges();

//                //var learningPerformanceTmp = new LearningPerformance
//                //{
//                //    Score = 0,
//                //    TimeSpent = 0,
//                //    UserId = userId,
//                //    CourseId = courseId,
//                //};
//                //_context.LearningPerformances.Add(learningPerformanceTmp);
//                //SaveChanges();

//                var statusText = string.Empty;

//                switch (paymentTransaction.Status)
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
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.Total}</td>
//    </tr>
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Payment Status</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'><span style='color: {(statusText == "Error" ? "#e53935" : (statusText == "Success" ? "#43a047" : "#333"))};'>{statusText}</span></td>
//    </tr>"; if (!string.IsNullOrEmpty(paymentTransaction.TransactionError)) { emailContent += $@"
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: red;'>Transaction Error</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.TransactionError}</td>
//    </tr>"; }
//                emailContent += $@"
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong>Trans. Date, Time</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.CreateDate}</td>
//    </tr>"; if (paymentTransaction.SuccessDate != null) { emailContent += $@"
//    <tr>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd;'><strong style='color: green;'>Success Date</strong></td>
//        <td style='padding: 20px; border-bottom: 2px solid #ddd; border-left: 2px solid #ddd;'>{paymentTransaction.SuccessDate}</td>
//    </tr>"; }
//                emailContent += @"
//    <tr>
//        <td colspan='2' style='border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; padding: 20px;'>
//            <p style='margin: 15px 0; font-size: 16px; color: #555;'>Thank you for your purchase!</p>
//        </td>
//    </tr>
//</table>";
//                try
//                {
//                    var message = new MailMessage();
//                    message.From = new MailAddress("minhduy1511@gmail.com");
//                    message.To.Add(enrollment.User.Email);
//                    message.Subject = $"Receipt for Your Payment - Course: {enrollment.Course.Name}";
//                    message.Body = emailContent;
//                    message.IsBodyHtml = true;

//                    var smtpClient = new SmtpClient("smtp.gmail.com", 587);
//                    smtpClient.Credentials = new NetworkCredential("minhduy1511@gmail.com", "dxhuwemtdtkobzoj");
//                    smtpClient.EnableSsl = true;

//                    smtpClient.Send(message);
//                }
//                catch (Exception ex)
//                {
//                    throw new Exception("Failed to send email" + ex.Message);
//                }
//            }
//            if (!enrollment.IsFree)
//            {
//                _context.Enrollments.Add(enrollment);
//                SaveChanges();

//                var paymentTransactionTmp = new PaymentTransaction
//                {
//                    Total = (int)course.Price,
//                    CreateDate = DateTime.UtcNow.AddHours(7),
//                    Status= (int)TransactionStatus.Pending,
//                    EnrollmentId = enrollment.Id,
//                };
//                var paymentTransaction = _context.PaymentTransactions.Add(paymentTransactionTmp).Entity;
//                SaveChanges();

//                var paymentTransactionDTO = _mapper.Map<PaymentTransactionDTO>(paymentTransaction);
//                vnPayUrl = paymentTrasactionRepo.CreateVNPayTransaction(paymentTransactionDTO, "::1", returnUrl);

//                paymentTransaction.PaymentUrl= vnPayUrl;
//                SaveChanges();

//            }

//            var totalEnroll = _context.Enrollments
//                    .Where(e => e.CourseId == courseId && (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed)).ToList().Count;

//            var courseEnroll = _context.Courses.Find(courseId);
//            courseEnroll.TotalEnrollment = totalEnroll + 1;

//            SaveChanges();
//            return vnPayUrl;
//        }








        public void SaveProcess(int userId, int lectureId, int courseId, int currentTime, int maxTime, int totalTime, int? timeSpent)
        {
            var learningProcess = _context.Enrollments.FirstOrDefault(l => l.UserId == userId && l.CourseId == courseId);
            var learningProcessDetail = _context.LearningProcessDetails.FirstOrDefault(ld => ld.EnrollmentId == learningProcess.Id && ld.LectureId == lectureId);
            if (learningProcessDetail == null)
            {
                learningProcessDetail = _context.LearningProcessDetails.Add(new LearningProcessDetail
                {
                    CurrentStudyTime = currentTime,
                    MaxStudyTime = maxTime,
                    TotalTime = totalTime,
                    TimeSpent = 1,
                    EnrollmentId = learningProcess.Id,
                    LectureId = lectureId,
                    Status = (int)LearningProcessDetailStatus.InProcessing
                }).Entity;
                SaveChanges();
            }
            else
            {
                learningProcessDetail.CurrentStudyTime = currentTime;
                learningProcessDetail.TotalTime = (totalTime < maxTime) ? maxTime : totalTime;
                if (learningProcessDetail.MaxStudyTime < maxTime)
                {
                    learningProcessDetail.MaxStudyTime = maxTime;
                }
            }
            if (timeSpent.HasValue)
            {
                learningProcessDetail.TimeSpent = learningProcessDetail.TimeSpent + timeSpent;
            } else
            {
                learningProcessDetail.TimeSpent = learningProcessDetail.TimeSpent + 1;
            }

            SaveChanges();
            var enroll = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId);
            var testsResults = _context.TestResults
                .Include(ts => ts.Test)
                .Where(l => l.UserId == userId && l.Test.CourseId == courseId)
                .ToList();
            var lectures = _context.LearningProcessDetails.Where(l => l.EnrollmentId == enroll.Id);
            var testTimeSpent = testsResults
                .Where(result => result.TimeSpent.HasValue)
                .Sum(result => result.TimeSpent.Value);
            var lectureTimeSpent =
                lectures
                .Where(result => result.TimeSpent.HasValue)
                .Sum(result => result.TimeSpent.Value);

            enroll.TimeSpent = testTimeSpent + lectureTimeSpent;
            enroll.TotalScore = testsResults.Where(result => result.Score.HasValue).Sum(result => result.Score.Value);

            if (maxTime > totalTime * 0.9)
            {
                learningProcessDetail.Status = (int)LearningProcessDetailStatus.Completed;
            }
            var lectureCount = _context.Lectures.Where(l => l.CourseId == courseId).Count();
            var percentPerLecture = (100 / (decimal)lectureCount);
            var learned = _context.LearningProcessDetails.Where(ld => ld.EnrollmentId == learningProcess.Id && ld.Status == 0).Count();
            var learning = _context.LearningProcessDetails.FirstOrDefault(ld => ld.EnrollmentId == learningProcess.Id && ld.Status == 1);

            if (learning != null && learning.LectureId == lectureId)
            {
                var percentComplete = learned * percentPerLecture + ((decimal)learningProcessDetail.MaxStudyTime / totalTime) * percentPerLecture * 1;
                learningProcess.PercentComplete = (decimal)percentComplete;
            }

            if (learningProcess.PercentComplete == 100)
            {
                learningProcess.Status = (int)LearningProcessStatus.Completed;
                _context.Enrollments.FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId && e.Status == (int)EnrollmentStatus.InProcessing).Status = (int)LearningProcessStatus.Completed;
            }

            SaveChanges();
        }

        public LearningProcessDetailDTO GetProcessByLectureId(int lectureId, int courseId, int userId)
        {
            var learningProcess = _context.Enrollments.FirstOrDefault(l => l.UserId == userId && l.CourseId == courseId);
            var learningProcessDetail = _context.LearningProcessDetails.FirstOrDefault(ld => ld.LectureId == lectureId && ld.EnrollmentId == learningProcess.Id);
            if (learningProcessDetail == null)
            {
                learningProcessDetail = _context.LearningProcessDetails.Add(new LearningProcessDetail
                {
                    CurrentStudyTime = 0,
                    MaxStudyTime = 0,
                    TotalTime = 0,
                    TimeSpent = 1,
                    EnrollmentId = learningProcess.Id,
                    LectureId = lectureId,
                    Status = (int)LearningProcessDetailStatus.InProcessing
                }).Entity;
                SaveChanges();
            }
            var learningProcessDetailDTO = _mapper.Map<LearningProcessDetailDTO>(learningProcessDetail);
            return learningProcessDetailDTO;
        }

        public object GetCurrentLecture(int courseId, int userId)
        {
            var learningProcess = _context.Enrollments.FirstOrDefault(l => l.UserId == userId && l.CourseId == courseId);
            var lectures = _context.LearningProcessDetails.Where(ld => ld.EnrollmentId == learningProcess.Id);
            var lectureLearned = lectures.Count();
            var currentLecture = lectures.FirstOrDefault(l => l.Status == (int)LearningProcessDetailStatus.InProcessing);
            if (currentLecture == null)
            {
                return new
                {
                    LectureLearned = lectureLearned + 1,
                    Progress = 0
                };
            }
            if (currentLecture.TotalTime == 0)
            {
                return new
                {
                    LectureLearned = lectureLearned,
                    Progress = 0
                };
            }
            var currentLectureProgressPersent = (decimal)currentLecture.MaxStudyTime / currentLecture.TotalTime * 100;

            return new
            {
                LectureLearned = lectureLearned,
                Progress = currentLectureProgressPersent
            };
        }

        public object StatisticsLearningProcess(int userId)
        {

            var enrollmentsToUpdate = _context.Enrollments
                .Where(e => e.UserId == userId && (e.Status == (int)EnrollmentStatus.InProcessing)).ToList();

            foreach (var enrollment in enrollmentsToUpdate)
            {
                var percentComplete = _context.Enrollments
                    .FirstOrDefault(l => l.CourseId == enrollment.CourseId && l.UserId == userId)?.PercentComplete;

                if (percentComplete == 100 && enrollment.Status != (int)EnrollmentStatus.Completed)
                {
                    enrollment.Status = (int)EnrollmentStatus.Completed;
                }
            }

            _context.SaveChanges();

            var dataCourse = _context.Courses
                .Include(c => c.Enrollments)
                .Where(c => c.Enrollments.Any(e => e.UserId == userId
                    && (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed)))
                .Select(course => new
                {
                    CourseName = course.Name,
                    CourseImage = course.ImageUrl,
                    Completion = _context.Enrollments.FirstOrDefault(l => l.CourseId == course.Id && l.UserId == userId).PercentComplete,
                    Grade = _context.Enrollments.FirstOrDefault(l => l.CourseId == course.Id && l.UserId == userId).TotalScore,
                    StartDate = _context.Enrollments
                                .FirstOrDefault(e => (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed) && e.CourseId == course.Id && e.UserId == userId)
                                .PaymentTransactions.FirstOrDefault().SuccessDate,
                    EndDate = _context.Enrollments
                                .Where(e => e.Status == (int)EnrollmentStatus.Completed && e.CourseId == course.Id && e.UserId == userId)
                                .Select(e => e.EnrollmentDate)
                                .FirstOrDefault(),
                    Status = _context.Enrollments
                                .Where(e => (e.Status == (int)EnrollmentStatus.InProcessing || e.Status == (int)EnrollmentStatus.Completed) && e.CourseId == course.Id && e.UserId == userId)
                                .Select(e => e.Status)
                                .FirstOrDefault(),
                })
                .ToList();
            var courseOnGoing = _context.Enrollments.Count(e => e.Status == (int)EnrollmentStatus.InProcessing && e.UserId == userId);
            var courseDone = _context.Enrollments.Count(e => e.Status == (int)EnrollmentStatus.Completed && e.UserId == userId);
            var resultList = new
            {
                Purchased = courseOnGoing + courseDone,
                OnGoing = courseOnGoing,
                Done = courseDone,
                CourseInfo = dataCourse
            };

            return resultList;
        }

        public object GetLearnCourseResult(int courseId, int userId)
        {
            var enroll = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId&& e.UserId == userId);

            return new
            {
                enroll.TimeSpent,
                enroll.TotalScore,
            };
        }
    }
}
