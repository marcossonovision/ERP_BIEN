using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ERP_BIEN.Pages.Licenses
{
    public class DetailsModel : PageModel
    {
        public IndexModel.LicenseVM License { get; set; }

        public IActionResult OnGet(int id)
        {
            License = LicenseData.GetAll().FirstOrDefault(x => x.Id == id);

            if (License == null)
                return RedirectToPage("/Licenses/Index");

            return Page();
        }
    }
}
