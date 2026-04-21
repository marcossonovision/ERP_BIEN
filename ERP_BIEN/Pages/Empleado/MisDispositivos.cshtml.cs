using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Empleado
{
    public class MisDispositivosModel : PageModel
    {
        private readonly AppDbContext _context;

        public MisDispositivosModel(AppDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<Device> Devices { get; private set; } = new List<Device>();

        public bool UserFound { get; private set; }
        public string? DomainUser { get; private set; }

        public async Task OnGetAsync()
        {
            DomainUser = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(DomainUser))
            {
                UserFound = false;
                return;
            }

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Devices)
                .SingleOrDefaultAsync(u => u.DomainUser == DomainUser);

            if (user == null)
            {
                UserFound = false;
                return;
            }

            UserFound = true;

            Devices = user.Devices?
                .OrderBy(d => d.Hostname)
                .ToList()
                ?? new List<Device>();
        }
    }
}