using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Licenses
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<License> Licenses { get; set; } = new();
        public List<User> Users { get; set; } = new();

        // ============================
        // PAGINACIÓN
        // ============================
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // ============================
        // FILTROS AVANZADOS
        // ============================
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchProveedor { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchProducto { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchAsignada { get; set; }

        // ============================
        // GET PRINCIPAL
        // ============================
        public async Task OnGetAsync()
        {
            var query = _context.Licenses
                .Include(l => l.User)
                .AsQueryable();

            // FILTRO GENERAL
            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(l =>
                    l.Code.Contains(Search) ||
                    l.Producto.Contains(Search) ||
                    l.Proveedor.Contains(Search));
            }

            // FILTRO PROVEEDOR
            if (!string.IsNullOrWhiteSpace(SearchProveedor))
            {
                query = query.Where(l => l.Proveedor.Contains(SearchProveedor));
            }

            // FILTRO PRODUCTO
            if (!string.IsNullOrWhiteSpace(SearchProducto))
            {
                query = query.Where(l => l.Producto.Contains(SearchProducto));
            }

            // FILTRO ASIGNADA
            if (!string.IsNullOrWhiteSpace(SearchAsignada))
            {
                bool asignada = SearchAsignada == "true";
                query = query.Where(l => l.Asignada == asignada);
            }

            // PAGINACIÓN
            int totalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            if (PageNumber < 1) PageNumber = 1;
            if (TotalPages == 0) PageNumber = 1;
            else if (PageNumber > TotalPages) PageNumber = TotalPages;

            Licenses = await query
                .OrderBy(l => l.Producto)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // LISTA DE USUARIOS PARA LOS MODALES
            Users = await _context.Users
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        // ============================
        // DETAILS (DTO SEGURO)
        // ============================
        public async Task<JsonResult> OnGetDetails(int id)
        {
            var lic = await _context.Licenses
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lic == null)
                return new JsonResult(null);

            var dto = new
            {
                id = lic.Id,
                code = lic.Code,
                producto = lic.Producto,
                proveedor = lic.Proveedor,
                price = lic.Price,
                caducidad = lic.Caducidad?.ToString("yyyy-MM-dd"),
                asignada = lic.Asignada,
                disponible = lic.Disponible,
                userId = lic.UserId,
                userName = lic.User != null ? $"{lic.User.Name} {lic.User.LastName}" : null
            };

            return new JsonResult(dto);
        }

        // ============================
        // CREATE
        // ============================
        public async Task<IActionResult> OnPostCreateAsync(
            string Code, string Producto, string Proveedor, string Price,
            DateTime? Caducidad, bool Asignada, bool Disponible, int? UserId)
        {
            var lic = new License
            {
                Code = Code.Trim(),
                Producto = Producto.Trim(),
                Proveedor = Proveedor.Trim(),
                Price = Price.Trim(),
                Caducidad = Caducidad,
                Asignada = Asignada,
                Disponible = Disponible,
                UserId = UserId
            };

            _context.Licenses.Add(lic);
            await _context.SaveChangesAsync();

            return RedirectToPage(new
            {
                PageNumber,
                Search,
                SearchProveedor,
                SearchProducto,
                SearchAsignada
            });
        }

        // ============================
        // EDIT
        // ============================
        public async Task<IActionResult> OnPostEditAsync(
            int Id, string Code, string Producto, string Proveedor, string Price,
            DateTime? Caducidad, bool Asignada, bool Disponible, int? UserId)
        {
            var lic = await _context.Licenses.FindAsync(Id);
            if (lic == null)
            {
                return RedirectToPage(new
                {
                    PageNumber,
                    Search,
                    SearchProveedor,
                    SearchProducto,
                    SearchAsignada
                });
            }

            lic.Code = Code.Trim();
            lic.Producto = Producto.Trim();
            lic.Proveedor = Proveedor.Trim();
            lic.Price = Price.Trim();
            lic.Caducidad = Caducidad;
            lic.Asignada = Asignada;
            lic.Disponible = Disponible;
            lic.UserId = UserId;

            _context.Licenses.Update(lic);
            await _context.SaveChangesAsync();

            return RedirectToPage(new
            {
                PageNumber,
                Search,
                SearchProveedor,
                SearchProducto,
                SearchAsignada
            });
        }

        // ============================
        // DELETE
        // ============================
        public async Task<IActionResult> OnPostDeleteAsync(int Id)
        {
            var lic = await _context.Licenses.FindAsync(Id);
            if (lic != null)
            {
                _context.Licenses.Remove(lic);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new
            {
                PageNumber,
                Search,
                SearchProveedor,
                SearchProducto,
                SearchAsignada
            });
        }
    }
}
