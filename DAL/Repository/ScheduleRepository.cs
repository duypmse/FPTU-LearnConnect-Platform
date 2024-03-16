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
    public interface IScheduleRepository : IBaseRepository<ScheduleDTO>
    {
        public IEnumerable<ScheduleDTO> GetAllByUser(int userId);
    }
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public ScheduleRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<ScheduleDTO> GetList()
        {
            var _list = _context.Schedules.ToList();
            var _listDTO = _mapper.Map<List<ScheduleDTO>>(_list);
            return _listDTO;
        }
        public IEnumerable<ScheduleDTO> GetAllByUser(int userId)
        {  
            var _list = _context.Schedules.Where(s => s.UserId == userId).Join(_context.Courses, s => s.CourseId, c => c.Id, (s,c) => new ScheduleDTO
            { 
                Id = s.Id,
                CourseId = s.CourseId,
                CourseName = c.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Note = s.Note,
                UserId = s.UserId
            });
            return _list;
        }

        public ScheduleDTO Get(int id)
        {
            var _object = _context.Schedules.Find(id);
            var _objectDTO = _mapper.Map<ScheduleDTO>(_object);
            return _objectDTO;
        }

        public ScheduleDTO Add(ScheduleDTO _objectDTO)
        {
            var _object = _mapper.Map<Schedule>(_objectDTO);
            _object.StartDate = _object.StartDate.AddHours(7);
            _object.EndDate = _object.EndDate.AddHours(7);
            _context.Schedules.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<ScheduleDTO>(_object);
        }

        public int Update(int id, ScheduleDTO _objectDTO)
        {
            var existingObject = _context.Schedules.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            Schedule _object = _context.Schedules.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Schedules.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Schedules.Any(e => e.Id == id);
            return _isExist;
        }
    }
}
