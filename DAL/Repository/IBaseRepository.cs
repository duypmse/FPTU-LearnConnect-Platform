using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IBaseRepository<T>
    {
        T Add(T _object);
        int Delete(int id);
        int Update(int id, T _object);
        IEnumerable<T> GetList();
        T Get(int Id);
        bool Exists(int id);
        int SaveChanges();
    }
}
