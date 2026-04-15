using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Mvc;

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
        // INDEX (LISTA + FILTROS + PÁGINA)
        // ============================
        public IActionResult Index(
            int pageNumber = 1,
            string searchName = null,
            string searchDomain = null,
            int? searchTeamId = null)
        {
            var result = _service.GetPagedUsers(searchName, searchDomain, searchTeamId, pageNumber);

            // Mapeo entidad → ViewModel
            var users = result.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Name = u.Name,
                LastName = u.LastName,
                DomainUser = u.DomainUser,
                TeamId = u.TeamId,
                TeamName = u.Team?.Name
            }).ToList();

            // Paginación
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = result.TotalPages;

            // Filtros
            ViewBag.SearchName = searchName;
            ViewBag.SearchDomain = searchDomain;
            ViewBag.SearchTeamId = searchTeamId;

            // Equipos
            ViewBag.TeamList = _service.GetTeams();

            return View(users);
        }

        // ============================
        // DETALLES (JSON PARA MODALES)
        // ============================
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
                teamName = u.Team?.Name
            });
        }

        // ============================
        // CREAR
        // ============================
        [HttpPost]
        public IActionResult Create(UserViewModel model)
        {
            _service.CreateUser(new User
            {
                Name = model.Name,
                LastName = model.LastName,
                DomainUser = model.DomainUser,
                TeamId = model.TeamId
            });

            return RedirectToAction("Index");
        }

        // ============================
        // EDITAR
        // ============================
        [HttpPost]
        public IActionResult Edit(UserViewModel model)
        {
            _service.UpdateUser(new User
            {
                Id = model.Id,
                Name = model.Name,
                LastName = model.LastName,
                DomainUser = model.DomainUser,
                TeamId = model.TeamId
            });

            return RedirectToAction("Index");
        }

        // ============================
        // ELIMINAR
        // ============================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            _service.DeleteUser(id);
            return RedirectToAction("Index");
        }
    }
}
