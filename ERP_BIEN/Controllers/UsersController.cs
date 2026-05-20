using System;
using System.Linq;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

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
        // INDEX
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
        // DETAILS
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
        // CREATE ✅ CORREGIDO
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
            // ✅ VALIDACIÓN
            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.DomainUser))
            {
                TempData["ToastMsg"] = "Nombre, Apellido y Usuario son obligatorios";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index");
            }

            var normalized = Normalize(model.DomainUser);

            if (_service.DomainUserExists(normalized))
            {
                TempData["ToastMsg"] = "El usuario ya existe";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index");
            }

            try
            {
                var user = _service.CreateUser(new User
                {
                    Name = model.Name.Trim(),
                    LastName = model.LastName.Trim(),
                    DomainUser = normalized,
                    TeamId = model.TeamId,
                    IsActive = true // ✅ siempre activo al crear
                });

                if (model.RoleId.HasValue)
                {
                    _service.UpdateUserRole(user.Id, model.RoleId);
                }

                TempData["ToastMsg"] = $"Usuario creado ({user.DomainUser})";
                TempData["ToastType"] = "success";

                return RedirectToAction("Index", new
                {
                    pageNumber = 1,
                    searchDomain = user.DomainUser
                });
            }
            catch (Exception ex)
            {
                TempData["ToastMsg"] = "ERROR: " + ex.Message;
                TempData["ToastType"] = "error";

                return RedirectToAction("Index");
            }
        }

        // ============================
        // EDIT ✅ CORREGIDO
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
            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.DomainUser))
            {
                TempData["ToastMsg"] = "Campos obligatorios incorrectos";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index");
            }

            var normalized = Normalize(model.DomainUser);

            if (_service.DomainUserExists(normalized, model.Id))
            {
                TempData["ToastMsg"] = "El usuario ya existe";
                TempData["ToastType"] = "error";

                return RedirectToAction("Index");
            }

            try
            {
                _service.UpdateUser(new User
                {
                    Id = model.Id,
                    Name = model.Name.Trim(),
                    LastName = model.LastName.Trim(),
                    DomainUser = normalized,
                    TeamId = model.TeamId,
                    IsActive = model.IsActive
                });

                _service.UpdateUserRole(model.Id, model.RoleId);

                TempData["ToastMsg"] = "Usuario actualizado";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMsg"] = "ERROR: " + ex.Message;
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }

        // ============================
        // DELETE
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
        // TOGGLE ACTIVE
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var newState = _service.ToggleUserActive(id);
            return Json(new { isActive = newState });
        }
        [Authorize(Policy = "USR_VIEW")]
        public IActionResult ExportExcel(
    string searchName = null,
    string searchDomain = null,
    int? searchTeamId = null,
    int? searchRoleId = null,
    bool? searchIsActive = null)
        {
            // Obtener usuarios filtrados
            var result = _service.GetPagedUsers(
                searchName,
                searchDomain,
                searchTeamId,
                1,
                searchRoleId,
                searchIsActive
            );

            var users = result.Users;

            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Usuarios");

            // Headers
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Nombre";
            ws.Cell(1, 3).Value = "Apellido";
            ws.Cell(1, 4).Value = "Usuario";
            ws.Cell(1, 5).Value = "Equipo";
            ws.Cell(1, 6).Value = "Rol";
            ws.Cell(1, 7).Value = "Estado";

            int row = 2;

            foreach (var u in users)
            {
                ws.Cell(row, 1).Value = u.Id;
                ws.Cell(row, 2).Value = u.Name;
                ws.Cell(row, 3).Value = u.LastName;
                ws.Cell(row, 4).Value = u.DomainUser;
                ws.Cell(row, 5).Value = u.Team?.Name ?? "";

                ws.Cell(row, 6).Value =
                    (u.UserRoles != null && u.UserRoles.Any())
                        ? (u.UserRoles.FirstOrDefault()?.Role?.Code ?? "Sin Rol")
                        : "Sin Rol";

                ws.Cell(row, 7).Value = u.IsActive ? "Activo" : "Inactivo";

                row++;
            }

            // Estilo
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Usuarios_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }

        // ============================
        // NORMALIZE ✅ CLAVE PARA RBAC
        // ============================
        private string Normalize(string input)
        {
            var s = input.Trim();

            if (s.Contains("\\"))
                s = s.Split('\\')[1];

            if (s.Contains("@"))
                s = s.Split('@')[0];

            return s.ToLower();
        }
    }
}