using ERP_BIEN.Data;
using ERP_BIEN.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_BIEN.Controllers
{
    public class AuditController : Controller
    {
        private readonly AppDbContext _db;

        public AuditController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(
            string user,
            string action,
            string entity,
            DateTime? from,
            DateTime? to,
            int page = 1,
            int pageSize = 20)
        {
            var query = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(user))
                query = query.Where(x => x.UserName.Contains(user));

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(x => x.Action == action);

            if (!string.IsNullOrWhiteSpace(entity))
                query = query.Where(x => x.Entity == entity);

            if (from.HasValue)
                query = query.Where(x => x.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.Timestamp <= to.Value);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new AuditIndexViewModel
            {
                Logs = logs,
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return View(vm);
        }
    }
}
