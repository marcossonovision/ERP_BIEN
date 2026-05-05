using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly AppDbContext _db;
        public LicenseService(AppDbContext db) => _db = db;

        public async Task<(IEnumerable<License> Items, int TotalCount)> GetPagedAsync(LicenseQueryParameters qp)
        {
            if (qp.PageNumber < 1) qp.PageNumber = 1;
            if (qp.PageSize < 1) qp.PageSize = 10;

            var query = _db.Licenses
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(qp.Search))
            {
                var s = qp.Search.Trim();
                query = query.Where(l =>
                    l.Code.Contains(s) ||
                    l.Producto.Contains(s) ||
                    l.Proveedor.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(qp.SearchProveedor))
                query = query.Where(l => l.Proveedor.Contains(qp.SearchProveedor.Trim()));

            if (!string.IsNullOrWhiteSpace(qp.SearchProducto))
                query = query.Where(l => l.Producto.Contains(qp.SearchProducto.Trim()));

            if (!string.IsNullOrWhiteSpace(qp.SearchAsignada))
            {
                var raw = qp.SearchAsignada.Trim().ToLowerInvariant();
                bool? asignada = raw switch
                {
                    "true" => true,
                    "false" => false,
                    "1" => true,
                    "0" => false,
                    _ => null
                };

                if (asignada.HasValue)
                    query = query.Where(l => l.Asignada == asignada.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(l => l.Producto)
                .ThenBy(l => l.Code)
                .Skip((qp.PageNumber - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<License> GetByIdAsync(int id)
        {
            return await _db.Licenses
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<License> CreateAsync(License license)
        {
            _db.Licenses.Add(license);
            await _db.SaveChangesAsync();
            return license;
        }

        public async Task<bool> UpdateAsync(License license)
        {
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ent = await _db.Licenses.FindAsync(id);
            if (ent == null) return false;

            _db.Licenses.Remove(ent);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }
    }
}