using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PaymentAuditService(IPaymentAuditLogRepository repository) : IPaymentAuditService
    {
        public Task<IEnumerable<PaymentAuditLog>> ObterPorJogoAsync(int gameId)
            => repository.ObterPorJogoAsync(gameId);
    }
}
