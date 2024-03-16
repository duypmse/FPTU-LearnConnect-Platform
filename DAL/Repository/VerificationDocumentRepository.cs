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
    public interface IVerificationDocumentRepository : IBaseRepository<VerificationDocumentDTO> { }
    public class VerificationDocumentRepository : IVerificationDocumentRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public VerificationDocumentRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public VerificationDocumentDTO Add(VerificationDocumentDTO _objectDTO)
        {
            var _object = _mapper.Map<VerificationDocument>(_objectDTO);
            _context.VerificationDocuments.Add(_object);
            return null;
        }

        public VerificationDocumentDTO Get(int id)
        {
            var _object = _context.VerificationDocuments.Find(id);
            var _objectDTO = _mapper.Map<VerificationDocumentDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<VerificationDocumentDTO> GetList()
        {
            var _list = _context.VerificationDocuments.ToList();
            var _listDTO = _mapper.Map<List<VerificationDocumentDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, VerificationDocumentDTO _objectDTO)
        {
            var _object = _context.VerificationDocuments.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.VerificationDocuments.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            VerificationDocument _object = _context.VerificationDocuments.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.VerificationDocuments.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.VerificationDocuments.Any(e => e.Id == id);
            return _isExist;
        }

    }
}
