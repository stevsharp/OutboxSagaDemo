namespace OutboxSagaDemo;

public interface OrderSubmitted
{
    Guid OrderId { get; }
    string CustomerEmail { get; }
    decimal Total { get; }
    DateTime Timestamp { get; }
}
