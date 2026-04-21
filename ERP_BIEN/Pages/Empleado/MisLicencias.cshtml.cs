using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Empleado
{
    public class MisLicenciasModel : PageModel
    {
        private readonly AppDbContext _context;

        public MisLicenciasModel(AppDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<License> Licenses { get; private set; } = new List<License>();

        public async Task OnGetAsync()
        {
            var domainUser = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(domainUser))
                return;

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Licenses)
                .SingleOrDefaultAsync(u => u.DomainUser == domainUser);

            if (user == null)
                return;

            Licenses = user.Licenses
                .Where(l => l.Asignada)
                .OrderBy(l => l.Producto)
                .ToList();
        }
    }
}
