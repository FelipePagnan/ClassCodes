using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Isp
{
    public class StateRepository : IReadOnlyRepository<State>
    {
        public IEnumerable<State> GetAll(Guid id)
        {
            throw new NotImplementedException();
        }

        public State GetById(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public class State {
        
    }
}