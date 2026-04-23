using System.Threading.Tasks;

namespace ERP_BIEN.Services
{
    public interface IAuditService
    {
        Task LogAsync(string user, string action, string entity, int? entityId, string details = null);
    }
}
