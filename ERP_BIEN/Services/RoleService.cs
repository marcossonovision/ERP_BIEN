using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Services
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // GET PAGED (LISTADO + FILTRO)
        // ============================
        public async Task<(IEnumerable<Role> items, int total)> GetPagedAsync(RoleQueryParameters qp)
        {
            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(qp.Search))
            {
                query = query.Where(r =>
                    r.Code.Contains(qp.Search) ||
                    r.Name.Contains(qp.Search));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.Code)
                .Skip((qp.PageNumber - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();

            return (items, total);
        }

        // ============================
        // GET BY ID
        // ============================
        public async Task<Role> GetByIdAsync(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        // ============================
        // CREATE
        // ============================
        public async Task CreateAsync(Role role)
        {
            NormalizeRole(role);

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        // ============================
        // UPDATE ✅ (EDITA DE VERDAD)
        // ============================
        public async Task UpdateAsync(Role role)
        {
            NormalizeRole(role);

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }

        // ============================
        // DELETE
        // ============================
        public async Task DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        // ============================
        // NORMALIZACIÓN (CLAVE)
        // ============================
        private void NormalizeRole(Role role)
        {
            role.Code = string.IsNullOrWhiteSpace(role.Code)
                ? string.Empty
                : role.Code.Trim();

            role.Name = string.IsNullOrWhiteSpace(role.Name)
                ? string.Empty
                : role.Name.Trim();

            
        }
    }
}