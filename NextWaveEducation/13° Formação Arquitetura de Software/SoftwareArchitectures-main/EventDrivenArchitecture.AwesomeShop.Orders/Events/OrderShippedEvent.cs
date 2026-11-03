namespace EventDrivenArchitecture.AwesomeShop.Orders.Events
{
    public class OrderShippedEvent
    {
        public int Id { get; set; }
        public DateTime DateShipped { get; set; }
    }
}
