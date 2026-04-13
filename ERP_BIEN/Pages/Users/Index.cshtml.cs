using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<User> Users { get; set; } = new();
        public List<Role> AllRoles { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        public async Task OnGetAsync(int page = 1)
        {
            int pageSize = 10;
            CurrentPage = page;

            AllRoles = await _context.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            var query = _context.Users
                .Include(u => u.PersonalInfo)
                .Include(u => u.CompanyInfo)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // BÚSQUEDA
            if (!string.IsNullOrEmpty(Search))
            {
                var search = Search.ToLower();

                query = query.Where(u =>
                    u.Name.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    u.PersonalInfo.Email.ToLower().Contains(search) ||
                    u.PersonalInfo.DNI.ToLower().Contains(search) ||
                    u.DomainUser.ToLower().Contains(search)
                );
            }

            // FILTRO POR ROL
            if (!string.IsNullOrEmpty(RoleFilter))
            {
                query = query.Where(u =>
                    u.UserRoles.Any(ur => ur.Role.Name == RoleFilter));
            }

            // FILTRO POR ESTADO
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                if (StatusFilter == "Activo")
                {
                    query = query.Where(u =>
                        u.CompanyInfo.ContratEndDate == null ||
                        u.CompanyInfo.ContratEndDate > DateTime.Today);
                }
                else if (StatusFilter == "Inactivo")
                {
                    query = query.Where(u =>
                        u.CompanyInfo.ContratEndDate != null &&
                        u.CompanyInfo.ContratEndDate <= DateTime.Today);
                }
            }

            int totalUsers = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            Users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public string GetEstado(User u)
        {
            if (u.CompanyInfo?.ContratEndDate == null ||
                u.CompanyInfo.ContratEndDate > DateTime.Today)
                return "Activo";

            return "Inactivo";
        }
    }
}
