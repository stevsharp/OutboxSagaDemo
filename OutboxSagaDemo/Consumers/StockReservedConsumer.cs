using MassTransit;

namespace OutboxSagaDemo.Consumers
{
    public class StockReservedConsumer : IConsumer<StockReserved>
    {
        private readonly ILogger<StockReservedConsumer> _log;

        public StockReservedConsumer(ILogger<StockReservedConsumer> log) => _log = log;

        public Task Consume(ConsumeContext<StockReserved> ctx)
        {
            var m = ctx.Message;
            _log.LogInformation("StockReserved: {OrderId} at {Ts}", m.OrderId, m.Timestamp);
            // e.g., notify warehouse, update BI, etc.
            return Task.CompletedTask;
        }
    }
}
