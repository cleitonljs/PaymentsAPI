using Domain.Common.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Nosql;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PaymentAuditLogRepository : IPaymentAuditLogRepository
    {
        private readonly IMongoCollection<PaymentAuditLog> _logs;

        public PaymentAuditLogRepository(MongoDbContext mongoDbContext, IOptions<MongoSettings> settings)
        {
            _logs = mongoDbContext.GetCollection<PaymentAuditLog>(settings.Value.AuditLogsCollection);
        }

        public async Task<PaymentAuditLog> AdicionarAsync(PaymentAuditLog log)
        {
            if (string.IsNullOrWhiteSpace(log.Id))
                log.Id = ObjectId.GenerateNewId().ToString();

            await _logs.InsertOneAsync(log);

            return log;
        }

        public async Task<IEnumerable<PaymentAuditLog>> ObterPorJogoAsync(int gameId)
        {
            var filtro = Builders<PaymentAuditLog>.Filter.Eq(l => l.GameId, gameId);

            return await _logs.Find(filtro)
                .SortByDescending(l => l.ProcessadoEm)
                .ToListAsync();
        }
    }
}
