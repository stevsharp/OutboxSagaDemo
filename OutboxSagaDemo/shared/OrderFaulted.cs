namespace OutboxSagaDemo;

public interface OrderFaulted
{
    Guid OrderId { get; }
    string Reason { get; }
    DateTime Timestamp { get; }
}
