using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP_BIEN.Models;
using ERP_BIEN.ViewModels;

namespace ERP_BIEN.Services
{
    public interface ILicenseService
    {
        Task<(IEnumerable<License> Items, int TotalCount)> GetPagedAsync(LicenseQueryParameters qp);
        Task<License> GetByIdAsync(int id);
        Task<License> CreateAsync(License license);
        Task<bool> UpdateAsync(License license);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
