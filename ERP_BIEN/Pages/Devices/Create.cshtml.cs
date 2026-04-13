using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ERP_BIEN.Pages.Devices
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        // Campos del formulario
        [BindProperty] public string DeviceType { get; set; }
        [BindProperty] public string Hostname { get; set; }
        [BindProperty] public string Model { get; set; }
        [BindProperty] public string SN { get; set; }
        [BindProperty] public StatusDevice Status { get; set; }
        [BindProperty] public string Comment { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Device newDevice = DeviceType switch
            {
                "Computer" => new Computer(),
                "Screen" => new Screen(),
                "Phone" => new Phone(),
                "Ubikey" => new Ubikey(),
                "DockStation" => new DockStation(),
                _ => null
            };

            if (newDevice == null)
            {
                ModelState.AddModelError("", "Tipo de dispositivo no válido.");
                return Page();
            }

            newDevice.Hostname = Hostname;
            newDevice.Model = Model;
            newDevice.SN = SN;
            newDevice.Status = Status;
            newDevice.Comment = Comment;

            _context.Devices.Add(newDevice);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Devices/Index");
        }
    }
}
