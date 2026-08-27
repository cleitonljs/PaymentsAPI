using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsAPI.Controllers
{
    // Sem [Authorize]: essa API ainda não tem JWT configurado (é um serviço
    // orientado a evento, sem rotas externas de negócio até este ponto).
    // Se JWT for adicionado no futuro, proteger essa rota com [Authorize(Roles = "Administrador")].
    public class PaymentAuditController(IPaymentAuditService paymentAuditService) : Controller
    {
        [HttpGet("payments/auditoria/{gameId}")]
        public async Task<IActionResult> ObterPorJogo(int gameId)
        {
            var logs = await paymentAuditService.ObterPorJogoAsync(gameId);
            return Ok(logs);
        }
    }
}
