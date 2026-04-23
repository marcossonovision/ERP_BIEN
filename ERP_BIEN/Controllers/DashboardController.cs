using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_BIEN.Models;
using System.Globalization;

namespace ERP_BIEN.Controllers
{
    [Authorize(Policy = "DASHBOARD")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var raw = User.Identity?.Name ?? "Usuario";

            var clean = raw.Contains("\\")
                ? raw.Split('\\')[1]
                : raw;

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
