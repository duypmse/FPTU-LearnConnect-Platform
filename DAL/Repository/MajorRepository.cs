using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DAL.Repository
{
    public interface IMajorRepository : IBaseRepository<MajorDTO>
    {
        public IEnumerable<object> GetMajorsNotRequestYet(int mentorUserId);
        public object Create(string name, string description);
        public object Update(int id, string name, string description);

    }
    public class MajorRepository : IMajorRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public MajorRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public MajorDTO Add(MajorDTO _objectDTO)
        {
            var _object = _mapper.Map<Major>(_objectDTO);
            _context.Majors.Add(_object);
            return null;
        }

        public MajorDTO Get(int id)
        {
            var _object = _context.Majors.Find(id);
            var _objectDTO = _mapper.Map<MajorDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<MajorDTO> GetList()
        {
            var _list = _context.Majors.ToList();
            var _listDTO = _mapper.Map<List<MajorDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, MajorDTO _objectDTO)
        {
            var _object = _context.Majors.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.Majors.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            Major majorToDelete = _context.Majors.Find(id);

            if (majorToDelete == null)
            {
                throw new Exception("Major not found!");
            }
            var specsToDelete = _context.Specializations.Where(a => a.MajorId == id);
            _context.Specializations.RemoveRange(specsToDelete);
            _context.Majors.Remove(majorToDelete);
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
            var _isExist = _context.Majors.Any(e => e.Id == id);
            return _isExist;
        }

        public IEnumerable<object> GetMajorsNotRequestYet(int mentorUserId)
        {
            var taughtSpecIds = _context.SpecializationOfMentors
                .Where(sm => sm.Mentor.UserId == mentorUserId && (sm.Status == (int)SpecializationOfMentorStatus.Approve || sm.Status == (int)SpecializationOfMentorStatus.Pending))
                .Select(sm => sm.SpecializationId)
                .ToList();

            var majorsNotTaughtCompletely = _context.Majors
                .Where(major =>
                    major.Specializations.Any(spec => !taughtSpecIds.Contains(spec.Id))
                )
                .Select(major => new { MajorId = major.Id, MajorName = major.Name })
                .ToList();

            return majorsNotTaughtCompletely;
        }

        public object Create(string name, string description)
        {
            try
            {
                var _object = new Major
                {
                    Name = name,
                    Description = description,
                    IsActive = true
                };

                _context.Majors.Add(_object);

                var _objectDTO = _mapper.Map<MajorDTO>(_object);

                return _objectDTO;
            }
            catch (Exception ex)
            {
                return $"Error create: {ex.Message}";
            }
        }
        public object Update(int id, string name, string description)
        {
            try
            {
                var existing = _context.Majors.Find(id);
                if (existing == null)
                {
                    return 0;
                }

                existing.Name = name;
                existing.Description = description;

                var existingDTO = _mapper.Map<MajorDTO>(existing);

                return existingDTO;
            }
            catch (Exception ex)
            {
                return $"Error update: {ex.Message}";
            }
        }


    }
}
