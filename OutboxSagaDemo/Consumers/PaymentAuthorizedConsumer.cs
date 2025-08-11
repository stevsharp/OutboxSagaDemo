using MassTransit;

namespace OutboxSagaDemo.Consumers
{
    public class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
    {
        private readonly ILogger<PaymentAuthorizedConsumer> _log;

        public PaymentAuthorizedConsumer(ILogger<PaymentAuthorizedConsumer> log) => _log = log;

        public Task Consume(ConsumeContext<PaymentAuthorized> ctx)
        {
            var m = ctx.Message;
            _log.LogInformation("PaymentAuthorized: {OrderId} | Auth {Auth} | {Ts}",
                m.OrderId, m.AuthorizationCode, m.Timestamp);

            return Task.CompletedTask;
        }
    }
}
