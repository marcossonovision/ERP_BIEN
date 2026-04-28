using ERP_BIEN.Models;
using ERP_BIEN.Services;
using ERP_BIEN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_BIEN.Controllers
{
    // ============================
    // ACCESO AL MÓDULO LICENSES (LECTURA REAL)
    // ============================
    [Authorize(Policy = "LIC_VIEW")]
    public class LicenseController : Controller
    {
        private readonly ILicenseService _svc;
        public LicenseController(ILicenseService svc) => _svc = svc;

        // ============================
        // INDEX (LECTURA)
        // ============================
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
                SearchAsignada = qp.SearchAsignada
            };

            return View(vm);
        }

        // ============================
        // DETAILS (LECTURA – JSON)
        // ============================
        [HttpGet]
        public async Task<JsonResult> DetailsJson(int id)
        {
            var lic = await _svc.GetByIdAsync(id);
            if (lic == null) return Json(null);

            var dto = new
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
                userName = lic.User != null
                    ? $"{lic.User.Name} {lic.User.LastName}"
                    : null
            };

            return Json(dto);
        }

        // ============================
        // CREATE (ESCRITURA)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LicenseViewModel vm, LicenseQueryParameters qp)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index), qp);

            var lic = new License
            {
                Code = vm.Code?.Trim(),
                Producto = vm.Producto?.Trim(),
                Proveedor = vm.Proveedor?.Trim(),
                Price = vm.Price?.Trim(),
                Caducidad = vm.Caducidad,
                Asignada = vm.Asignada,
                Disponible = vm.Disponible,
                UserId = vm.UserId
            };

            await _svc.CreateAsync(lic);
            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // EDIT (ESCRITURA)
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

            lic.Code = vm.Code?.Trim();
            lic.Producto = vm.Producto?.Trim();
            lic.Proveedor = vm.Proveedor?.Trim();
            lic.Price = vm.Price?.Trim();
            lic.Caducidad = vm.Caducidad;
            lic.Asignada = vm.Asignada;
            lic.Disponible = vm.Disponible;
            lic.UserId = vm.UserId;

            await _svc.UpdateAsync(lic);
            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // DELETE (ESCRITURA)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, LicenseQueryParameters qp)
        {
            await _svc.DeleteAsync(id);
            return RedirectToAction(nameof(Index), qp);
        }
    }
}