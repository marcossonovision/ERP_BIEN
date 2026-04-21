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
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _db;
        public RoleService(AppDbContext db) => _db = db;

        public async Task<(IEnumerable<Role> items, int total)> GetPagedAsync(RoleQueryParameters qp)
        {
            qp.PageNumber = qp.PageNumber <= 0 ? 1 : qp.PageNumber;
            qp.PageSize = qp.PageSize <= 0 ? 10 : qp.PageSize;

            var query = _db.Roles
                .Include(r => r.UserRoles)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(qp.Search))
            {
                var s = qp.Search.Trim();
                query = query.Where(r => (r.Code ?? "").Contains(s) || (r.Name ?? "").Contains(s));
            }

            var total = await query.CountAsync();

            var skip = (qp.PageNumber - 1) * qp.PageSize;
            if (skip < 0) skip = 0;

            var items = await query
                .OrderBy(r => r.Name)
                .Skip(skip)
                .Take(qp.PageSize)
                .ToListAsync();

            Console.WriteLine($"[RoleService] total={total}, skip={skip}, take={qp.PageSize}");

            return (items, total);
        }

        public async Task<Role> GetByIdAsync(int id) =>
            await _db.Roles.Include(r => r.RolePermissions).Include(r => r.UserRoles).FirstOrDefaultAsync(r => r.Id == id);

        public async Task CreateAsync(Role role)
        {
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            _db.Roles.Update(role);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _db.Roles.FindAsync(id);
            if (e != null)
            {
                _db.Roles.Remove(e);
                await _db.SaveChangesAsync();
            }
        }
    }
}
