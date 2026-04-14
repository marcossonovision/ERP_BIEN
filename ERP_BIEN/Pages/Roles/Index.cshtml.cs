using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Role> Roles { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(r =>
                    r.Name.Contains(Search) ||
                    r.Code.Contains(Search));
            }

            Roles = await query.OrderBy(r => r.Name).ToListAsync();
        }

        // ============================
        // CREATE
        // ============================
        public async Task<IActionResult> OnPostCreateAsync(string Code, string Name)
        {
            if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Name))
                return RedirectToPage();

            var role = new Role
            {
                Code = Code.Trim(),
                Name = Name.Trim()
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // ============================
        // EDIT
        // ============================
        public async Task<IActionResult> OnPostEditAsync(int Id, string Code, string Name)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == Id);

            if (role == null)
                return RedirectToPage();

            role.Code = Code.Trim();
            role.Name = Name.Trim();

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // ============================
        // DELETE
        // ============================
        public async Task<IActionResult> OnPostDeleteAsync(int Id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == Id);

            if (role == null)
                return RedirectToPage();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
