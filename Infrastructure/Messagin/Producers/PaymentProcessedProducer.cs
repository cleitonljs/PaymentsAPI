using Application.Interfaces;
using Domain.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messagin.Producers
{
    public class PaymentProcessedProducer(ISendEndpointProvider sendEndpointProvider, IConfiguration cfg) : IPaymentProcessedProducer
    {
        public async Task PaymentProcessed(PaymentProcessedEvent evento)
        {
            Console.WriteLine($"Gerando evento para a fila {cfg["RabbitMQ:Queues:FCG_Payment"]}");

            var endpoint = await sendEndpointProvider.GetSendEndpoint(
                    new Uri($"queue:{cfg["RabbitMQ:Queues:FCG_Payment"]}"));

            await endpoint.Send(evento);
        }
    }
}
