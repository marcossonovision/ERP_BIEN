using System;
using System.Collections.Generic;
using System.Linq;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ✅ Excel (EPPlus)
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ERP_BIEN.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        // ============================
        // INDEX (Rol + Estado)
        // ============================
        [Authorize(Policy = "USR_VIEW")]
        public IActionResult Index(
            int pageNumber = 1,
            string searchName = null,
            string searchDomain = null,
            int? searchTeamId = null,
            int? searchRoleId = null,
            bool? searchIsActive = null)
        {
            if (pageNumber < 1) pageNumber = 1;

            var result = _service.GetPagedUsers(
                searchName,
                searchDomain,
                searchTeamId,
                pageNumber,
                searchRoleId,
                searchIsActive
            );

            // Si se va de rango
            if (result.TotalPages > 0 && pageNumber > result.TotalPages)
            {
                pageNumber = result.TotalPages;
                result = _service.GetPagedUsers(
                    searchName,
                    searchDomain,
                    searchTeamId,
                    pageNumber,
                    searchRoleId,
                    searchIsActive
                );
            }

            var users = result.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Name = u.Name,
                LastName = u.LastName,
                DomainUser = u.DomainUser,
                TeamId = u.TeamId,
                TeamName = u.Team?.Name,
                IsActive = u.IsActive,

                RoleName = (u.UserRoles != null && u.UserRoles.Any())
                    ? (u.UserRoles.FirstOrDefault()?.Role?.Code ?? "Sin Rol")
                    : "Sin Rol",

                RoleId = (u.UserRoles != null && u.UserRoles.Any())
                    ? (int?)u.UserRoles.FirstOrDefault()?.RoleId
                    : null
            }).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = result.TotalPages;

            ViewBag.SearchName = searchName;
            ViewBag.SearchDomain = searchDomain;
            ViewBag.SearchTeamId = searchTeamId;
            ViewBag.SearchRoleId = searchRoleId;
            ViewBag.SearchIsActive = searchIsActive;

            ViewBag.TeamList = _service.GetTeams();
            ViewBag.RoleList = _service.GetRoles();

            return View(users);
        }

        // ============================
        // DETAILS (JSON PARA MODALES)
        // ============================
        [Authorize(Policy = "USR_VIEW")]
        public IActionResult Details(int id)
        {
            var u = _service.GetUser(id);
            if (u == null) return NotFound();

            return Json(new
            {
                id = u.Id,
                name = u.Name,
                lastName = u.LastName,
                domainUser = u.DomainUser,
                teamId = u.TeamId,
                teamName = u.Team?.Name,

                roleName = (u.UserRoles != null && u.UserRoles.Any())
                    ? (u.UserRoles.FirstOrDefault()?.Role?.Code ?? "Sin Rol")
                    : "Sin Rol",

                roleId = (u.UserRoles != null && u.UserRoles.Any())
                    ? (int?)u.UserRoles.FirstOrDefault()?.RoleId
                    : null,

                isActive = u.IsActive
            });
        }

        // ============================
        // CREATE (Rol + Estado)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
    UserViewModel model,
    int pageNumber,
    string searchName,
    string searchDomain,
    int? searchTeamId,
    int? searchRoleId,
    bool? searchIsActive)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMsg"] = "Revisa los campos ❌";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index", new
                {
                    pageNumber,
                    searchName,
                    searchDomain,
                    searchTeamId,
                    searchRoleId,
                    searchIsActive
                });
            }

            if (_service.DomainUserExists(model.DomainUser))
            {
                TempData["ToastMsg"] = "El usuario ya existe ❌";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index", new
                {
                    pageNumber,
                    searchName,
                    searchDomain,
                    searchTeamId,
                    searchRoleId,
                    searchIsActive
                });
            }

            try
            {
                var user = _service.CreateUser(new User
                {
                    Name = model.Name?.Trim(),
                    LastName = model.LastName?.Trim(),
                    DomainUser = model.DomainUser?.Trim(),
                    TeamId = model.TeamId,
                    IsActive = model.IsActive
                });

                // ✅ CONFIRMACIÓN: si no hay ID, no se guardó
                if (user == null || user.Id <= 0)
                    throw new Exception("No se ha generado Id (no se guardó en BD).");

                // ✅ NO romper si RoleId viene null
                if (model.RoleId.HasValue)
                {
                    _service.UpdateUserRole(user.Id, model.RoleId);
                }

                TempData["ToastMsg"] = $"Usuario creado ✅ ({user.DomainUser})";
                TempData["ToastType"] = "success";

                // ✅ CLAVE: redirigir filtrando por el usuario creado para VERLO sí o sí
                return RedirectToAction("Index", new
                {
                    pageNumber = 1,
                    searchName = (string)null,
                    searchDomain = user.DomainUser,   // 👈 esto hace que aparezca en la tabla
                    searchTeamId = (int?)null,
                    searchRoleId = (int?)null,
                    searchIsActive = (bool?)null
                });
            }
            catch (Exception ex)
            {
                // ✅ Ver el error real en Output/Console del servidor
                Console.WriteLine("ERROR CREATE USER: " + ex);

                TempData["ToastMsg"] = "Error al crear usuario ❌: " + ex.Message;
                TempData["ToastType"] = "error";

                return RedirectToAction("Index", new
                {
                    pageNumber,
                    searchName,
                    searchDomain,
                    searchTeamId,
                    searchRoleId,
                    searchIsActive
                });
            }
        }

        // ============================
        // EDIT (Rol + Estado)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
    UserViewModel model,
    int pageNumber,
    string searchName,
    string searchDomain,
    int? searchTeamId,
    int? searchRoleId,
    bool? searchIsActive)
        {
            // ✅ VALIDACIÓN
            if (!ModelState.IsValid)
            {
                TempData["ToastMsg"] = "Revisa los campos ❌";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index", new
                {
                    pageNumber,
                    searchName,
                    searchDomain,
                    searchTeamId,
                    searchRoleId,
                    searchIsActive
                });
            }

            // ✅ DUPLICADO (excluyendo el propio usuario)
            if (_service.DomainUserExists(model.DomainUser, model.Id))
            {
                TempData["ToastMsg"] = "El usuario ya existe ❌";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index", new
                {
                    pageNumber,
                    searchName,
                    searchDomain,
                    searchTeamId,
                    searchRoleId,
                    searchIsActive
                });
            }

            try
            {
                _service.UpdateUser(new User
                {
                    Id = model.Id,
                    Name = model.Name?.Trim(),
                    LastName = model.LastName?.Trim(),
                    DomainUser = model.DomainUser?.Trim(),
                    TeamId = model.TeamId,
                    IsActive = model.IsActive
                });

                _service.UpdateUserRole(model.Id, model.RoleId);

                TempData["ToastMsg"] = "Usuario actualizado ✅";
                TempData["ToastType"] = "success";
            }
            catch
            {
                TempData["ToastMsg"] = "Error al actualizar ❌";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index", new
            {
                pageNumber,
                searchName,
                searchDomain,
                searchTeamId,
                searchRoleId,
                searchIsActive
            });
        }

        // ============================
        // DELETE (AJAX)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete([FromBody] DeleteUserRequest request)
        {
            if (request == null || request.Id <= 0)
                return BadRequest();

            _service.DeleteUser(request.Id);
            return Ok();
        }

        public class DeleteUserRequest
        {
            public int Id { get; set; }
        }

        // ============================
        // TOGGLE ESTADO (AJAX)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var newState = _service.ToggleUserActive(id);
            return Json(new { isActive = newState });
        }

        // ============================
        // ✅ EXPORTAR A EXCEL PRO (RESPETA FILTROS)
        // ============================
        [Authorize(Policy = "USR_VIEW")]
        [HttpGet]
        public IActionResult ExportExcel(
            string searchName = null,
            string searchDomain = null,
            int? searchTeamId = null,
            int? searchRoleId = null,
            bool? searchIsActive = null)
        {
            // Traemos todos los registros filtrados
            var result = _service.GetPagedUsers(
                searchName,
                searchDomain,
                searchTeamId,
                pageNumber: 1,
                searchRoleId: searchRoleId,
                searchIsActive: searchIsActive,
                pageSize: 50000
            );

            var users = result.Users;

            // ✅ EPPlus 8+ (licencia correcta)
            ExcelPackage.License.SetNonCommercialPersonal("Marcos");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Usuarios");

            // =========================
            // CABECERA PRO (TÍTULO + FILTROS)
            // =========================
            ws.Cells["A1:E1"].Merge = true;
            ws.Cells["A1"].Value = "ERP BIEN — Exportación de Usuarios";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.Font.Size = 16;
            ws.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(13, 110, 253)); // azul fuerte
            ws.Row(1).Height = 28;

            ws.Cells["A2"].Value = "Generado:";
            ws.Cells["B2"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            ws.Cells["A3"].Value = "Filtros:";
            ws.Cells["B3"].Value = BuildFiltersSummary(searchName, searchDomain, searchTeamId, searchRoleId, searchIsActive);

            ws.Cells["A2:A3"].Style.Font.Bold = true;
            ws.Cells["B3"].Style.WrapText = true;
            ws.Row(3).Height = 30;

            // =========================
            // CABECERA TABLA
            // =========================
            const int headerRow = 5;

            ws.Cells[headerRow, 1].Value = "Nombre";
            ws.Cells[headerRow, 2].Value = "Usuario";
            ws.Cells[headerRow, 3].Value = "Equipo";
            ws.Cells[headerRow, 4].Value = "Rol";
            ws.Cells[headerRow, 5].Value = "Estado";

            using (var range = ws.Cells[headerRow, 1, headerRow, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(33, 37, 41)); // gris oscuro
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            ws.Row(headerRow).Height = 20;

            // =========================
            // DATOS + ESTILO PRO
            // =========================
            int row = headerRow + 1;

            foreach (var u in users)
            {
                var role = (u.UserRoles != null && u.UserRoles.Any())
                    ? (u.UserRoles.FirstOrDefault()?.Role?.Code ?? "Sin Rol")
                    : "Sin Rol";

                ws.Cells[row, 1].Value = $"{u.Name} {u.LastName}".Trim();
                ws.Cells[row, 2].Value = u.DomainUser ?? "";
                ws.Cells[row, 3].Value = u.Team?.Name ?? "-";
                ws.Cells[row, 4].Value = role;
                ws.Cells[row, 5].Value = u.IsActive ? "Activo" : "Inactivo";

                // Zebra
                if ((row - headerRow) % 2 == 0)
                {
                    using var r = ws.Cells[row, 1, row, 5];
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 249, 250));
                }

                // Estado colores fuertes
                var stateCell = ws.Cells[row, 5];
                stateCell.Style.Fill.PatternType = ExcelFillStyle.Solid;

                if (u.IsActive)
                {
                    stateCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(25, 135, 84)); // verde fuerte
                }
                else
                {
                    stateCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 53, 69)); // rojo fuerte
                }

                stateCell.Style.Font.Color.SetColor(Color.White);
                stateCell.Style.Font.Bold = true;
                stateCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row++;
            }

            int lastRow = row - 1;

            // Bordes tabla
            if (lastRow >= headerRow + 1)
            {
                using var tableRange = ws.Cells[headerRow, 1, lastRow, 5];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            // Autofiltro + Freeze
            ws.View.FreezePanes(headerRow + 1, 1);
            ws.Cells[headerRow, 1, Math.Max(lastRow, headerRow), 5].AutoFilter = true;

            // Anchos pro
            ws.Column(1).Width = 28;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 26;
            ws.Column(4).Width = 12;
            ws.Column(5).Width = 12;

            var fileName = $"Usuarios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            var bytes = package.GetAsByteArray();

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );

            // -------------------------
            // Helper local: resumen filtros (con nombres de Team/Rol si existen en listas)
            // -------------------------
            string BuildFiltersSummary(string n, string d, int? teamId, int? roleId, bool? active)
            {
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(n)) parts.Add($"Nombre='{n}'");
                if (!string.IsNullOrWhiteSpace(d)) parts.Add($"Usuario='{d}'");

                if (teamId.HasValue)
                {
                    var teams = _service.GetTeams();
                    var teamName = teams.FirstOrDefault(t => t.Value == teamId.Value.ToString())?.Text;
                    parts.Add($"Equipo='{teamName ?? teamId.Value.ToString()}'");
                }

                if (roleId.HasValue)
                {
                    var roles = _service.GetRoles();
                    var roleName = roles.FirstOrDefault(r => r.Value == roleId.Value.ToString())?.Text;
                    parts.Add($"Rol='{roleName ?? roleId.Value.ToString()}'");
                }

                if (active.HasValue)
                {
                    parts.Add($"Estado={(active.Value ? "Activo" : "Inactivo")}");
                }

                return parts.Count == 0
                    ? "Sin filtros (todos los usuarios)"
                    : string.Join(" | ", parts);
            }
        }
    }
}
