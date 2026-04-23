using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ERP_BIEN.Controllers
{
    // ============================
    // ACCESO AL MÓDULO EMPLOYEES
    // ============================
    [Authorize(Policy = "EMPLOYEES")]
    public class EmployeesController : Controller
    {
        private readonly EmployeeService _service;

        public EmployeesController(AppDbContext db)
        {
            _service = new EmployeeService(db);
        }

        // ============================
        // GET – LISTADO
        // ============================
        public IActionResult Index(
            int pageNumber = 1,
            string? searchName = null,
            string? searchDomain = null,
            int? searchTeamId = null)
        {
            if (pageNumber < 1) pageNumber = 1;

            var result = _service.GetPagedEmployees(searchName, searchDomain, searchTeamId, pageNumber);

            if (result.TotalPages > 0 && pageNumber > result.TotalPages)
            {
                pageNumber = result.TotalPages;
                result = _service.GetPagedEmployees(searchName, searchDomain, searchTeamId, pageNumber);
            }

            var employees = _service.ToViewModels(result.Users);

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = result.TotalPages;

            ViewBag.SearchName = searchName;
            ViewBag.SearchDomain = searchDomain;
            ViewBag.SearchTeamId = searchTeamId;

            ViewBag.TeamList = _service.GetTeams();

            return View(employees);
        }

        // ============================
        // GET – DETAILS (LECTURA)
        // ============================
        public IActionResult Details(int id)
        {
            var u = _service.GetEmployee(id);
            if (u == null) return NotFound();

            var rol = u.UserRoles?
                .FirstOrDefault()?.Role?.Code ?? "Sin Rol";

            return Json(new
            {
                id = u.Id,
                name = u.Name,
                lastName = u.LastName,
                domainUser = u.DomainUser,
                teamId = u.TeamId,
                teamName = u.Team?.Name,
                rolPrincipal = rol,
            });
        }

        // ============================
        // POST – CREATE (ESCRITURA)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            EmployeeViewModel model,
            int pageNumber,
            string? searchName,
            string? searchDomain,
            int? searchTeamId)
        {
            _service.CreateEmployee(new User
            {
                Name = model.Name ?? "",
                LastName = model.LastName ?? "",
                DomainUser = model.DomainUser,
                TeamId = model.TeamId
            });

            return RedirectToAction("Index", new
            {
                pageNumber,
                searchName,
                searchDomain,
                searchTeamId
            });
        }

        // ============================
        // POST – EDIT (ESCRITURA)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            EmployeeViewModel model,
            int pageNumber,
            string? searchName,
            string? searchDomain,
            int? searchTeamId)
        {
            _service.UpdateEmployee(new User
            {
                Id = model.Id,
                Name = model.Name ?? "",
                LastName = model.LastName ?? "",
                DomainUser = model.DomainUser,
                TeamId = model.TeamId
            });

            return RedirectToAction("Index", new
            {
                pageNumber,
                searchName,
                searchDomain,
                searchTeamId
            });
        }

        // ============================
        // POST – DELETE (ESCRITURA)
        // ============================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(
            int id,
            int pageNumber,
            string? searchName,
            string? searchDomain,
            int? searchTeamId)
        {
            _service.DeleteEmployee(id);

            return RedirectToAction("Index", new
            {
                pageNumber,
                searchName,
                searchDomain,
                searchTeamId
            });
        }

        // ============================
        // GET – EXPORT EXCEL (LECTURA)
        // ============================
        public IActionResult ExportExcel(
            string? searchName = null,
            string? searchDomain = null,
            int? searchTeamId = null)
        {
            var result = _service.GetPagedEmployees(
                searchName,
                searchDomain,
                searchTeamId,
                pageNumber: 1,
                pageSize: 100000);

            var rows = _service.ToViewModels(result.Users);

            var xml = BuildSpreadsheetXml(rows);
            var bytes = Encoding.UTF8.GetBytes(xml);
            var fileName = $"Employees_{DateTime.Now:yyyyMMdd_HHmm}.xls";

            return File(bytes, "application/vnd.ms-excel", fileName);
        }

        private static string EscapeXml(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static string BuildSpreadsheetXml(IEnumerable<EmployeeViewModel> rows)
        {
            var sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Workbook xmlns=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:o=""urn:schemas-microsoft-com:office:office""
 xmlns:x=""urn:schemas-microsoft-com:office:excel""
 xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:html=""http://www.w3.org/TR/REC-html40"">
  <Worksheet ss:Name=""Employees"">
    <Table>");

            void Cell(string v) =>
                sb.Append($@"<Cell><Data ss:Type=""String"">{EscapeXml(v)}</Data></Cell>");

            sb.Append("<Row>");
            Cell("Nombre");
            Cell("Usuario");
            Cell("Equipo");
            Cell("Rol");
            Cell("Email");
            Cell("Departamento");
            sb.Append("</Row>");

            foreach (var r in rows)
            {
                sb.Append("<Row>");
                Cell($"{r.Name} {r.LastName}".Trim());
                Cell(r.DomainUser ?? "");
                Cell(r.TeamName ?? "");
                Cell(r.RolPrincipal ?? "Sin Rol");
                Cell(r.CompanyEmail ?? "");
                Cell(r.Department ?? "");
                sb.Append("</Row>");
            }

            sb.Append(@"</Table>
  </Worksheet>
</Workbook>");

            return sb.ToString();
        }
    }
}