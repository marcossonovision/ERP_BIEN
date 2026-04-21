using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Empleado
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        // ✅ Usuario cargado desde la BD
        public User Empleado { get; set; }

        // ✅ Rol principal para mostrar en la card
        public string RolPrincipal { get; set; } = "Sin Rol";

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task OnGet()
        {
            // ✅ Usuario del SSO Windows (CORP\adrian.corraliza)
            var domainUser = User.Identity?.Name;

            // ✅ Carga completa del empleado
            Empleado = await _db.Users
                .Include(u => u.Team)
                .Include(u => u.PersonalInfo)
                .Include(u => u.CompanyInfo)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.DomainUser == domainUser);

            // ✅ Si no existe, evitar error
            if (Empleado == null)
            {
                Empleado = new User
                {
                    Name = "Usuario",
                    LastName = "No encontrado",
                    DomainUser = domainUser
                };
                return;
            }

            // ✅ Extraer Rol principal o dejar uno por defecto
            RolPrincipal = Empleado.UserRoles?
                .FirstOrDefault()?.Role?.Code ?? "Sin Rol";
        }
    }
}