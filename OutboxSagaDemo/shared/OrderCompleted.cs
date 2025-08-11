namespace OutboxSagaDemo;

public interface OrderCompleted
{
    Guid OrderId { get; }
    DateTime Timestamp { get; }
}
