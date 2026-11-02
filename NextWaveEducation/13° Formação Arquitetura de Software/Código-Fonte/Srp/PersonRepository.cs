using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Srp
{

    public class PersonRepository : IPersonRepository
    {
        public void Add(Person person)
        {
            // Implementaria a persistência
        }
    }
}