using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PB_Cartoes;
using PB_Cartoes.Consumer;
using PB_Cartoes.Domain.Interfaces;
using PB_Cartoes.Infrastructure.Data;
using PB_Cartoes.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

// OpenTelemetry Tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("PB_Cartoes.Worker"))
            .AddSource("PB_Cartoes.Worker")
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(configuration["OpenTelemetry:OtlpEndpoint"] ?? "");
            });
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CartaoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(configuration["RabbitMQ:Host"], 5672, "/", h =>
        {
            h.Username(configuration["RabbitMQ:Username"] ?? "");
            h.Password(configuration["RabbitMQ:Password"] ?? "");
        });

        cfg.ReceiveEndpoint("cartao-credito-queue", e =>
        {
            e.ConfigureConsumer<CartaoConsumer>(context);
        });
    });
});

builder.Services.AddDbContext<CartoesContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("CartoesDb")));

builder.Services.AddScoped<ICartaoRepository, CartaoRepository>();
builder.Services.AddScoped<CartaoRepository>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
