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
    public interface ISpecializationRepository : IBaseRepository<SpecializationDTO>
    {
        IEnumerable<SpecializationDTO> GetListByMajorId(int majorId);
        public object Create(int majorId, string name, string description);
        public object Update(int id, string name, string description, int majorId);
        public object GetById(int id);


    }
    public class SpecializationRepository : ISpecializationRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public SpecializationRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public SpecializationDTO Add(SpecializationDTO _objectDTO)
        {
            var _object = _mapper.Map<Specialization>(_objectDTO);
            _context.Specializations.Add(_object);
            return null;
        }

        public SpecializationDTO Get(int id)
        {
            var _object = _context.Specializations.Find(id);
            var _objectDTO = _mapper.Map<SpecializationDTO>(_object);
            return _objectDTO;
        }

        public object GetById(int id)
        {
            var _object = _context.Specializations
                .Include(x => x.Major)
                .FirstOrDefault(s => s.Id == id);

            if (_object == null)
            {
                throw new Exception("Not found SpecializationId");
            }

            var _objectDTO = new
            {
                Id = _object.Id,
                Name = _object.Name,
                Description = _object.Description,
                IsActive = _object.IsActive,
                MajorId = _object.MajorId,
                MajorName = _object.Major.Name
            };


            return _objectDTO;
        }

        public IEnumerable<SpecializationDTO> GetList()
        {
            var _list = _context.Specializations.ToList();
            var _listDTO = _mapper.Map<List<SpecializationDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, SpecializationDTO _objectDTO)
        {
            var _object = _context.Specializations.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.Specializations.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            Specialization _object = _context.Specializations.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Specializations.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Specializations.Any(e => e.Id == id);
            return _isExist;
        }
        public IEnumerable<SpecializationDTO> GetListByMajorId(int majorId)
        {
            var _list = _context.Specializations.Where(s => s.MajorId == majorId).ToList();
            var _listDTO = _mapper.Map<List<SpecializationDTO>>(_list);
            return _listDTO;
        }

        public object Create(int majorId, string name, string description)
        {
            try
            {
                var _object = new Specialization
                {
                    MajorId = majorId,
                    Name = name,
                    Description = description,
                    IsActive = true
                };

                _context.Specializations.Add(_object);

                var _objectDTO = _mapper.Map<SpecializationDTO>(_object);

                return _objectDTO;
            }
            catch (Exception ex)
            {
                return $"Error create: {ex.Message}";
            }
        }
        public object Update(int id, string name, string description, int majorId)
        {
            try
            {
                var existing = _context.Specializations.Find(id);
                if (existing == null)
                {
                    return 0;
                }

                existing.Name = name;
                existing.Description = description;
                existing.MajorId = majorId;

                var existingDTO = _mapper.Map<SpecializationDTO>(existing);

                return existingDTO;
            }
            catch (Exception ex)
            {
                return $"Error update: {ex.Message}";
            }
        }
    }
}
