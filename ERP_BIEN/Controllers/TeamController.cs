using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Controllers
{
    [Authorize(Policy = "TEAM")]
    public class TeamController : Controller
    {
        private readonly TeamService _teamService;
        private readonly AppDbContext _context;

        public TeamController(TeamService teamService, AppDbContext context)
        {
            _teamService = teamService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? teamId = null, string? msg = null, string? err = null)
        {
            // ===== Usuario actual =====
            var domainUser = TeamService.NormalizeDomainUser(User.Identity?.Name);
            var currentUser = await _teamService.GetCurrentUserAsync(domainUser);
            if (currentUser == null)
                return Unauthorized();

            // ===== Equipos que gestiono =====
            var teams = await _teamService.GetManagedTeamsAsync(currentUser.Id);

            // ✅ SI NO TENGO EQUIPO → SE CREA AUTOMÁTICAMENTE
            if (teams.Count == 0)
            {
                var newTeam = new Team
                {
                    Name = $"Equipo de {currentUser.Name} {currentUser.LastName}",
                    ProjectManagerId = currentUser.Id
                };

                _context.Teams.Add(newTeam);
                await _context.SaveChangesAsync();

                teams.Add(newTeam);
            }

            // ===== Equipo seleccionado =====
            var selectedTeamId = teamId ?? teams.First().Id;

            // ===== Miembros del equipo =====
            var members = await _context.Users
                .Where(u => u.TeamId == selectedTeamId)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.Name)
                .ToListAsync();

            // ===== Usuarios disponibles (NO en este equipo) =====
            var candidates = await _context.Users
                .Where(u => u.TeamId == null || u.TeamId != selectedTeamId)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.Name)
                .ToListAsync();

            // ===== ViewModel =====
            var vm = new TeamModuleIndexViewModel
            {
                SelectedTeamId = selectedTeamId,
                ManagedTeams = teams.Select(t => new TeamItemVm
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList(),

                Members = members.Select(u => new UserItemVm
                {
                    Id = u.Id,
                    FullName = $"{u.Name} {u.LastName}".Trim(),
                    DomainUser = u.DomainUser ?? ""
                }).ToList(),

                Candidates = candidates.Select(u => new UserItemVm
                {
                    Id = u.Id,
                    FullName = $"{u.Name} {u.LastName}".Trim(),
                    DomainUser = u.DomainUser ?? ""
                }).ToList(),

                InfoMessage = msg,
                ErrorMessage = err
            };

            return View(vm);
        }

        // ===== AÑADIR MIEMBRO =====
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int selectedTeamId, int userId)
        {
            try
            {
                var domainUser = TeamService.NormalizeDomainUser(User.Identity?.Name);
                var currentUser = await _teamService.GetCurrentUserAsync(domainUser);
                if (currentUser == null)
                    return Unauthorized();

                await _teamService.AddMemberAsync(currentUser.Id, selectedTeamId, userId);

                return RedirectToAction(nameof(Index),
                    new { teamId = selectedTeamId, msg = "Miembro añadido correctamente." });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index),
                    new { teamId = selectedTeamId, err = ex.Message });
            }
        }

        // ===== QUITAR MIEMBRO =====
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int selectedTeamId, int userId)
        {
            try
            {
                var domainUser = TeamService.NormalizeDomainUser(User.Identity?.Name);
                var currentUser = await _teamService.GetCurrentUserAsync(domainUser);
                if (currentUser == null)
                    return Unauthorized();

                await _teamService.RemoveMemberAsync(currentUser.Id, userId);

                return RedirectToAction(nameof(Index),
                    new { teamId = selectedTeamId, msg = "Miembro eliminado del equipo." });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index),
                    new { teamId = selectedTeamId, err = ex.Message });
            }
        }
    }
}
