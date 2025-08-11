
namespace OutboxSagaDemo.Sagas;
using MassTransit;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitting { get; private set; } = default!;
    public State AwaitingStock { get; private set; } = default!;
    public State AwaitingPayment { get; private set; } = default!;
    public State Completed { get; private set; } = default!;
    public State Faulted { get; private set; } = default!;

    public Event<OrderSubmitted> OrderSubmitted { get; private set; } = default!;
    public Event<StockReserved> StockReserved { get; private set; } = default!;
    public Event<PaymentAuthorized> PaymentAuthorized { get; private set; } = default!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, x =>
        {
            x.CorrelateById(m => m.Message.OrderId);
            x.InsertOnInitial = true;
            x.SetSagaFactory(ctx => new OrderState
            {
                CorrelationId = ctx.Message.OrderId,
                CustomerEmail = ctx.Message.CustomerEmail,
                Total = ctx.Message.Total,
                CreatedAtUtc = DateTime.UtcNow
            });
        });

        Event(() => StockReserved, x =>
        {
            x.CorrelateById(m => m.Message.OrderId);
        });

        Event(() => PaymentAuthorized, x =>
        {
            x.CorrelateById(m => m.Message.OrderId);
        });

        Initially(
            When(OrderSubmitted)
                .TransitionTo(Submitting)
                .Then(ctx => { /* logging/metrics if you want */ })
                .TransitionTo(AwaitingStock)
        );

        During(AwaitingStock,
            When(StockReserved)
                .Then(ctx => ctx.Saga.StockOk = true)
                .TransitionTo(AwaitingPayment)
        );

        During(AwaitingPayment,
            When(PaymentAuthorized)
                .Then(ctx =>
                {
                    ctx.Saga.PaymentOk = true;
                    ctx.Saga.PaymentAuthCode = ctx.Message.AuthorizationCode;
                    ctx.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(Completed)
                .Publish(ctx => ctx.Init<OrderCompleted>(new
                {
                    OrderId = ctx.Saga.CorrelationId,
                    Timestamp = InVar.Timestamp
                }))
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
