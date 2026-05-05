using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
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
    // ACCESO AL MÓDULO ROLES (SOLO ADMIN)
    // ============================
    [Authorize(Policy = "ROLES")]
    [Route("Roles")]
    public class RoleController : Controller
    {
        private readonly IRoleService _svc;

        public RoleController(IRoleService svc)
        {
            _svc = svc;
        }

        // ============================
        // GET – INDEX (LECTURA)
        // URL: /Roles
        // ============================
        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] RoleQueryParameters qp)
        {
            qp ??= new RoleQueryParameters();
            qp.PageNumber = qp.PageNumber <= 0 ? 1 : qp.PageNumber;
            qp.PageSize = qp.PageSize <= 0 ? 10 : qp.PageSize;

            var (items, total) = await _svc.GetPagedAsync(qp);

            var vm = new RoleIndexMvcViewModel
            {
                Roles = items.ToList(),
                PageNumber = qp.PageNumber,
                PageSize = qp.PageSize,
                TotalItems = total,
                TotalPages = qp.PageSize > 0
                    ? (int)Math.Ceiling(total / (double)qp.PageSize)
                    : 0,
                Search = qp.Search
            };

            return View("Index", vm);
        }

        // ============================
        // GET – DETAILS (JSON) (OPCIONAL)
        // URL: /Roles/DetailsJson/5
        // ============================
        [HttpGet("DetailsJson/{id}")]
        public async Task<JsonResult> DetailsJson(int id)
        {
            var role = await _svc.GetByIdAsync(id);
            if (role == null)
                return Json(null);

            return Json(new
            {
                id = role.Id,
                code = role.Code,
                name = role.Name
            });
        }

        // ============================
        // POST – CREATE ✅ (ARREGLADO)
        // URL: /Roles/Create
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string Code,
            string Name,
            RoleQueryParameters qp)
        {
            qp ??= new RoleQueryParameters();

            // Validación mínima (evita ModelState “silencioso”)
            if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Name))
            {
                return RedirectToAction(nameof(Index), new
                {
                    PageNumber = 1,
                    PageSize = qp.PageSize,
                    Search = qp.Search
                });
            }

            await _svc.CreateAsync(new Role
            {
                Code = Code,
                Name = Name
            });

            return RedirectToAction(nameof(Index), new
            {
                PageNumber = 1,
                PageSize = qp.PageSize,
                Search = qp.Search
            });
        }

        // ============================
        // POST – EDIT ✅
        // URL: /Roles/Edit
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel vm, RoleQueryParameters qp)
        {
            qp ??= new RoleQueryParameters();

            // Validación mínima (sin “quedarse colgado”)
            if (vm == null || vm.Id <= 0 || string.IsNullOrWhiteSpace(vm.Code) || string.IsNullOrWhiteSpace(vm.Name))
            {
                return RedirectToAction(nameof(Index), new
                {
                    PageNumber = qp.PageNumber <= 0 ? 1 : qp.PageNumber,
                    PageSize = qp.PageSize <= 0 ? 10 : qp.PageSize,
                    Search = qp.Search
                });
            }

            var role = await _svc.GetByIdAsync(vm.Id);
            if (role == null)
            {
                return RedirectToAction(nameof(Index), new
                {
                    PageNumber = qp.PageNumber <= 0 ? 1 : qp.PageNumber,
                    PageSize = qp.PageSize <= 0 ? 10 : qp.PageSize,
                    Search = qp.Search
                });
            }

            role.Code = vm.Code;
            role.Name = vm.Name;

            await _svc.UpdateAsync(role);

            return RedirectToAction(nameof(Index), new
            {
                PageNumber = qp.PageNumber <= 0 ? 1 : qp.PageNumber,
                PageSize = qp.PageSize <= 0 ? 10 : qp.PageSize,
                Search = qp.Search
            });
        }

        // ============================
        // POST – DELETE
        // URL: /Roles/Delete
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            int PageNumber = 1,
            int PageSize = 10,
            string Search = null)
        {
            await _svc.DeleteAsync(id);

            // Recalcular página máxima para evitar páginas vacías
            var (_, totalAfter) = await _svc.GetPagedAsync(
                new RoleQueryParameters
                {
                    PageNumber = 1,
                    PageSize = PageSize,
                    Search = Search
                });

            var maxPage = PageSize > 0
                ? (int)Math.Ceiling(totalAfter / (double)PageSize)
                : 1;

            if (PageNumber > maxPage)
            {
                PageNumber = Math.Max(1, maxPage);
            }

            return RedirectToAction(nameof(Index), new
            {
                PageNumber,
                PageSize,
                Search
            });
        }
    }
}