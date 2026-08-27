using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
        {
            _cache = cache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<T?> ObterAsync<T>(string chave)
        {
            var valor = await _cache.GetStringAsync(chave);

            if (string.IsNullOrEmpty(valor))
                return default;

            return JsonSerializer.Deserialize<T>(valor);
        }

        public async Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao)
        {
            var json = JsonSerializer.Serialize(valor);

            await _cache.SetStringAsync(chave, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiracao
            });
        }

        public async Task RemoverAsync(string chave)
        {
            await _cache.RemoveAsync(chave);
        }

        public async Task<bool> DefinirSeNaoExisteAsync(string chave, TimeSpan expiracao)
        {
            // SET NX com TTL numa única chamada ao Redis: atômico, ao contrário de
            // ObterAsync+DefinirAsync (que são um GET e um SET separados via
            // IDistributedCache). Retorna true só para a primeira chamada a
            // conseguir criar a chave; chamadas concorrentes seguintes recebem false.
            var database = _connectionMultiplexer.GetDatabase();
            return await database.StringSetAsync(chave, "1", expiracao, When.NotExists);
        }

        public async Task LiberarClaimAsync(string chave)
        {
            // Mesma via (Redis cru, sem prefixo) usada em DefinirSeNaoExisteAsync —
            // precisa ser a mesma chave literal que o SET NX criou.
            var database = _connectionMultiplexer.GetDatabase();
            await database.KeyDeleteAsync(chave);
        }
    }
}
