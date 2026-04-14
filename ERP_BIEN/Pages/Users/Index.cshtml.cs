using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;

namespace ERP_BIEN.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) => _context = context;

        public List<User> Users { get; set; }
        public List<SelectListItem> TeamList { get; set; }

        // FILTROS
        [BindProperty(SupportsGet = true)]
        public string SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchDomain { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTeamId { get; set; }

        // PAGINACIÓN
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 10;

        public async Task OnGet(int pageNumber = 1)
        {
            PageNumber = pageNumber;

            var query = _context.Users
                .Include(u => u.Team)
                .AsQueryable();

            // FILTROS
            if (!string.IsNullOrWhiteSpace(SearchName))
                query = query.Where(u => u.Name.Contains(SearchName) || u.LastName.Contains(SearchName));

            if (!string.IsNullOrWhiteSpace(SearchDomain))
                query = query.Where(u => u.DomainUser.Contains(SearchDomain));

            if (!string.IsNullOrWhiteSpace(SearchTeamId))
                query = query.Where(u => u.TeamId.ToString() == SearchTeamId);

            int totalUsers = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

            Users = await query
                .OrderBy(u => u.LastName)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            TeamList = await _context.Teams
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                .ToListAsync();
        }

        // DETALLES (DTO seguro)
        public async Task<JsonResult> OnGetDetails(int id)
        {
            var user = await _context.Users
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return new JsonResult(null);

            var dto = new
            {
                id = user.Id,
                name = user.Name,
                lastName = user.LastName,
                domainUser = user.DomainUser,
                teamId = user.TeamId,
                teamName = user.Team?.Name
            };

            return new JsonResult(dto);
        }

        // CREAR
        public async Task<IActionResult> OnPostCreate(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // EDITAR
        public async Task<IActionResult> OnPostEdit(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // ELIMINAR
        public async Task<IActionResult> OnPostDelete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // EXPORTAR A EXCEL
        public async Task<IActionResult> OnGetExportExcel()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var users = await _context.Users.Include(u => u.Team).ToListAsync();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Usuarios");

            ws.Cells["A1"].Value = "Nombre";
            ws.Cells["B1"].Value = "Usuario";
            ws.Cells["C1"].Value = "Equipo";

            int row = 2;
            foreach (var u in users)
            {
                ws.Cells[row, 1].Value = $"{u.Name} {u.LastName}";
                ws.Cells[row, 2].Value = u.DomainUser;
                ws.Cells[row, 3].Value = u.Team?.Name;
                row++;
            }

            var bytes = package.GetAsByteArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Usuarios.xlsx");
        }
    }
}
