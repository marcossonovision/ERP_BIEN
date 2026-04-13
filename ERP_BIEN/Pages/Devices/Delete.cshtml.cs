using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Devices
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        // Datos para mostrar en la vista
        public string DeviceType { get; set; }
        public string Model { get; set; }
        public string SN { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }

        [BindProperty]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            var device = await _context.Devices
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return RedirectToPage("/Devices/Index");

            DeviceType = device.GetType().Name;
            Model = device.Model;
            SN = device.SN;
            Status = device.Status.ToString();
            UserName = device.User != null ? device.User.Name + " " + device.User.LastName : "—";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var device = await _context.Devices.FindAsync(Id);

            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Devices/Index");
        }
    }
}
