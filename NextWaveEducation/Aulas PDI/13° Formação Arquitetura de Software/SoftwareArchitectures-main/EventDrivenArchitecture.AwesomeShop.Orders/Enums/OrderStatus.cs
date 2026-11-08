namespace EventDrivenArchitecture.AwesomeShop.Orders.Enums
{
    public enum OrderStatus
    {
        StartedAndPaymentPending = 1,
        PreparingForDelivery = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }
}
