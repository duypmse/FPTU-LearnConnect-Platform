using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IQuestionRepository : IBaseRepository<QuestionDTO> {
        public object CreateQuestion(int testId, string questionText);
        public object UpdateQuestion(int id, string questionText);
    }
    public class QuestionRepository : IQuestionRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationRepository notificationRepository;

        public QuestionRepository(LearnConnectDBContext context, IMapper mapper, INotificationRepository notificationRepository)
        {
            _context = context;
            _mapper = mapper;
            this.notificationRepository = notificationRepository;
        }

        public IEnumerable<QuestionDTO> GetList()
        {
            var _list = _context.Questions.ToList();
            var _listDTO = _mapper.Map<List<QuestionDTO>>(_list);
            return _listDTO;
        }

        public QuestionDTO Get(int id)
        {
            var _object = _context.Questions.Find(id);
            var _objectDTO = _mapper.Map<QuestionDTO>(_object);
            return _objectDTO;
        }

        public QuestionDTO Add(QuestionDTO _objectDTO)
        {
            var _object = _mapper.Map<Question>(_objectDTO);
            _context.Questions.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<QuestionDTO>(_object);
        }

        public int Update(int id, QuestionDTO _objectDTO)
        {
            var existingObject = _context.Questions.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            Question questionToDelete = _context.Questions.Find(id);

            if (questionToDelete == null)
            {
                throw new Exception("Question not found");
            }

            var answersToDelete = _context.Answers.Where(a => a.QuestionId == id);
            _context.Answers.RemoveRange(answersToDelete);

            _context.Questions.Remove(questionToDelete);

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
            var _isExist = _context.Questions.Any(e => e.Id == id);
            return _isExist;
        }

        public object CreateQuestion(int testId, string questionText)
        {
            try
            {
                var newQuestion = new Question
                {
                    QuestionText = questionText,
                    QuestionType = 0,
                    Status = 0,
                    TestId = testId
                };

                _context.Questions.Add(newQuestion);

                var questionDTO = _mapper.Map<QuestionDTO>(newQuestion);

                _context.SaveChanges();

                var countQuestion = _context.Questions.Where(q => q.TestId == testId).Count();

                if (countQuestion == 2)
                {
                    var usersReceive = _context.Users.Where(u => u.Role == (int)Roles.Staff).Select(u => u.Id).ToArray();
                    var test = _context.Tests
                        .Include(u => u.Course)
                        .FirstOrDefault(t => t.Id == testId);

                    if (test != null && usersReceive != null)
                    {
                        notificationRepository.Create(
                            "New Request",
                            $"Course {test.Course.Name} has just been created and needs to be approved.",
                            usersReceive
                        );
                    }
                }

                return questionDTO;
            }
            catch (Exception ex)
            {
                return $"Error creating test: {ex.Message}";
            }
        }
        public object UpdateQuestion(int id, string questionText)
        {
            try
            {
                var existingQuestion = _context.Questions.Find(id);
                if (existingQuestion == null)
                {
                    return 0;
                }

                existingQuestion.QuestionText = questionText;

                var questionDTO = _mapper.Map<QuestionDTO>(existingQuestion);

                return questionDTO;
            }
            catch (Exception ex)
            {
                return $"Error creating test: {ex.Message}";
            }
        }
    }
}
