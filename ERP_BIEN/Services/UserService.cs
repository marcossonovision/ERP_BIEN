using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================
        // PAGINACIÓN + FILTROS
        // ============================================
        public (List<User> Users, int TotalPages) GetPagedUsers(
            string searchName,
            string searchDomain,
            int? searchTeamId,
            int pageNumber,
            int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;

            var query = _context.Users
                .Include(u => u.Team)
                .AsQueryable();

            // ============================
            // FILTRO POR NOMBRE / APELLIDO
            // (prefijo, en orden)
            // ============================
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var nameFilter = searchName.Trim().ToLower();

                query = query.Where(u =>
                    (u.Name != null && u.Name.ToLower().StartsWith(nameFilter)) ||
                    (u.LastName != null && u.LastName.ToLower().StartsWith(nameFilter))
                );
            }

            // ============================
            // FILTRO POR USUARIO DE DOMINIO
            // (prefijo)
            // ============================
            if (!string.IsNullOrWhiteSpace(searchDomain))
            {
                var domainFilter = searchDomain.Trim().ToLower();

                query = query.Where(u =>
                    u.DomainUser != null &&
                    u.DomainUser.ToLower().StartsWith(domainFilter)
                );
            }

            // ============================
            // FILTRO POR EQUIPO
            // ============================
            if (searchTeamId.HasValue)
            {
                query = query.Where(u => u.TeamId == searchTeamId.Value);
            }

            // ============================
            // TOTAL DE PÁGINAS
            // ============================
            int totalUsers = query.Count();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
                pageNumber = totalPages;

            // ============================
            // PAGINACIÓN
            // ============================
            var users = query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (users, totalPages);
        }

        // ============================================
        // OBTENER UN USUARIO
        // ============================================
        public User GetUser(int id)
        {
            return _context.Users
                .Include(u => u.Team)
                .FirstOrDefault(u => u.Id == id);
        }

        // ============================================
        // CREAR USUARIO
        // (tal y como lo tienes ahora)
        // ============================================
        public void CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        // ============================================
        // EDITAR USUARIO (CORRECTO)
        // ============================================
        public void UpdateUser(User updated)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Id == updated.Id);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            user.Name = updated.Name;
            user.LastName = updated.LastName;
            user.DomainUser = updated.DomainUser;
            user.TeamId = updated.TeamId;

            _context.SaveChanges();
        }

        // ============================================
        // ELIMINAR USUARIO (HARD DELETE REAL)
        // ============================================
        public void DeleteUser(int id)
        {
            var user = _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Devices)
                .Include(u => u.Licenses)
                .Include(u => u.PersonalInfo)
                .Include(u => u.CompanyInfo)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return;

            if (user.UserRoles != null && user.UserRoles.Any())
                _context.UserRoles.RemoveRange(user.UserRoles);

            if (user.Devices != null && user.Devices.Any())
                _context.Devices.RemoveRange(user.Devices);

            if (user.Licenses != null && user.Licenses.Any())
                _context.Licenses.RemoveRange(user.Licenses);

            if (user.PersonalInfo != null)
                _context.PersonalInformation.Remove(user.PersonalInfo);

            if (user.CompanyInfo != null)
                _context.CompanyInformation.Remove(user.CompanyInfo);

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        // ============================================
        // LISTA DE EQUIPOS PARA SELECT
        // ============================================
        public List<SelectItem> GetTeams()
        {
            return _context.Teams
                .OrderBy(t => t.Name)
                .Select(t => new SelectItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToList();
        }
    }

    // ============================================
    // MODELO PARA SELECT
    // ============================================
    public class SelectItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}
