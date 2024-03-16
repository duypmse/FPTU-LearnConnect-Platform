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
    public interface ICommentRepository : IBaseRepository<CommentDTO> {
        public object GetByLectureId(int lectureId);

    }
    public class CommentRepository : ICommentRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public CommentRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<CommentDTO> GetList()
        {
            var _list = _context.Comments.ToList();
            var _listDTO = _mapper.Map<List<CommentDTO>>(_list);
            return _listDTO;
        }

        public CommentDTO Get(int id)
        {
            var _object = _context.Comments.Find(id);
            var _objectDTO = _mapper.Map<CommentDTO>(_object);
            return _objectDTO;
        }

        public CommentDTO Add(CommentDTO _objectDTO)
        {
            var _object = _mapper.Map<Comment>(_objectDTO);
            _context.Comments.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<CommentDTO>(_object);
        }

        public int Update(int id, CommentDTO _objectDTO)
        {
            var existingObject = _context.Comments.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            var existingObject = _context.Comments.Find(id);
            if (existingObject == null)
            {
                return 0;
            }
            _context.Comments.Remove(existingObject);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Comments.Any(e => e.Id == id);
            return _isExist;
        }

        public object GetByLectureId(int lectureId)
        {
            var listComment = _context.Comments
                .Where(c => c.LectureId == lectureId && c.Status == (int)CommentStatus.Comment)
                .Include(c => c.User)
                .Select(c => new
                {
                    Comment = new
                    {
                        c.Id,
                        c.UserId,
                        c.ParentCommentId,
                        c.Comment1,
                        c.CommentTime,
                        c.Status,
                        c.LectureId,
                    },
                    User = new
                    {
                        UserId = c.User.Id,
                        UserName = c.User.FullName,
                        UserEmail = c.User.Email,
                        UserImage = c.User.ProfilePictureUrl
                    },
                    Reply = _context.Comments
                        .Where(r => r.ParentCommentId == c.Id && r.Status == (int)CommentStatus.Reply)
                        .Include(c => c.User)
                        .OrderByDescending(c => c.CommentTime)
                        .Select(c => new
                        {
                            Comment = new
                            {
                                c.Id,
                                c.UserId,
                                c.ParentCommentId,
                                c.Comment1,
                                c.CommentTime,
                                c.Status,
                                c.LectureId,
                            },
                            User = new
                            {
                                UserId = c.User.Id,
                                UserName = c.User.FullName,
                                UserEmail = c.User.Email,
                                UserImage = c.User.ProfilePictureUrl
                            },
                        }).ToList(),
                }).ToList();

            return listComment;
        }


    }
}
