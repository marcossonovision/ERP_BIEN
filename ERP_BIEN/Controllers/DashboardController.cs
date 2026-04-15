using Microsoft.AspNetCore.Mvc;
using ERP_BIEN.Models;
using System.Globalization;

namespace ERP_BIEN.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var raw = User.Identity?.Name ?? "Usuario";

            // Quitar dominio
            var clean = raw.Contains("\\") ? raw.Split('\\')[1] : raw;

            // Convertir "marcos.gutierrez" → "Marcos Gutierrez"
            clean = clean.Replace('.', ' ');
            clean = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clean);

            var model = new DashboardViewModel
            {
                UserName = clean
            };

            return View(model);
        }
    }
}