using Application.Interfaces;
using Application.Services;
using Domain.Common.Settings;
using Domain.Interfaces;
using Infrastructure.Cache;
using Infrastructure.Messagin.Consumer;
using Infrastructure.Messagin.Producers;
using Infrastructure.Nosql;
using Infrastructure.Repositories;
using MassTransit;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================================
// MongoDB (item 4, Fase 3 — auditoria de pagamentos processados)
// =========================================
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection(MongoSettings.SectionName));

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IPaymentAuditLogRepository, PaymentAuditLogRepository>();
builder.Services.AddScoped<IPaymentAuditService, PaymentAuditService>();

// =========================================
// Redis (item 4, Fase 3 — idempotência no processamento de eventos)
// =========================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "payments-api:";
});

// IConnectionMultiplexer separado do IDistributedCache: precisamos dele para o
// SET NX atômico usado no claim de idempotência (DefinirSeNaoExisteAsync),
// primitiva que o IDistributedCache não expõe.
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"],
            ushort.Parse(builder.Configuration["RabbitMQ:Port"]!),
            "/",
            h =>
            {
                h.Username(
                    builder.Configuration["RabbitMQ:Username"]);

                h.Password(
                    builder.Configuration["RabbitMQ:Password"]);
            });

        cfg.ReceiveEndpoint(
                builder.Configuration["RabbitMQ:Queues:FCG_Payment"],
                e =>
                {
                    e.ConfigureConsumer<OrderPlacedConsumer>(
                        context);
                });
    });
});

builder.Services.AddScoped<IPaymentProcessedProducer, PaymentProcessedProducer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
