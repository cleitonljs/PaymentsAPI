using Application.Interfaces;
using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Messagin.Consumer
{
    public class OrderPlacedConsumer(
        IPaymentProcessedProducer paymentProcessedProducer,
        IPaymentAuditLogRepository auditLogRepository,
        ICacheService cacheService) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var message = context.Message;

            // Item 4, Fase 3 — idempotência via Redis: se essa mensagem já foi
            // processada (ou está sendo processada agora por outra entrega
            // concorrente da mesma mensagem), não processa de novo.
            //
            // Usa um claim atômico (SET NX) ANTES de processar, em vez de
            // GET-depois-SET: GET+SET são duas chamadas Redis separadas, então
            // duas entregas da mesma mensagem podem passar pelo GET antes de
            // qualquer uma delas terminar de processar e fazer o SET — foi
            // exatamente esse cenário que causou duplicação sob redelivery real
            // do RabbitMQ (MassTransit processa o ReceiveEndpoint com mais de
            // uma thread por padrão). DefinirSeNaoExisteAsync resolve isso numa
            // única chamada atômica: só uma das entregas concorrentes recebe
            // "true" e segue adiante.
            var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
            var chaveIdempotencia = $"payments:processado:{messageId}";

            var conseguiuClaim = await cacheService.DefinirSeNaoExisteAsync(chaveIdempotencia, TimeSpan.FromHours(24));
            if (!conseguiuClaim)
                return;

            try
            {
                var paymentEvent = new PaymentProcessedEvent
                {
                    UserId = message.UserId,
                    GameId = message.GameId,
                    Status = "Approved"
                };

                await paymentProcessedProducer.PaymentProcessed(paymentEvent);

                // Item 4, Fase 3 — auditoria em MongoDB: antes disso não existia
                // nenhum registro persistido dos pagamentos processados.
                await auditLogRepository.AdicionarAsync(new PaymentAuditLog
                {
                    MessageId = messageId,
                    UserId = message.UserId,
                    GameId = message.GameId,
                    Price = message.Price,
                    Status = paymentEvent.Status,
                    ProcessadoEm = DateTime.UtcNow
                });
            }
            catch
            {
                // Libera a chave de idempotência: se o processamento falhou no
                // meio (ex.: Mongo fora do ar), a mensagem precisa poder ser
                // reprocessada numa próxima tentativa em vez de ficar travada
                // como "já processada" sem nunca ter sido, de fato, concluída.
                // Usa LiberarClaimAsync (não RemoverAsync): precisa remover a
                // MESMA chave sem prefixo que DefinirSeNaoExisteAsync criou —
                // RemoverAsync passa pelo IDistributedCache, que prefixaria a
                // chave com o InstanceName e tentaria apagar outra coisa.
                await cacheService.LiberarClaimAsync(chaveIdempotencia);
                throw;
            }
        }
    }
}
