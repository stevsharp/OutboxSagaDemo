namespace OutboxSagaDemo;

public interface StockReserved
{
    Guid OrderId { get; }
    DateTime Timestamp { get; }
}

public class SeedMarker { public Guid Id { get; set; } }

