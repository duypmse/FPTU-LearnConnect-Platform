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
    
    public interface IFlagDetailRepository : IBaseRepository<FlagDetailDTO> { }
    public class FlagDetailRepository : IFlagDetailRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public FlagDetailRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<FlagDetailDTO> GetList()
        {
            var _list = _context.FlagDetails.ToList();
            var _listDTO = _mapper.Map<List<FlagDetailDTO>>(_list);
            return _listDTO;
        }

        public FlagDetailDTO Get(int id)
        {
            var _object = _context.FlagDetails.Find(id);
            var _objectDTO = _mapper.Map<FlagDetailDTO>(_object);
            return _objectDTO;
        }

        public FlagDetailDTO Add(FlagDetailDTO _objectDTO)
        {
            var _object = _mapper.Map<FlagDetail>(_objectDTO);
            _context.FlagDetails.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<FlagDetailDTO>(_object);
        }

        public int Update(int id, FlagDetailDTO _objectDTO)
        {
            var existingObject = _context.FlagDetails.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            FlagDetail _object = _context.FlagDetails.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.FlagDetails.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.FlagDetails.Any(e => e.Id == id);
            return _isExist;
        }
    }
}
