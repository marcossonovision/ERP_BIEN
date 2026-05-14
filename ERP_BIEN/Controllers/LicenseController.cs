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
        // INDEX
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

            var allLicenses = await _db.Licenses
                .Include(l => l.User)
                .ToListAsync();

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

                TotalLicenses = allLicenses.Count,
                AssignedLicenses = allLicenses.Count(l => l.UserId != null),
                FreeLicenses = allLicenses.Count(l => l.UserId == null),
                UsagePercentage = allLicenses.Count > 0
                    ? (int)((allLicenses.Count(l => l.UserId != null) * 100.0) / allLicenses.Count)
                    : 0
            };

            vm.Users = (await _svc.GetAllUsersAsync()).ToList();
            return View(vm);
        }

        // ============================
        // DETAILS
        // ============================
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
                userId = lic.UserId,
                userName = lic.User != null ? $"{lic.User.Name} {lic.User.LastName}" : null
            });
        }

        // ============================
        // HISTÓRICO
        // ============================
        [HttpGet]
        public async Task<JsonResult> HistoryJson(int licenseId)
        {
            var rows = await _db.LicenseHistories
                .Where(h => h.LicenseId == licenseId)
                .Include(h => h.User)
                .OrderByDescending(h => h.StartDate)
                .Select(h => new
                {
                    userName = h.User != null
                        ? h.User.Name + " " + h.User.LastName
                        : "User " + h.UserId,
                    startDate = h.StartDate.ToString("yyyy-MM-dd HH:mm"),
                    endDate = h.EndDate.HasValue
                        ? h.EndDate.Value.ToString("yyyy-MM-dd HH:mm")
                        : null,
                    duration = h.EndDate.HasValue
                        ? ((h.EndDate.Value - h.StartDate).TotalMinutes) + " min"
                        : "ACTUAL"
                })
                .ToListAsync();

            return Json(rows);
        }

        // ============================
        // CREATE
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LicenseViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ToastMsg"] = "Error: " + string.Join(" | ", errores);
                TempData["ToastType"] = "error";

                return RedirectToAction(nameof(Index));
            }

            if (vm.UserId.HasValue &&
                !await _db.Users.AnyAsync(u => u.Id == vm.UserId.Value))
            {
                TempData["ToastMsg"] = "Usuario inválido ";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }

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
            TempData["HighlightId"] = lic.Id;
            TempData["ToastMsg"] = "Guardado correctamente ";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // ============================
        // EDIT
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LicenseViewModel vm, LicenseQueryParameters qp)
        {
            var lic = await _svc.GetByIdAsync(vm.Id);
            if (lic == null)
            {
                TempData["ToastMsg"] = "Error al editar";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index), qp);
            }

            lic.Code = vm.Code?.Trim();
            lic.Producto = vm.Producto?.Trim();
            lic.Proveedor = vm.Proveedor?.Trim();
            lic.Price = vm.Price?.Trim();
            lic.Caducidad = vm.Caducidad;
            lic.UserId = vm.UserId;

            ApplyAssignmentState(lic);

            await _svc.UpdateAsync(lic);
            TempData["HighlightId"] = lic.Id;


            TempData["ToastMsg"] = "Guardado correctamente";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // ASSIGN
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int userId, LicenseQueryParameters qp)
        {
            await _svc.AssignToUserAsync(id, userId);

            TempData["ToastMsg"] = "Asignado correctamente ";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // UNASSIGN
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(int id, LicenseQueryParameters qp)
        {
            await _svc.UnassignAsync(id);

            TempData["ToastMsg"] = "Quitado correctamente";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // DELETE
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, LicenseQueryParameters qp)
        {
            await _svc.DeleteAsync(id);

            TempData["ToastMsg"] = "Eliminado correctamente";
            TempData["ToastType"] = "error";

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