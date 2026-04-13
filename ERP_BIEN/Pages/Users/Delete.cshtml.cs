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

        public User User { get; set; }

        public IActionResult OnGet(int id)
        {
            User = _context.Users.FirstOrDefault(x => x.Id == id);

            if (User == null)
                return RedirectToPage("/Users/Index");

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToPage("/Users/Index");
        }
    }
}
