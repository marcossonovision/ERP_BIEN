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

            // FILTROS
            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(u => (u.Name + " " + u.LastName).Contains(searchName));

            if (!string.IsNullOrWhiteSpace(searchDomain))
                query = query.Where(u => u.DomainUser.Contains(searchDomain));

            if (searchTeamId.HasValue)
                query = query.Where(u => u.TeamId == searchTeamId);

            // TOTAL DE PÁGINAS
            int totalUsers = query.Count();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            // PAGINACIÓN
            var users = query
                .OrderBy(u => u.Name)
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
        // ============================================
        public void CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        // ============================================
        // EDITAR USUARIO
        // ============================================
        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        // ============================================
        // ELIMINAR USUARIO
        // ============================================
        public void DeleteUser(int id)
        {
            var u = _context.Users.Find(id);
            if (u != null)
            {
                _context.Users.Remove(u);
                _context.SaveChanges();
            }
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
