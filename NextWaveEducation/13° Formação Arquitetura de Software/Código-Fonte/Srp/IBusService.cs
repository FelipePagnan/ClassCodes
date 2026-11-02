namespace SolidPrinciples.Srp
{
    public interface IBusService
    {
        void Publish(string queue, object data);
    }

    public class ServiceBusService : IBusService
    {
        public void Publish(string queue, object data)
        {
            throw new NotImplementedException();
        }
    }
}