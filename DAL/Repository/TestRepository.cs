using AutoMapper;
using BAL.Models;
using CloudinaryDotNet;
using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface ITestRepository : IBaseRepository<TestDTO> {
        public object GetTestByCourseId(int courseId);
        public object GetTest(int courseId, int testId);
        public object CreateTest(int courseId, string title, string description);
        public object UpdateTest(int testId, string title, string description);
        public object UpdateStatusTest(int testId, bool status);
        public object ProcessRequest(int testId, bool acceptRequest, string? note);


    }
    public class TestRepository : ITestRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public TestRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<TestDTO> GetList()
        {
            var _list = _context.Tests.ToList();
            var _listDTO = _mapper.Map<List<TestDTO>>(_list);
            return _listDTO;
        }

        public TestDTO Get(int id)
        {
            var _object = _context.Tests.Find(id);
            var _objectDTO = _mapper.Map<TestDTO>(_object);
            return _objectDTO;
        }

        public TestDTO Add(TestDTO _objectDTO)
        {
            var _object = _mapper.Map<Test>(_objectDTO);
            _context.Tests.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<TestDTO>(_object);
        }

        public int Update(int id, TestDTO _objectDTO)
        {
            var existingObject = _context.Tests.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            Test testToDelete = _context.Tests.Find(id);

            if (testToDelete == null)
            {
                throw new Exception("Test not found");
            }

            var questionsToDelete = _context.Questions.Where(q => q.TestId == id).ToList();

            foreach (var question in questionsToDelete)
            {
                var answersToDelete = _context.Answers.Where(a => a.QuestionId == question.Id).ToList();

                _context.Answers.RemoveRange(answersToDelete);
            }

            _context.Questions.RemoveRange(questionsToDelete);

            _context.Tests.Remove(testToDelete);

            _context.SaveChanges();

            return 1;
        }


        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Tests.Any(e => e.Id == id);
            return _isExist;
        }

        public object GetTestByCourseId(int courseId)
        {
            var testsWithQuestions = _context.Tests
                    .Where(test => test.CourseId == courseId)
                    .Select(test => new
                    {
                        Test = new TestDTO
                        {
                            Id = test.Id,
                            Title = test.Title,
                            Description = test.Description,
                            TotalQuestion = test.Questions.Count,
                            CreateDate = test.CreateDate,
                            Status = test.Status,
                            CourseId = test.CourseId
                        },
                        Questions = test.Questions.Select(q => new
                        {
                            Question = new QuestionDTO
                            {
                                Id = q.Id,
                                QuestionText = q.QuestionText,
                                QuestionType = q.QuestionType,
                                Status = q.Status,
                                TestId = q.TestId
                            },
                            Answers = q.Answers.Select(a => new AnswerDTO
                            {
                                Id = a.Id,
                                AnswerText = a.AnswerText,
                                IsCorrect = a.IsCorrect,
                                QuestionId = a.QuestionId
                            }).ToList()
                        }).ToList()
                    })
                    .ToList();

            return testsWithQuestions.Any() ? testsWithQuestions : new List<object>();
        }

        public object GetTest(int courseId, int testId)
        {
            var testsWithQuestions = _context.Tests
                .Where(test => test.CourseId == courseId && test.Id == testId)
                .Select(test => new
                {
                    Test = new TestDTO
                    {
                        Id = test.Id,
                        Title = test.Title,
                        Description = test.Description,
                        TotalQuestion = test.Questions.Count,
                        CreateDate = test.CreateDate,
                        Status = test.Status,
                        CourseId = test.CourseId
                    },
                    Questions = test.Questions.Select(q => new
                    {
                        Question = new QuestionDTO
                        {
                            Id = q.Id,
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            Status = q.Status,
                            TestId = q.TestId
                        },
                        Answers = q.Answers.Select(a => new AnswerDTO
                        {
                            Id = a.Id,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            QuestionId = a.QuestionId
                        }).ToList()
                    }).ToList()
                })
                .ToList();

            return testsWithQuestions.Any() ? testsWithQuestions : new List<object>();
        }



        public object CreateTest(int courseId, string title, string description)
        {
            try
            {
                var newTest = new Test
                {
                    CourseId = courseId,
                    Title = title,
                    Description = description,
                    CreateDate = DateTime.UtcNow.AddHours(7),
                    Status = (int)TestStatus.Pending
                };

                _context.Tests.Add(newTest);

                _context.SaveChanges();

                var testDTO = _mapper.Map<TestDTO>(newTest);

                return testDTO;
            }
            catch (Exception ex)
            {
                return $"Error creating test: {ex.Message}";
            }
        }
        public object UpdateTest(int testId, string title, string description)
        {
            var existingTest = _context.Tests.Find(testId);
            if (existingTest == null)
            {
                return null;
            }

            existingTest.Title = title;
            existingTest.Description = description;
            existingTest.CreateDate = DateTime.UtcNow.AddHours(7);
            existingTest.Status = (int)TestStatus.Pending;

            _context.SaveChanges();
            var testDTO = _mapper.Map<TestDTO>(existingTest);

            return testDTO;
        }
        public object UpdateStatusTest(int testId, bool status)
        {
            var test = _context.Tests.Find(testId);

            if (test == null)
            {
                return "Test not test";
            }
            test.Status = status ? (int)TestStatus.Active : (int)TestStatus.Inactive;

            return status ? "Test show successfully" : "Test hide successfully";
        }
        public object ProcessRequest(int testId, bool acceptRequest, string? note)
        {
            var existing = _context.Tests
                .Where(l => l.Status == (int)TestStatus.Pending)
                .FirstOrDefault(l => l.Id == testId);

            if (existing == null)
            {
                throw new Exception("Test not found for the provided testId.");
            }

            existing.Status = acceptRequest ? (int)TestStatus.Active : (int)TestStatus.Reject;
            existing.Note = acceptRequest ? "Approved!" : note;

            _context.SaveChanges();

            return new
            {
                Message = acceptRequest
                    ? "Test request accepted successfully!"
                    : "Test request rejected successfully!",
                Data = new
                {
                    TestId = testId,
                    AcceptRequest = acceptRequest,
                    Note = note
                }
            };
        }
    }
}
