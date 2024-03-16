using AutoMapper;
using BAL.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DAL.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface ITestResultRepository : IBaseRepository<TestResultDTO>
    {
        public TestResultDTO Get(int userId, int courseId);
        public List<object> GetTestsResult(int userId, int courseId);
        public TestResultDTO UpdateTestResult(int userId, int testId, int? score, int? timeSpent);

    }
    public class TestResultRepository : ITestResultRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public TestResultRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<TestResultDTO> GetList()
        {
            var _list = _context.TestResults.ToList();
            var _listDTO = _mapper.Map<List<TestResultDTO>>(_list);
            return _listDTO;
        }

        public TestResultDTO Get(int id)
        {
            var _object = _context.TestResults.Find(id);
            var _objectDTO = _mapper.Map<TestResultDTO>(_object);
            return _objectDTO;
        }

        public TestResultDTO Add(TestResultDTO _objectDTO)
        {
            var _object = _mapper.Map<TestResult>(_objectDTO);
            _context.TestResults.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<TestResultDTO>(_object);
        }

        public int Update(int id, TestResultDTO _objectDTO)
        {
            var existingObject = _context.TestResults.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }

        public TestResultDTO UpdateTestResult(int userId, int testId, int? score, int? timeSpent)
        {
            var testResult = _context.TestResults.FirstOrDefault(ts => ts.UserId == userId && ts.TestId == testId);

            if (testResult == null)
            {
                throw new Exception("TestResult not found.");
            }

            if (score.HasValue)
            {
                testResult.Score = score;
            }
            
            if (timeSpent.HasValue)
            {
                testResult.TimeSpent = timeSpent;
            }

            var courseId = _context.Tests.Find(testId).CourseId;
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

            _context.SaveChanges();

            return _mapper.Map<TestResultDTO>(testResult);
        }


        public int Delete(int id)
        {
            TestResult _object = _context.TestResults.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.TestResults.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.TestResults.Any(e => e.Id == id);
            return _isExist;
        }
        public TestResultDTO Get(int userId, int testId)
        {
            var _object = _context.TestResults
                .FirstOrDefault(e => e.TestId == testId && e.UserId == userId);

            /*var enrollment = _context.Enrollments
                .Include(e => e.PaymentTransaction)
                .FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId && e.EnrollmentDate != null);

            if (_object != null && enrollment != null)
            {
                if (enrollment.Status == (int)EnrollmentStatus.InProcessing)
                {
                    _object.TimeSpent = (int?)DateTime.UtcNow.AddHours(7).Subtract(enrollment.PaymentTransaction.SuccessDate.Value).TotalMinutes;
                }

                if (enrollment.Status == (int)EnrollmentStatus.Completed)
                {
                    _object.TimeSpent = (int?)DateTime.UtcNow.AddHours(7).Subtract(enrollment.PaymentTransaction.SuccessDate.Value).TotalMinutes;
                }
                return _mapper.Map<TestResultDTO>(_object);
            }*/

            return _mapper.Map<TestResultDTO>(_object);
        }

        public List<object> GetTestsResult(int userId, int courseId)
        {
            try
            {
                var testsResults = _context.TestResults
                    .Include(ts => ts.Test)
                    .Where(l => l.UserId == userId && l.Test.CourseId == courseId)
                    .ToList();

                if (testsResults.Count == 0)
                {
                    return new List<object> { "Test result not found for the given user and course." };
                }

                var returnTestsResults = new List<object>();

                foreach (var testsResult in testsResults)
                {
                    var timeSubmit = _context.UserAnswers
                        .Where(us => us.TestResultId == testsResult.Id)
                        .Select(us => us.CreateDate)
                        .FirstOrDefault();

                    var returnTestsResult = new
                    {
                        testsResult.Id,
                        testsResult.Score,
                        testsResult.TimeSpent,
                        testsResult.UserId,
                        testsResult.TestId,
                        TimeSubmit = timeSubmit
                    };
                    returnTestsResults.Add(returnTestsResult);
                }

                return returnTestsResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data: {ex.Message}");
                return new List<object> { $"Error retrieving data: {ex.Message}" };
            }
        }

    }
}
