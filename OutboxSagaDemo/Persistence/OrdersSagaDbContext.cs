using MassTransit;

using Microsoft.EntityFrameworkCore;

using OutboxSagaDemo.Sagas;
namespace OutboxSagaDemo.Persistence;


public class OrdersSagaDbContext : DbContext
{
    public OrdersSagaDbContext(DbContextOptions<OrdersSagaDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("app");
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity(); // for Consumer Outbox (saga endpoints)

        modelBuilder.Entity<OrderState>(b =>
        {
            b.ToTable("order_state", "app");
            b.HasKey(x => x.CorrelationId);
            b.Property(x => x.CurrentState).HasMaxLength(64);
            b.Property(x => x.CustomerEmail).HasMaxLength(256);
            b.Property(x => x.Total).HasColumnType("numeric(18,2)");
            b.HasIndex(x => x.CurrentState);
        });
    }
}
