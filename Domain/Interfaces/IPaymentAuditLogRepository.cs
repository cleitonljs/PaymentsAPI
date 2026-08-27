using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IPaymentAuditLogRepository
    {
        Task<PaymentAuditLog> AdicionarAsync(PaymentAuditLog log);
        Task<IEnumerable<PaymentAuditLog>> ObterPorJogoAsync(int gameId);
    }
}
