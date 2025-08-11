using MassTransit;

using Microsoft.EntityFrameworkCore;

using OutboxSagaDemo;
using OutboxSagaDemo.Consumers;
using OutboxSagaDemo.Persistence;
using OutboxSagaDemo.Sagas;

using Serilog;

using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("SqlServer");
builder.Services.AddDbContext<OrdersSagaDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
       .UseSnakeCaseNamingConvention());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.ExistingDbContext<OrdersSagaDbContext>(); // <— key change
            r.UsePostgres();
        });

    x.AddConsumer<OrderSubmittedConsumer>();
    x.AddConsumer<StockReservedConsumer>();
    x.AddConsumer<PaymentAuthorizedConsumer>();
    x.AddConsumer<OrderCompletedConsumer>();

    // Producer-side Bus Outbox on the SAME DbContext
    x.AddEntityFrameworkOutbox<OrdersSagaDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.SetKebabCaseEndpointNameFormatter();

    // Consumer Outbox on every endpoint
    x.AddConfigureEndpointsCallback((ctx, name, ep) =>
    {
        ep.UseEntityFrameworkOutbox<OrdersSagaDbContext>(ctx);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapPost("/checkout", async (OrdersSagaDbContext db, IPublishEndpoint bus, CheckoutRequest req, CancellationToken ct) =>
{
    var id = Guid.NewGuid();

    // Publish BEFORE SaveChanges — Bus Outbox captures it
    await bus.Publish<OrderSubmitted>(new
    {
        OrderId = id,
        CustomerEmail = req.Email,
        Total = req.Total,
        Timestamp = DateTime.UtcNow
    }, ct);

    // COMMIT — persists outbox rows; delivery service sends to RabbitMQ
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { orderId = id });
})
.WithName("Checkout")
.WithSummary("Submit order and start the saga");

app.MapPost("/stock/{orderId:guid}/reserved", async (OrdersSagaDbContext db, Guid orderId, IPublishEndpoint bus, CancellationToken ct) =>
{
    await bus.Publish<StockReserved>(new { OrderId = orderId, Timestamp = DateTime.UtcNow }, ct);
    await db.SaveChangesAsync(ct);
    return Results.Accepted();
})
.WithName("StockReserved");

app.MapPost("/payment/{orderId:guid}/authorized", async (OrdersSagaDbContext db, Guid orderId, IPublishEndpoint bus, CancellationToken ct) =>
{
    var auth = $"AUTH-{Random.Shared.Next(100000, 999999)}";
    await bus.Publish<PaymentAuthorized>(new { OrderId = orderId, AuthorizationCode = auth, Timestamp = DateTime.UtcNow }, ct);
    await db.SaveChangesAsync(ct);
    return Results.Accepted(auth);
})
.WithName("PaymentAuthorized");

// ---- Swagger/HTTP pipeline ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();


//builder.Services.AddMassTransit(x =>
//{
//    //x.AddSagaStateMachine<OrderStateMachine, OrderState>()
//    //    .InMemoryRepository();
//    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
//        .EntityFrameworkRepository(r =>
//        {
//            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
//            //r.ExistingDbContext<OrdersSagaDbContext>();
//            r.AddDbContext<DbContext, OrdersSagaDbContext>((provider, builder) =>
//            {
//                builder.UseNpgsql(connectionString, m =>
//                {
//                    m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
//                    m.MigrationsHistoryTable($"__{nameof(OrdersSagaDbContext)}");
//                });
//            });

//            r.UsePostgres();
//        });

//    //x.AddConsumer<OrderSubmittedConsumer>();
//    //x.AddConsumer<StockReservedConsumer>();
//    //x.AddConsumer<PaymentAuthorizedConsumer>();
//    //x.AddConsumer<OrderCompletedConsumer>();


//    x.AddEntityFrameworkOutbox<OrdersSagaDbContext>(o =>
//    {
//        o.UsePostgres();
//        o.UseBusOutbox();
//    });

//    x.SetKebabCaseEndpointNameFormatter();

//    // Register the global per-endpoint callback BEFORE the bus is built
//    x.AddConfigureEndpointsCallback((registrationContext, endpointName, endpointCfg) =>
//    {
//        endpointCfg.UseEntityFrameworkOutbox<OrdersSagaDbContext>(registrationContext);
//    });

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host("localhost", "/", h =>
//        {
//            h.Username("guest");
//            h.Password("guest");
//        });

//        // Now just configure endpoints (callback above will be applied to each)
//        cfg.ConfigureEndpoints(context);
//    });
//});


//// Add services to the container.
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//app.MapPost("/checkout", async (OrdersSagaDbContext db, IPublishEndpoint bus, CheckoutRequest req, CancellationToken ct) =>
//{
//    var id = Guid.NewGuid();
//    //db.Add(new SeedMarker { Id = id }); // optional: ensures DbContext has work
//    await bus.Publish<OrderSubmitted>(new { OrderId = id, CustomerEmail = req.Email, Total = req.Total, Timestamp = DateTime.UtcNow }, ct);
//    //await db.SaveChangesAsync(ct); // ⬅️ commits outbox rows
//    return Results.Ok(new { orderId = id });
//});

//app.MapPost("/stock/{orderId:guid}/reserved", async (OrdersSagaDbContext db, Guid orderId, IPublishEndpoint bus, CancellationToken ct) =>
//{
//    //db.Add(new SeedMarker { Id = Guid.NewGuid() });
//    await bus.Publish<StockReserved>(new { OrderId = orderId, Timestamp = DateTime.UtcNow }, ct);
//    //await db.SaveChangesAsync(ct);
//    return Results.Accepted();
//});

//app.MapPost("/payment/{orderId:guid}/authorized", async (OrdersSagaDbContext db, Guid orderId, IPublishEndpoint bus, CancellationToken ct) =>
//{
//    var auth = $"AUTH-{Random.Shared.Next(100000, 999999)}";
//    //db.Add(new SeedMarker { Id = Guid.NewGuid() });
//    await bus.Publish<PaymentAuthorized>(new { OrderId = orderId, AuthorizationCode = auth, Timestamp = DateTime.UtcNow }, ct);
//    //await db.SaveChangesAsync(ct);
//    return Results.Accepted(auth);
//});


//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();


//app.Run();


