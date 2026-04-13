using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace ERP_BIEN.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public string UserName { get; set; }

        public void OnGet()
        {
            // Obtiene DOMINIO\usuario
            var raw = User.Identity?.Name ?? "Usuario";

            // Quitar dominio
            var clean = raw.Contains("\\") ? raw.Split('\\')[1] : raw;

            // Convertir "marcos.gutierrez" → "Marcos Gutierrez"
            clean = clean.Replace('.', ' ');
            clean = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clean);

            UserName = clean;
        }
    }
}
