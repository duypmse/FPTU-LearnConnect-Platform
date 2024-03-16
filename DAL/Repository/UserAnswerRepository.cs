using AutoMapper;
using BAL.Models;
using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IUserAnswerRepository : IBaseRepository<UserAnswerDTO> {
        public void Create(int userId, int courseId, int[] answerIds);
        public object Get(int userId, int testId);

    }
    public class UserAnswerRepository : IUserAnswerRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public UserAnswerRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<UserAnswerDTO> GetList()
        {
            var _list = _context.UserAnswers.ToList();
            var _listDTO = _mapper.Map<List<UserAnswerDTO>>(_list);
            return _listDTO;
        }

        public UserAnswerDTO Get(int id)
        {
            var _object = _context.UserAnswers.Find(id);
            var _objectDTO = _mapper.Map<UserAnswerDTO>(_object);
            return _objectDTO;
        }

        public UserAnswerDTO Add(UserAnswerDTO _objectDTO)
        {
            var _object = _mapper.Map<UserAnswer>(_objectDTO);
            _context.UserAnswers.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<UserAnswerDTO>(_object);
        }

        public int Update(int id, UserAnswerDTO _objectDTO)
        {
            var existingObject = _context.UserAnswers.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            UserAnswer _object = _context.UserAnswers.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.UserAnswers.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.UserAnswers.Any(e => e.Id == id);
            return _isExist;
        }

        public void Create(int userId, int testId, int[] answerIds)
        {
            try
            {
                var testResult = _context.TestResults
                    .FirstOrDefault(l => l.UserId == userId && l.TestId == testId);

                if (testResult == null)
                {
                    testResult = new TestResult
                    {
                        Score = 0,
                        TimeSpent = 0,
                        UserId = userId,
                        TestId = testId,
                    };
                    _context.TestResults.Add(testResult);
                    _context.SaveChanges();
                }
                
                var listOldUserAnswer = _context.UserAnswers.Where(u => u.TestResultId == testResult.Id).ToList();

                foreach(var userAnswer in listOldUserAnswer)
                {
                    _context.UserAnswers.Remove(userAnswer);
                }
                _context.SaveChanges();
                
                foreach(var answerId in answerIds)
                {
                    var userAnswer = new UserAnswer
                    {
                        AnswerId = answerId,
                        CreateDate = DateTime.UtcNow.AddHours(7),
                        Status = 0,
                        TestResultId = testResult.Id,
                    };

                    _context.UserAnswers.Add(userAnswer);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error at Create() in UserAnswerRepo: " + ex);
            }
        }

        public object Get(int userId, int testId)
        {
            try
            {
                var testsResult = _context.TestResults
                    .Where(l => l.UserId == userId && l.TestId == testId).ToList();

                if (testsResult != null)
                {
                    var returnData = new List<List<UserAnswerDTO>>();
                    foreach (var testResult in testsResult)
                    {
                        var userAnswers = _context.UserAnswers
                        .Where(ua => ua.TestResultId == testResult.Id)
                        .ToList();

                        var userAnswerDTOs = _mapper.Map<List<UserAnswerDTO>>(userAnswers);
                        returnData.Add(userAnswerDTOs);
                    }
                    return returnData;
                }

                return "Test result not found for the given user and course.";
            }
            catch (Exception ex)
            {
                return $"Error retrieving data: {ex.Message}";
            }
        }



    }
}
