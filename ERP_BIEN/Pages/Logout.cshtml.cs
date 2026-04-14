using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // No se puede cerrar sesión de Windows Authentication.
        // Solo redirigimos fuera del ERP.
        return Redirect("/Dashboard");
    }
}