using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Services
{
    public class TeamService
    {
        private readonly AppDbContext _db;

        public TeamService(AppDbContext db)
        {
            _db = db;
        }

        public static string NormalizeDomainUser(string? identityName)
        {
            if (string.IsNullOrWhiteSpace(identityName)) return "";
            var s = identityName.Trim();
            if (s.Contains("\\")) s = s.Split('\\')[1];
            if (s.Contains("@")) s = s.Split('@')[0];
            return s.Trim();
        }

        public async Task<User?> GetCurrentUserAsync(string domainUser)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.DomainUser == domainUser);
        }

        /// <summary>
        /// Equipos de los que este usuario es Project Manager
        /// </summary>
        public async Task<List<Team>> GetManagedTeamsAsync(int managerUserId)
        {
            return await _db.Teams
                .Where(t => t.ProjectManagerId == managerUserId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Miembros del equipo
        /// </summary>
        public async Task<List<User>> GetTeamMembersAsync(int teamId)
        {
            return await _db.Users
                .Where(u => u.TeamId == teamId)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Candidatos a añadir (por defecto: usuarios sin equipo)
        /// </summary>
        public async Task<List<User>> GetCandidatesAsync()
        {
            return await _db.Users
                .Where(u => u.TeamId == null)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Añadir usuario a un equipo (solo si el equipo lo gestiona el PM)
        /// </summary>
        public async Task AddMemberAsync(int managerUserId, int teamId, int userId)
        {
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null) throw new InvalidOperationException("Team no existe.");

            if (team.ProjectManagerId != managerUserId)
                throw new UnauthorizedAccessException("No puedes modificar un equipo que no gestionas.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("Usuario no existe.");

            user.TeamId = teamId;
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Quitar usuario del equipo (TeamId = null), solo si el PM gestiona el equipo actual del usuario
        /// </summary>
        public async Task RemoveMemberAsync(int managerUserId, int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("Usuario no existe.");

            if (user.TeamId == null) return;

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == user.TeamId.Value);
            if (team == null) return;

            if (team.ProjectManagerId != managerUserId)
                throw new UnauthorizedAccessException("No puedes modificar un equipo que no gestionas.");

            user.TeamId = null;
            await _db.SaveChangesAsync();
        }
    }
}