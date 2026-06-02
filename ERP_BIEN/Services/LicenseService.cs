using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_BIEN.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly AppDbContext _db;

        public LicenseService(AppDbContext db)
        {
            _db = db;
        }

        // ============================================================
        // GET PAGED
        // ============================================================
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
            {
                var proveedor = qp.SearchProveedor.Trim();
                query = query.Where(l => l.Proveedor.Contains(proveedor));
            }

            if (!string.IsNullOrWhiteSpace(qp.SearchProducto))
            {
                var producto = qp.SearchProducto.Trim();
                query = query.Where(l => l.Producto.Contains(producto));
            }

            if (!string.IsNullOrWhiteSpace(qp.SearchAsignada))
            {
                var raw = qp.SearchAsignada.Trim().ToLower();

                if (raw == "true")
                    query = query.Where(l => l.UserId != null);
                else if (raw == "false")
                    query = query.Where(l => l.UserId == null);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(l => l.Producto)
                .ThenBy(l => l.Code)
                .Skip((qp.PageNumber - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();

            foreach (var l in items)
            {
                l.Asignada = l.UserId != null;
                l.Disponible = l.UserId == null;
            }

            return (items, total);
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<License> GetByIdAsync(int id)
        {
            return await _db.Licenses
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<License> CreateAsync(License license)
        {
            _db.Licenses.Add(license);
            await _db.SaveChangesAsync();
            return license;
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task<bool> UpdateAsync(License license)
        {
            await _db.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // DELETE
        // ============================================================
        public async Task<bool> DeleteAsync(int id)
        {
            var ent = await _db.Licenses.FindAsync(id);
            if (ent == null) return false;

            _db.Licenses.Remove(ent);
            await _db.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // USERS
        // ============================================================
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        // ============================================================
        // ✅ ASSIGN (CORRECTO)
        // ============================================================
        public async Task<bool> AssignToUserAsync(int licenseId, int userId)
        {
            if (licenseId <= 0 || userId <= 0)
                return false;

            var license = await _db.Licenses
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return false;

            // ✅ SOLO UNA ASIGNACIÓN
            license.UserId = userId;
            license.Asignada = true;
            license.Disponible = false;

            await _db.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // ✅ UNASSIGN (CORRECTO)
        // ============================================================
        public async Task<bool> UnassignAsync(int licenseId)
        {
            var license = await _db.Licenses
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return false;

            // ✅ QUITAR USUARIO
            license.UserId = null;
            license.Asignada = false;
            license.Disponible = true;

            await _db.SaveChangesAsync();

            return true;
        }
    }
}