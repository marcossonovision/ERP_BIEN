using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Services;
using ERP_BIEN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_BIEN.Controllers
{
    [Authorize(Policy = "LICENSES")]
    public class LicenseController : Controller
    {
        private readonly ILicenseService _svc;
        private readonly AppDbContext _db;

        public LicenseController(ILicenseService svc, AppDbContext db)
        {
            _svc = svc;
            _db = db;
        }

        // ============================
        // INDEX (LECTURA)
        // ============================
        [Authorize(Policy = "LIC_VIEW")]
        public async Task<IActionResult> Index([FromQuery] LicenseQueryParameters qp)
        {
            qp.PageNumber = qp.PageNumber <= 0 ? 1 : qp.PageNumber;
            qp.PageSize = qp.PageSize <= 0 ? 10 : qp.PageSize;

            var (items, total) = await _svc.GetPagedAsync(qp);

            var totalPages = qp.PageSize > 0
                ? (int)Math.Ceiling(total / (double)qp.PageSize)
                : 0;

            if (totalPages > 0 && qp.PageNumber > totalPages)
            {
                qp.PageNumber = totalPages;
                (items, total) = await _svc.GetPagedAsync(qp);
            }

            var topUsers = items
                .Where(l => l.User != null)
                .GroupBy(l => l.User.Name + " " + l.User.LastName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .ToList();

            var allLicenses = await _db.Licenses
                .Include(l => l.User)
                .ToListAsync();

            var totalLicenses = allLicenses.Count;
            var assignedLicenses = allLicenses.Count(l => l.UserId != null);
            var freeLicenses = allLicenses.Count(l => l.UserId == null);

            var usage = totalLicenses > 0
                ? (int)((assignedLicenses * 100.0) / totalLicenses)
                : 0;

            var vm = new LicenseIndexMvcViewModel
            {
                Licenses = items.ToList(),
                PageNumber = qp.PageNumber,
                PageSize = qp.PageSize,
                TotalItems = total,
                TotalPages = totalPages,
                Search = qp.Search,
                SearchProveedor = qp.SearchProveedor,
                SearchProducto = qp.SearchProducto,
                SearchAsignada = qp.SearchAsignada,

                TotalLicenses = totalLicenses,
                AssignedLicenses = assignedLicenses,
                FreeLicenses = freeLicenses,
                UsagePercentage = usage,

                TopUsers = topUsers
                    .Select(x => $"{x.Name} ({x.Count})")
                    .ToList()
            };

            vm.Users = (await _svc.GetAllUsersAsync()).ToList();
            return View(vm);
        }

        // ============================
        // DETAILS (JSON)
        // ============================
        [Authorize(Policy = "LIC_VIEW")]
        [HttpGet]
        public async Task<JsonResult> DetailsJson(int id)
        {
            var lic = await _svc.GetByIdAsync(id);
            if (lic == null) return Json(null);

            return Json(new
            {
                id = lic.Id,
                code = lic.Code,
                producto = lic.Producto,
                proveedor = lic.Proveedor,
                price = lic.Price,
                caducidad = lic.Caducidad?.ToString("yyyy-MM-dd"),
                asignada = lic.Asignada,
                disponible = lic.Disponible,
                userId = lic.UserId,
                userName = lic.User != null ? $"{lic.User.Name} {lic.User.LastName}" : null
            });
        }

        // ============================
        // HISTÓRICO (JSON)
        // ============================
        [Authorize(Policy = "LIC_VIEW")]
        [HttpGet]
        public async Task<JsonResult> HistoryJson(int licenseId)
        {
            var rows = await _db.LicenseHistories
                .AsNoTracking()
                .Where(h => h.LicenseId == licenseId)
                .Include(h => h.User)
                .OrderByDescending(h => h.StartDate)
                .Select(h => new
                {
                    id = h.Id,
                    userName = h.User != null
                        ? h.User.Name + " " + h.User.LastName
                        : "User " + h.UserId,
                    startDate = h.StartDate.ToString("yyyy-MM-dd HH:mm"),
                    endDate = h.EndDate.HasValue
                        ? h.EndDate.Value.ToString("yyyy-MM-dd HH:mm")
                        : null,
                    duration = h.EndDate.HasValue
                        ? ((h.EndDate.Value - h.StartDate).TotalMinutes).ToString("0") + " min"
                        : "ACTUAL"
                })
                .ToListAsync();

            return Json(rows);
        }

        // ============================
        // CREATE
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LicenseViewModel vm, LicenseQueryParameters qp)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index), qp);

            if (vm.UserId.HasValue &&
                !await _db.Users.AnyAsync(u => u.Id == vm.UserId.Value))
                return RedirectToAction(nameof(Index), qp);

            var lic = new License
            {
                Code = vm.Code?.Trim(),
                Producto = vm.Producto?.Trim(),
                Proveedor = vm.Proveedor?.Trim(),
                Price = vm.Price?.Trim(),
                Caducidad = vm.Caducidad,
                UserId = vm.UserId
            };

            ApplyAssignmentState(lic);

            await _svc.CreateAsync(lic);
            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // EDIT
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LicenseViewModel vm, LicenseQueryParameters qp)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index), qp);

            var lic = await _svc.GetByIdAsync(vm.Id);
            if (lic == null)
                return RedirectToAction(nameof(Index), qp);

            if (vm.UserId.HasValue &&
                !await _db.Users.AnyAsync(u => u.Id == vm.UserId.Value))
                return RedirectToAction(nameof(Index), qp);

            lic.Code = vm.Code?.Trim();
            lic.Producto = vm.Producto?.Trim();
            lic.Proveedor = vm.Proveedor?.Trim();
            lic.Price = vm.Price?.Trim();
            lic.Caducidad = vm.Caducidad;
            lic.UserId = vm.UserId;

            ApplyAssignmentState(lic);

            await _svc.UpdateAsync(lic);
            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // ASSIGN
        // ============================
        [Authorize(Policy = "LIC_ASSIGN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int userId, LicenseQueryParameters qp)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == userId))
                return RedirectToAction(nameof(Index), qp);

            try
            {
                await _svc.AssignToUserAsync(id, userId);
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction(nameof(Index), qp);
            }

            var open = await _db.LicenseHistories
                .Where(h => h.LicenseId == id && h.EndDate == null)
                .FirstOrDefaultAsync();

            if (open != null)
                open.EndDate = DateTime.Now;

            _db.LicenseHistories.Add(new LicenseHistory
            {
                LicenseId = id,
                UserId = userId,
                StartDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // UNASSIGN
        // ============================
        [Authorize(Policy = "LIC_ASSIGN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(int id, LicenseQueryParameters qp)
        {
            var open = await _db.LicenseHistories
                .Where(h => h.LicenseId == id && h.EndDate == null)
                .FirstOrDefaultAsync();

            if (open != null)
                open.EndDate = DateTime.Now;

            await _svc.UnassignAsync(id);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // DELETE
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, LicenseQueryParameters qp)
        {
            await _svc.DeleteAsync(id);
            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // HELPER
        // ============================
        private static void ApplyAssignmentState(License lic)
        {
            lic.Asignada = lic.UserId.HasValue;
            lic.Disponible = !lic.UserId.HasValue;
        }
    }
}
