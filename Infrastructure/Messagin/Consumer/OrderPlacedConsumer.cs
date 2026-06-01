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

            var email = $"Compra Recebida\nGameId:{message.GameId}\nUserId:{message.UserId}\nPrice:{message.Price}";

            Console.WriteLine(email);

            var paymentEvent = new PaymentProcessedEvent
            {
                UserId = message.UserId,
                GameId = message.GameId,
                Status = "Approved"
            };

            await paymentProcessedProducer.PaymentProcessed(paymentEvent);
        }
    }
}
