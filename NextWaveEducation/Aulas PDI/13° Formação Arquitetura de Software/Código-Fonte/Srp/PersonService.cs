namespace SolidPrinciples.Srp
{
    public class PersonService
    {
        private readonly PersonRepository _personRepository;
        private readonly ServiceBusService _serviceBusService;
        private readonly ErpSyncService _erpSyncService;
        public PersonService(
            PersonRepository personRepository, 
            ServiceBusService serviceBusService,
            ErpSyncService erpSyncService)
        {
            _personRepository = personRepository;
            _serviceBusService = serviceBusService;
            _erpSyncService = erpSyncService;
        }

        public void Add(PersonDto dto) {
            var person = new Person(dto.Name, dto.Document, dto.BirthDate);

            _personRepository.Add(person);

            _erpSyncService.SyncPerson(person);

            _serviceBusService.Publish("nome_fila", person);
        }
    }
}