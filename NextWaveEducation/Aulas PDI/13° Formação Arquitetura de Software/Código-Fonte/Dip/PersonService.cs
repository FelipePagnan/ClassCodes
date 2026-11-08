using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Dip
{
    public class PersonController {
        private readonly IPersonService service;

        public PersonController(IPersonService service)
        {
            this.service = service;
        }
        
    }
    public interface IPersonService
    {
        void Add(PersonDto dto);
    }

    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _repository;
        private readonly IErpSyncService _erpService;
        private readonly IMessageBusService _messageBusService;

        public PersonService(IPersonRepository repository, IErpSyncService erpService, IMessageBusService messageBusService)
        {
            _messageBusService = messageBusService;
            _erpService = erpService;
            _repository = repository;
        }


        public void Add(PersonDto dto)
        {
            var person = new Person(dto.Name, dto.Document, dto.BirthDate);

            _repository.Add(person);

            _erpService.SyncPerson(person);

            _messageBusService.Publish("nome_fila", person);
        }
    }
}