using Application.Interfaces;
using Domain.Events;
using MassTransit;
using MassTransit.Transports;
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
            var endpointCatalog = await sendEndpointProvider.GetSendEndpoint(
                    new Uri($"queue:{cfg["RabbitMQ:Queues:FCG_Catalog"]}"));

            await endpointCatalog.Send(evento);

            var endpointNotification = await sendEndpointProvider.GetSendEndpoint(
                    new Uri($"queue:{cfg["RabbitMQ:Queues:FCG_Notification"]}"));

            await endpointNotification.Send(evento);


        }
    }
}
