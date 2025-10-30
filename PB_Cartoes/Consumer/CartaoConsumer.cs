using MassTransit;
using PB_Cartoes.Domain.Entities;
using PB_Cartoes.Domain.Interfaces;
using PB_Common.Events;
using System.Diagnostics;
using System.Text.Json;

namespace PB_Cartoes.Consumer
{
    public class CartaoConsumer : IConsumer<PropostaCreditoCriadaEvent>
    {
        private readonly ILogger<CartaoConsumer> _logger;
        private readonly ICartaoRepository _repository;
        private static readonly ActivitySource ActivitySource = new("PB_Cartoes.Worker");

        public CartaoConsumer(ILogger<CartaoConsumer> logger, ICartaoRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<PropostaCreditoCriadaEvent> context)
        {
            var proposta = context.Message;

            // Extrai traceparent/tracestate dos headers, se existirem
            var traceParent = context.Headers.Get<string>("traceparent");
            var traceState = context.Headers.Get<string>("tracestate");

            var activityContext = traceParent != null
                ? ActivityContext.Parse(traceParent, traceState)
                : default;

            using var activity = ActivitySource.StartActivity("CartaoConsumer.Consume", ActivityKind.Consumer, activityContext);
            try
            {
                activity?.SetTag("cliente.id", proposta.ClienteId);
                activity?.SetTag("proposta.score", proposta.Score);

                _logger.LogInformation(
                    "[CartaoConsumer] Proposta recebida para ClienteId: {ClienteId}, Score: {Score}",
                    proposta.ClienteId, proposta.Score);

                var cartoes = new List<CartaoCriadoEvent>();

                cartoes.Add(CriarCartao(proposta.ClienteId, proposta.Limite1));

                if (proposta.Limite2.HasValue)
                    cartoes.Add(CriarCartao(proposta.ClienteId, proposta.Limite2.Value));

                activity?.SetTag("qtd.cartoes", cartoes.Count);

                foreach (var cartao in cartoes)
                {
                    activity?.AddEvent(new ActivityEvent("CriandoCartao"));

                    // Persiste na base
                    var entity = new Cartao
                    {
                        Id = Guid.NewGuid(),
                        ClienteId = cartao.ClienteId,
                        NumeroCartao = cartao.NumeroCartao,
                        Limite = cartao.Limite,
                        CriadoEmUtc = cartao.DataCriacao
                    };

                    await _repository.AddAsync(entity);

                    await context.Publish(cartao, publishContext =>
                    {
                        if (activity != null)
                        {
                            publishContext.Headers.Set("traceparent", activity.Id);

                            if (!string.IsNullOrEmpty(activity.TraceStateString))
                                publishContext.Headers.Set("tracestate", activity.TraceStateString);

                            foreach (var item in activity.Baggage)
                                publishContext.Headers.Set(item.Key, item.Value);
                        }
                    });

                    _logger.LogInformation(
                        "[CartaoConsumer] Cartão criado para ClienteId: {ClienteId}, Numero: {Numero}",
                        cartao.ClienteId, cartao.NumeroCartao);
                }

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex,
                    "🔴 [CartaoConsumer] Falha ao processar proposta de crédito para ClienteId: {ClienteId}",
                    proposta.ClienteId);

                // Serializa payload original
                var payload = JsonSerializer.Serialize(context.Message);

                var failure = new CartaoFalhaEvent(
                    Guid.NewGuid(),
                    proposta.ClienteId,
                    typeof(PropostaCreditoCriadaEvent).AssemblyQualifiedName!,
                    payload,
                    Attempt: 1,
                    Reason: ex.ToString(),
                    OccurredUtc: DateTime.UtcNow
                );

                await context.Publish(failure, publishContext =>
                {
                    if (activity != null)
                    {
                        publishContext.Headers.Set("traceparent", activity.Id);

                        if (!string.IsNullOrEmpty(activity.TraceStateString))
                            publishContext.Headers.Set("tracestate", activity.TraceStateString);

                        foreach (var item in activity.Baggage)
                            publishContext.Headers.Set(item.Key, item.Value);
                    }
                });
            }
        }

        private CartaoCriadoEvent CriarCartao(Guid clienteId, decimal limite)
        {
            // Simulação de número de cartão aleatório (em produção usaria serviço de cartão)
            var numeroCartao = $"{new Random().Next(1000_0000, 9999_9999)}-{new Random().Next(1000_0000, 9999_9999)}";
            return new CartaoCriadoEvent(clienteId, numeroCartao, limite, DateTime.UtcNow);
        }
    }
}
