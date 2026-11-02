namespace SolidPrinciples.Srp
{
    public interface IErpSyncService
    {
        void SyncPerson(Person person);
    }

    public class ErpSyncService : IErpSyncService
    {
        public void SyncPerson(Person person)
        {
            throw new NotImplementedException();
        }
    }
}