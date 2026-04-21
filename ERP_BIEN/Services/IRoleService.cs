using System.Collections.Generic;
using System.Threading.Tasks;
using ERP_BIEN.Models;
using ERP_BIEN.ViewModels;

namespace ERP_BIEN.Services
{
    public interface IRoleService
    {
        Task<(IEnumerable<Role> items, int total)> GetPagedAsync(RoleQueryParameters qp);
        Task<Role> GetByIdAsync(int id);
        Task CreateAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(int id);
    }
}
