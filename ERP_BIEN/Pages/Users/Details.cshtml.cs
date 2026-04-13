using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Users
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public User User { get; set; }

        public IActionResult OnGet(int id)
        {
            User = _context.Users
                .Include(u => u.PersonalInfo)
                .Include(u => u.CompanyInfo)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(x => x.Id == id);

            if (User == null)
                return RedirectToPage("/Users/Index");

            return Page();
        }

        // ============================
        // HANDLER JSON PARA EL MODAL
        // ============================
        public JsonResult OnGetJson(int id)
        {
            var user = _context.Users
                .Include(u => u.PersonalInfo)
                .Include(u => u.CompanyInfo)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(x => x.Id == id);

            if (user == null)
                return new JsonResult(new { error = true });

            string estado =
                user.CompanyInfo?.ContratEndDate == null ||
                user.CompanyInfo.ContratEndDate > DateTime.Today
                ? "Activo"
                : "Inactivo";

            return new JsonResult(new
            {
                id = user.Id,
                name = user.Name,
                lastName = user.LastName,
                email = user.PersonalInfo?.Email,
                domainUser = user.DomainUser,
                roles = string.Join(", ", user.UserRoles.Select(r => r.Role.Name)),
                status = estado
            });
        }
    }
}

