using Application.Interfaces;
using Domain.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messagin.Consumer
{
    public class OrderPlacedConsumer (IPaymentProcessedProducer paymentProcessedProducer) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var message = context.Message;

            var email = $"Pedido de compra recebido\nAprovando o pagamento ...";

            Console.WriteLine(email);

            var paymentEvent = new PaymentProcessedEvent
            {
                UserId = message.UserId,
                GameId = message.GameId,
                Status = "Approved"
            };

            await paymentProcessedProducer.PaymentProcessed(paymentEvent);

            Console.WriteLine("\nPagamento aprovado\n");
        }
    }
}
