using MassTransit;

namespace OutboxSagaDemo.Sagas;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;

    public string CustomerEmail { get; set; } = default!;
    public decimal Total { get; set; }

    public bool StockOk { get; set; }
    public bool PaymentOk { get; set; }
    public string? PaymentAuthCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}