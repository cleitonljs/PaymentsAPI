using System;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    // Abstração de cache distribuído (Redis). Item 4 da Fase 3.
    public interface ICacheService
    {
        Task<T?> ObterAsync<T>(string chave);
        Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao);
        Task RemoverAsync(string chave);

        // Claim atômico (SET NX) usado para idempotência: retorna true só para
        // quem conseguir criar a chave primeiro. Diferente de ObterAsync+DefinirAsync,
        // que são duas operações Redis separadas e não servem para check-then-act
        // sob concorrência (duas entregas da mesma mensagem podem passar no GET
        // antes de qualquer uma delas fazer o SET).
        Task<bool> DefinirSeNaoExisteAsync(string chave, TimeSpan expiracao);

        // Par de DefinirSeNaoExisteAsync: libera uma claim de idempotência.
        // Usa a MESMA via (Redis cru, chave sem prefixo de instância) que
        // DefinirSeNaoExisteAsync — ao contrário de RemoverAsync, que passa pelo
        // IDistributedCache e aplica o InstanceName configurado, então removeria
        // uma chave diferente da que foi criada pelo claim.
        Task LiberarClaimAsync(string chave);
    }
}
