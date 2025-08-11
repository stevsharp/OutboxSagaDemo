using MassTransit;

namespace OutboxSagaDemo.Consumers
{
    public class OrderCompletedConsumer : IConsumer<OrderCompleted>
    {
        private readonly ILogger<OrderCompletedConsumer> _log;

        public OrderCompletedConsumer(ILogger<OrderCompletedConsumer> log) => _log = log;

        public Task Consume(ConsumeContext<OrderCompleted> ctx)
        {
            var m = ctx.Message;
            _log.LogInformation("🎉 OrderCompleted: {OrderId} at {Ts}", m.OrderId, m.Timestamp);
            // e.g., trigger fulfillment, thank-you email, loyalty points, etc.
            return Task.CompletedTask;
        }
    }
}