using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_BIEN.Controllers
{
    [Authorize(Policy = "TEAM")] // requiere claim module=TEAM
    public class TeamController : Controller
    {
        private readonly TeamService _teamService;

        public TeamController(TeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? teamId = null, string? msg = null, string? err = null)
        {
            var domainUser = TeamService.NormalizeDomainUser(User.Identity?.Name);
            var currentUser = await _teamService.GetCurrentUserAsync(domainUser);
            if (currentUser == null) return Unauthorized();

            var teams = await _teamService.GetManagedTeamsAsync(currentUser.Id);

            // Si el PM no tiene equipos, mostramos vista vacía con mensaje
            if (teams.Count == 0)
            {
                return View(new TeamModuleIndexViewModel
                {
                    InfoMessage = "No tienes equipos asignados todavía. Pide a RRHH/SuperAdmin que te asigne un equipo o crea uno desde el módulo de equipos (si lo implementas)."
                });
            }

            var selectedTeamId = teamId ?? teams.First().Id;

            var members = await _teamService.GetTeamMembersAsync(selectedTeamId);
            var candidates = await _teamService.GetCandidatesAsync();

            var vm = new TeamModuleIndexViewModel
            {
                SelectedTeamId = selectedTeamId,
                ManagedTeams = teams.Select(t => new TeamItemVm { Id = t.Id, Name = t.Name }).ToList(),
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

        // Añadir miembro (PM puede; está bajo TEAM y además lo marcamos como operación de escritura)
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int selectedTeamId, int userId)
        {
            try
            {
                var domainUser = TeamService.NormalizeDomainUser(User.Identity?.Name);
                var currentUser = await _teamService.GetCurrentUserAsync(domainUser);
                if (currentUser == null) return Unauthorized();

                await _teamService.AddMemberAsync(currentUser.Id, selectedTeamId, userId);
                return RedirectToAction(nameof(Index), new { teamId = selectedTeamId, msg = "Miembro añadido correctamente." });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { teamId = selectedTeamId, err = ex.Message });
            }
        }

        // Quitar miembro
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int selectedTeamId, int userId)
        {
            try
            {
                var domainUser = TeamService.NormalizeDomainUser(User.Identity?.Name);
                var currentUser = await _teamService.GetCurrentUserAsync(domainUser);
                if (currentUser == null) return Unauthorized();

                await _teamService.RemoveMemberAsync(currentUser.Id, userId);
                return RedirectToAction(nameof(Index), new { teamId = selectedTeamId, msg = "Miembro eliminado del equipo." });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { teamId = selectedTeamId, err = ex.Message });
            }
        }
    }
}