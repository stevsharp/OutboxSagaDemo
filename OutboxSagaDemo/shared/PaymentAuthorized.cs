namespace OutboxSagaDemo;

public interface PaymentAuthorized
{
    Guid OrderId { get; }
    string AuthorizationCode { get; }
    DateTime Timestamp { get; }
}
