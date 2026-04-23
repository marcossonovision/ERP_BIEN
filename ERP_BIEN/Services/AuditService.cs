using ERP_BIEN.Data;
using ERP_BIEN.Models;
using System;
using System.Threading.Tasks;

namespace ERP_BIEN.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _db;

        public AuditService(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string user, string action, string entity, int? entityId, string details = null)
        {
            var log = new AuditLog
            {
                UserName = user,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Timestamp = DateTime.Now,
                Details = details
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
