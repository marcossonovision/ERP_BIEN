using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
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
        // INDEX
        // ============================
        [Authorize(Policy = "USERS")]
        public IActionResult Index(
            int pageNumber = 1,
            string searchName = null,
            string searchDomain = null,
            int? searchTeamId = null)
        {
            if (pageNumber < 1) pageNumber = 1;

            var result = _service.GetPagedUsers(searchName, searchDomain, searchTeamId, pageNumber);

            if (result.TotalPages > 0 && pageNumber > result.TotalPages)
            {
                pageNumber = result.TotalPages;
                result = _service.GetPagedUsers(searchName, searchDomain, searchTeamId, pageNumber);
            }

            var users = result.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Name = u.Name,
                LastName = u.LastName,
                DomainUser = u.DomainUser,
                TeamId = u.TeamId,
                TeamName = u.Team?.Name
            }).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.SearchName = searchName;
            ViewBag.SearchDomain = searchDomain;
            ViewBag.SearchTeamId = searchTeamId;
            ViewBag.TeamList = _service.GetTeams();

            return View(users);
        }

        // ============================
        // DETAILS
        // ============================
        [Authorize(Policy = "USERS")]
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
        // CREATE
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            UserViewModel model,
            int pageNumber,
            string searchName,
            string searchDomain,
            int? searchTeamId)
        {
            _service.CreateUser(new User
            {
                Name = model.Name,
                LastName = model.LastName,
                DomainUser = model.DomainUser,
                TeamId = model.TeamId
            });

            return RedirectToAction("Index", new
            {
                pageNumber,
                searchName,
                searchDomain,
                searchTeamId
            });
        }

        // ============================
        // EDIT
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            UserViewModel model,
            int pageNumber,
            string searchName,
            string searchDomain,
            int? searchTeamId)
        {
            _service.UpdateUser(new User
            {
                Id = model.Id,
                Name = model.Name,
                LastName = model.LastName,
                DomainUser = model.DomainUser,
                TeamId = model.TeamId
            });

            return RedirectToAction("Index", new
            {
                pageNumber,
                searchName,
                searchDomain,
                searchTeamId
            });
        }

        // ============================
        // DELETE (GET → NUNCA BORRA)
        // ============================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            // Si alguien entra por URL, volvemos al listado
            return RedirectToAction("Index");
        }

        // ============================
        // DELETE (POST REAL)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(
            int id,
            int pageNumber,
            string searchName,
            string searchDomain,
            int? searchTeamId)
        {
            _service.DeleteUser(id);

            return RedirectToAction("Index", new
            {
                pageNumber,
                searchName,
                searchDomain,
                searchTeamId
            });
        }
    }
}
