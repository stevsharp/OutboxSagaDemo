using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutboxSagaDemo.Sagas;

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.ToTable("order_state", schema: "saga");
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.CustomerEmail).HasMaxLength(256);
        entity.Property(x => x.Total).HasColumnType("numeric(18,2)");
        entity.Property(x => x.PaymentAuthCode).HasMaxLength(64);
        entity.HasIndex(x => x.CurrentState);
    }
}
