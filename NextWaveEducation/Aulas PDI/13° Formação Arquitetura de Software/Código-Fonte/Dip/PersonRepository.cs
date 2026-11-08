using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Dip
{
    public interface IPersonRepository
    {
        void Add(Person person);
    }

    public class PersonRepository : IPersonRepository
    {
        public PersonRepository()
        {
            // Abrir Conexão com Banco de Dados
        }
        public void Add(Person person)
        {
            throw new NotImplementedException();
        }
    }
}