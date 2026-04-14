using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ERP_BIEN.Pages.Users
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return RedirectToPage("/Users/Index");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Users/Index");
        }
    }
}
