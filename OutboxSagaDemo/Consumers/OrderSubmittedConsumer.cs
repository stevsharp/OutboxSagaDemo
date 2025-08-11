using MassTransit;

namespace OutboxSagaDemo.Consumers;

public class OrderSubmittedConsumer(ILogger<OrderSubmittedConsumer> logger) : IConsumer<OrderSubmitted>
{
    readonly ILogger<OrderSubmittedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        var msg = context.Message;
        _logger.LogInformation("OrderSubmitted received: {OrderId} for {Email}, Total: {Total}",
            msg.OrderId, msg.CustomerEmail, msg.Total);


        return Task.CompletedTask;
    }
}
