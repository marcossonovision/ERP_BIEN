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

        // ============================================================
        // GET PAGED (LISTADO + FILTROS)
        // ============================================================
        public async Task<(IEnumerable<License> Items, int TotalCount)> GetPagedAsync(LicenseQueryParameters qp)
        {
            if (qp.PageNumber < 1) qp.PageNumber = 1;
            if (qp.PageSize < 1) qp.PageSize = 10;

            var query = _db.Licenses
                .Include(l => l.User)
                .AsQueryable();

            // ----------------------------
            // Search general
            // ----------------------------
            if (!string.IsNullOrWhiteSpace(qp.Search))
            {
                var s = qp.Search.Trim();
                query = query.Where(l =>
                    l.Code.Contains(s) ||
                    l.Producto.Contains(s) ||
                    l.Proveedor.Contains(s));
            }

            // ----------------------------
            // Filtro proveedor
            // ----------------------------
            if (!string.IsNullOrWhiteSpace(qp.SearchProveedor))
            {
                var proveedor = qp.SearchProveedor.Trim();
                query = query.Where(l => l.Proveedor.Contains(proveedor));
            }

            // ----------------------------
            // Filtro producto
            // ----------------------------
            if (!string.IsNullOrWhiteSpace(qp.SearchProducto))
            {
                var producto = qp.SearchProducto.Trim();
                query = query.Where(l => l.Producto.Contains(producto));
            }

            // ============================================================
            // ✅ PASO 2: FILTRO ASIGNADA POR UserId (NO por l.Asignada)
            // ============================================================
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
                {
                    if (asignada.Value)
                        query = query.Where(l => l.UserId != null);
                    else
                        query = query.Where(l => l.UserId == null);
                }
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(l => l.Producto)
                .ThenBy(l => l.Code)
                .Skip((qp.PageNumber - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();

            // ============================================================
            // ✅ Blindaje visual: recalcular flags según UserId
            // (aunque en BD haya datos viejos, la UI se ve coherente)
            // ============================================================
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
            // Si el license viene trackeado (lo normal cuando lo has cargado con GetByIdAsync),
            // basta con SaveChangesAsync.
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
        // USERS (para dropdown, etc.)
        // ============================================================
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }
        // ============================================================
        // ASSIGN LICENSE TO USER
        // ============================================================
        public async Task<bool> AssignToUserAsync(int licenseId, int userId)
        {
            var license = await _db.Licenses
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                throw new InvalidOperationException("La licencia no existe.");

            // ✅ Fuente única de verdad
            if (license.UserId != null)
                throw new InvalidOperationException("La licencia ya está asignada.");

            // ✅ RF-036: no sobreasignar (blindaje extra)
            var total = await _db.Licenses.CountAsync(l =>
                l.Producto == license.Producto &&
                l.Proveedor == license.Proveedor);

            var assigned = await _db.Licenses.CountAsync(l =>
                l.Producto == license.Producto &&
                l.Proveedor == license.Proveedor &&
                l.UserId != null);

            if (assigned >= total)
                throw new InvalidOperationException("No hay licencias disponibles para este producto.");

            // ✅ Asignación
            license.UserId = userId;

            // Flags derivados (para coherencia inmediata si se usan)
            license.Asignada = true;
            license.Disponible = false;

            await _db.SaveChangesAsync();
            return true;
        }
        // ============================================================
        // UNASSIGN LICENSE
        // ============================================================
        public async Task<bool> UnassignAsync(int licenseId)
        {
            var license = await _db.Licenses
                .FirstOrDefaultAsync(l => l.Id == licenseId);

            if (license == null)
                return false;

            if (license.UserId == null)
                return false;

            license.UserId = null;

            // Flags derivados
            license.Asignada = false;
            license.Disponible = true;

            await _db.SaveChangesAsync();
            return true;
        }

    }

}