using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Mvc;

namespace ERP_BIEN.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Logout()
        {
            // Limpia cualquier dato de sesión
            HttpContext.SignOutAsync(NegotiateDefaults.AuthenticationScheme);

            // Forzar re-autenticación de Windows
            return Challenge(
                new AuthenticationProperties { RedirectUri = "/" },
                NegotiateDefaults.AuthenticationScheme
            );
        }
    }
}
