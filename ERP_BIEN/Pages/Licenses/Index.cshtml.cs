using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ERP_BIEN.Pages.Licenses
{
    public class IndexModel : PageModel
    {
        public class LicenseVM
        {
            public int Id { get; set; }
            public string Product { get; set; }
            public string Key { get; set; }
            public string AssignedTo { get; set; }
            public LicenseStatus Status { get; set; }
        }

        public List<LicenseVM> Licenses { get; set; }

        [BindProperty(SupportsGet = true)] public string Search { get; set; }
        [BindProperty(SupportsGet = true)] public string ProductFilter { get; set; }
        [BindProperty(SupportsGet = true)] public LicenseStatus? StatusFilter { get; set; }

        public List<string> AllProducts { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = page;
            int pageSize = 5;

            // ===== DATOS EXACTOS DE LA IMAGEN =====
            var all = new List<LicenseVM>
            {
                new() { Id = 1, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Carlos Martín", Status = LicenseStatus.Disponible },
                new() { Id = 2, Product = "Adobe Photoshop", Key = "****-****-**PS", AssignedTo = "Angel Díaz", Status = LicenseStatus.Activo },
                new() { Id = 3, Product = "AutoCAD", Key = "****-****-**AC", AssignedTo = "Maria Belén", Status = LicenseStatus.Expirada },
                new() { Id = 4, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Marcos Gutierrez", Status = LicenseStatus.Activo },
                new() { Id = 5, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Carlos Martín", Status = LicenseStatus.Expirada },
                new() { Id = 6, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Carlos Martín", Status = LicenseStatus.Activo }
            };

            AllProducts = all.Select(x => x.Product).Distinct().ToList();

            // ===== FILTRO POR PRODUCTO =====
            if (!string.IsNullOrEmpty(ProductFilter))
                all = all.Where(x => x.Product == ProductFilter).ToList();

            // ===== FILTRO POR ESTADO =====
            if (StatusFilter.HasValue)
                all = all.Where(x => x.Status == StatusFilter.Value).ToList();

            // ===== FILTRO POR TEXTO =====
            if (!string.IsNullOrEmpty(Search))
            {
                var s = Search.ToLower();
                all = all.Where(x => x.AssignedTo.ToLower().Contains(s)).ToList();
            }

            // ===== PAGINACIÓN =====
            int total = all.Count;
            TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            Licenses = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }

    public enum LicenseStatus
    {
        Disponible,
        Activo,
        Expirada
    }
}
