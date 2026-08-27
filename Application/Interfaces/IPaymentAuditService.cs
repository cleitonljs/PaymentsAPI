using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPaymentAuditService
    {
        Task<IEnumerable<PaymentAuditLog>> ObterPorJogoAsync(int gameId);
    }
}
