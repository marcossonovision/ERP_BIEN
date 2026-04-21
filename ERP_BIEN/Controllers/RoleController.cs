using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using ERP_BIEN.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_BIEN.Controllers
{
    public class RoleController : Controller
    {
        private readonly IRoleService _svc;
        public RoleController(IRoleService svc) => _svc = svc;

        // GET: /Role or /Roles
        public async Task<IActionResult> Index([FromQuery] RoleQueryParameters qp)
        {
            qp = qp ?? new RoleQueryParameters();
            qp.PageNumber = qp.PageNumber <= 0 ? 1 : qp.PageNumber;
            qp.PageSize = qp.PageSize <= 0 ? 10 : qp.PageSize;

            var (items, total) = await _svc.GetPagedAsync(qp);

            var vm = new RoleIndexMvcViewModel
            {
                Roles = items.ToList(),
                PageNumber = qp.PageNumber,
                PageSize = qp.PageSize,
                TotalItems = total,
                TotalPages = qp.PageSize > 0 ? (int)Math.Ceiling(total / (double)qp.PageSize) : 0,
                Search = qp.Search
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<JsonResult> DetailsJson(int id)
        {
            var role = await _svc.GetByIdAsync(id);
            if (role == null) return Json(null);
            var dto = new
            {
                id = role.Id,
                code = role.Code,
                name = role.Name
            };
            return Json(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel vm, RoleQueryParameters qp)
        {
            qp = qp ?? new RoleQueryParameters();
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index), qp);

            await _svc.CreateAsync(new Role
            {
                Code = vm.Code?.Trim(),
                Name = vm.Name?.Trim()
            });

            return RedirectToAction(nameof(Index), new { PageNumber = 1, PageSize = qp.PageSize, Search = qp.Search });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel vm, RoleQueryParameters qp)
        {
            qp = qp ?? new RoleQueryParameters();
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index), qp);

            var role = await _svc.GetByIdAsync(vm.Id);
            if (role == null) return RedirectToAction(nameof(Index), qp);

            role.Code = vm.Code?.Trim();
            role.Name = vm.Name?.Trim();

            await _svc.UpdateAsync(role);
            return RedirectToAction(nameof(Index), qp);
        }

        // ============================
        // DELETE (desde modal)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int PageNumber = 1, int PageSize = 10, string Search = null)
        {
            // Intentar eliminar
            await _svc.DeleteAsync(id);

            // Ajuste opcional: recalcular página máxima si quieres evitar páginas vacías
            // (puedes omitir este cálculo si prefieres mantener PageNumber tal cual)
            var (itemsAfter, totalAfter) = await _svc.GetPagedAsync(new RoleQueryParameters { PageNumber = 1, PageSize = PageSize, Search = Search });
            var maxPage = PageSize > 0 ? (int)Math.Ceiling(totalAfter / (double)PageSize) : 1;
            if (PageNumber > maxPage) PageNumber = Math.Max(1, maxPage);

            return RedirectToAction(nameof(Index), new { PageNumber, PageSize, Search });
        }
    }
}
