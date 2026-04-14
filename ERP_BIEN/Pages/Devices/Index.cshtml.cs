using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Device> Devices { get; set; } = new();
        public List<User> Users { get; set; } = new();

        // PAGINACIÓN
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // FILTROS GENERALES
        [BindProperty(SupportsGet = true)]
        public string DeviceTypeFilter { get; set; } // Computer, Phone, Screen, DockStation

        [BindProperty(SupportsGet = true)]
        public StatusDevice? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? UserIdFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string HostnameFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ModelFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SNFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ManufacturingFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ManufacturingTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? UseFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? UseTo { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Devices
                .Include(d => d.User)
                .AsQueryable();

            // FILTRO TIPO
            if (!string.IsNullOrWhiteSpace(DeviceTypeFilter))
            {
                query = DeviceTypeFilter switch
                {
                    "Computer" => query.Where(d => d is Computer),
                    "Phone" => query.Where(d => d is Phone),
                    "Screen" => query.Where(d => d is Screen),
                    "DockStation" => query.Where(d => d is DockStation),
                    _ => query
                };
            }

            // FILTRO STATUS
            if (StatusFilter.HasValue)
            {
                query = query.Where(d => d.Status == StatusFilter.Value);
            }

            // FILTRO USUARIO
            if (UserIdFilter.HasValue)
            {
                query = query.Where(d => d.UserId == UserIdFilter.Value);
            }

            // FILTROS TEXTO
            if (!string.IsNullOrWhiteSpace(HostnameFilter))
                query = query.Where(d => d.Hostname.Contains(HostnameFilter));

            if (!string.IsNullOrWhiteSpace(ModelFilter))
                query = query.Where(d => d.Model.Contains(ModelFilter));

            if (!string.IsNullOrWhiteSpace(SNFilter))
                query = query.Where(d => d.SN.Contains(SNFilter));

            // FECHAS
            if (ManufacturingFrom.HasValue)
                query = query.Where(d => d.ManufacturingDate >= ManufacturingFrom.Value);

            if (ManufacturingTo.HasValue)
                query = query.Where(d => d.ManufacturingDate <= ManufacturingTo.Value);

            if (UseFrom.HasValue)
                query = query.Where(d => d.UseDate >= UseFrom.Value);

            if (UseTo.HasValue)
                query = query.Where(d => d.UseDate <= UseTo.Value);

            // TOTAL Y PÁGINAS
            int totalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            if (PageNumber < 1) PageNumber = 1;
            if (TotalPages == 0) PageNumber = 1;
            else if (PageNumber > TotalPages) PageNumber = TotalPages;

            Devices = await query
                .OrderBy(d => d.Hostname)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Users = await _context.Users
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        // ============================
        // CREATE
        // ============================
        public async Task<IActionResult> OnPostCreateAsync(
            string DeviceType,
            string Hostname,
            string SN,
            string Model,
            int NumberOfDevice,
            DateTime? ManufacturingDate,
            StatusDevice Status,
            string Comment,
            DateTime? UseDate,
            int? UserId)
        {
            Device device = DeviceType switch
            {
                "Computer" => new Computer(),
                "Phone" => new Phone(),
                "Screen" => new Screen(),
                "DockStation" => new DockStation(),
                _ => null
            };

            if (device == null)
                return RedirectToPage(new { PageNumber });

            device.Hostname = Hostname?.Trim();
            device.SN = SN?.Trim();
            device.Model = Model?.Trim();
            device.NumberOfDevice = NumberOfDevice;
            device.ManufacturingDate = ManufacturingDate;
            device.Status = Status;
            device.Comment = Comment?.Trim();
            device.UseDate = UseDate;
            device.UserId = UserId;

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { PageNumber });
        }

        // ============================
        // EDIT
        // ============================
        public async Task<IActionResult> OnPostEditAsync(
            int Id,
            string Hostname,
            string SN,
            string Model,
            int NumberOfDevice,
            DateTime? ManufacturingDate,
            StatusDevice Status,
            string Comment,
            DateTime? UseDate,
            int? UserId)
        {
            var device = await _context.Devices.FindAsync(Id);
            if (device == null)
                return RedirectToPage(new { PageNumber });

            device.Hostname = Hostname?.Trim();
            device.SN = SN?.Trim();
            device.Model = Model?.Trim();
            device.NumberOfDevice = NumberOfDevice;
            device.ManufacturingDate = ManufacturingDate;
            device.Status = Status;
            device.Comment = Comment?.Trim();
            device.UseDate = UseDate;
            device.UserId = UserId;

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { PageNumber });
        }

        // ============================
        // DELETE
        // ============================
        public async Task<IActionResult> OnPostDeleteAsync(int Id)
        {
            var device = await _context.Devices.FindAsync(Id);
            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { PageNumber });
        }
    }
}
