using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Isp
{
    public interface IWriteOnlyRepository<T>
    {
        void Add(T data);
        void Update(T data);
        void Delete(Guid id);
    }
}