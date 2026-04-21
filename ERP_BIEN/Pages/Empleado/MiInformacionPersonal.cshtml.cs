using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Empleado
{
    public class MiInformacionPersonalModel : PageModel
    {
        private readonly AppDbContext _db;

        public User Empleado { get; set; }

        public MiInformacionPersonalModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var domainUser = User.Identity?.Name;

            if (string.IsNullOrEmpty(domainUser))
                return Unauthorized();

            Empleado = await _db.Users
                .Include(u => u.PersonalInfo)
                .Include(u => u.CompanyInfo)
                .FirstOrDefaultAsync(u => u.DomainUser == domainUser);

            if (Empleado == null || Empleado.PersonalInfo == null)
                return NotFound();

            return Page();
        }
    }
}