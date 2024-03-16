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
    public interface IAnswerRepository : IBaseRepository<AnswerDTO>
    {
        public object CreateAnswer(int questionId, string answerText, bool isCorrect);
        public object UpdateAnswer(int id, string answerText, bool isCorrect);

    }
    public class AnswerRepository : IAnswerRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public AnswerRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<AnswerDTO> GetList()
        {
            var _list = _context.Answers.ToList();
            var _listDTO = _mapper.Map<List<AnswerDTO>>(_list);
            return _listDTO;
        }

        public AnswerDTO Get(int id)
        {
            var _object = _context.Answers.Find(id);
            var _objectDTO = _mapper.Map<AnswerDTO>(_object);
            return _objectDTO;
        }

        public AnswerDTO Add(AnswerDTO _objectDTO)
        {
            var _object = _mapper.Map<Answer>(_objectDTO);
            _context.Answers.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<AnswerDTO>(_object);
        }

        public int Update(int id, AnswerDTO _objectDTO)
        {
            var existingObject = _context.Answers.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            try
            {
                Answer deletedAnswer = _context.Answers.Find(id);
                if (deletedAnswer == null)
                {
                    return 0;
                }

                int questionId = deletedAnswer.QuestionId;

                _context.Answers.Remove(deletedAnswer);

                _context.SaveChanges();

                int correctAnswerCount = _context.Answers.Count(a => a.QuestionId == questionId && a.IsCorrect);

                var questionToUpdate = _context.Questions.Find(questionId);
                if (questionToUpdate != null)
                {
                    questionToUpdate.QuestionType = correctAnswerCount > 1 ? 1 : 0;
                    _context.SaveChanges();
                }

                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating test: {ex.Message}");
            }
        }


        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Answers.Any(e => e.Id == id);
            return _isExist;
        }

        public object CreateAnswer(int questionId, string answerText, bool isCorrect)
        {
            try
            {
                var newAnswer = new Answer
                {
                    AnswerText = answerText,
                    IsCorrect = isCorrect,
                    QuestionId = questionId
                };

                _context.Answers.Add(newAnswer);
                _context.SaveChanges();

                var answerDTO = _mapper.Map<AnswerDTO>(newAnswer);

                int correctAnswerCount = _context.Answers.Count(a => a.QuestionId == questionId && a.IsCorrect);

                var questionToUpdate = _context.Questions.Find(questionId);
                if (questionToUpdate != null)
                {
                    questionToUpdate.QuestionType = correctAnswerCount > 1 ? 1 : 0;
                    _context.SaveChanges();
                }

                return answerDTO;
            }
            catch (Exception ex)
            {
                return $"Error creating test: {ex.Message}";
            }
        }

        public object UpdateAnswer(int id, string answerText, bool isCorrect)
        {
            try
            {
                var existingAnswer = _context.Answers.Find(id);
                if (existingAnswer == null)
                {
                    return 0;
                }

                existingAnswer.AnswerText = answerText;
                existingAnswer.IsCorrect = isCorrect;

                var answerDTO = _mapper.Map<AnswerDTO>(existingAnswer);

                _context.SaveChanges();

                int correctAnswerCount = _context.Answers.Count(a => a.QuestionId == existingAnswer.QuestionId && a.IsCorrect);

                var questionToUpdate = _context.Questions.Find(existingAnswer.QuestionId);
                if (questionToUpdate != null)
                {
                    questionToUpdate.QuestionType = correctAnswerCount > 1 ? 1 : 0;
                    _context.SaveChanges();
                }

                return answerDTO;
            }
            catch (Exception ex)
            {
                return $"Error creating test: {ex.Message}";
            }
        }
    }
}
