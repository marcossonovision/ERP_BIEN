using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public (List<User> Users, int TotalPages) GetPagedEmployees(
            string? searchName,
            string? searchDomain,
            int? searchTeamId,
            int pageNumber,
            int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;

            var query = _context.Users
                .AsNoTracking()
                .Include(u => u.Team)
                .Include(u => u.CompanyInfo)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(u => (u.Name + " " + u.LastName).Contains(searchName));

            if (!string.IsNullOrWhiteSpace(searchDomain))
                query = query.Where(u => (u.DomainUser ?? "").Contains(searchDomain));

            if (searchTeamId.HasValue)
                query = query.Where(u => u.TeamId == searchTeamId);

            int totalUsers = query.Count();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var users = query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (users, totalPages);
        }

        public List<EmployeeViewModel> ToViewModels(IEnumerable<User> users)
        {
            return users.Select(u =>
            {
                var rol = u.UserRoles?
                    .FirstOrDefault()?.Role?.Code ?? "Sin Rol";

                return new EmployeeViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    LastName = u.LastName,
                    DomainUser = u.DomainUser,
                    TeamId = u.TeamId,
                    TeamName = u.Team?.Name,
                    RolPrincipal = rol,
                  
                };
            }).ToList();
        }

        public User? GetEmployee(int id)
        {
            return _context.Users
                .Include(u => u.Team)
                .Include(u => u.CompanyInfo)
                .Include(u => u.PersonalInfo)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == id);
        }

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

        public void CreateEmployee(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateEmployee(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void DeleteEmployee(int id)
        {
            var u = _context.Users.Find(id);
            if (u == null) return;

            _context.Users.Remove(u);
            _context.SaveChanges();
        }
    }
}
