using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Packaging.Licenses;

namespace ERP_BIEN.Pages.Licenses
{
    public class DeleteModel : PageModel
    {
        public IndexModel.LicenseVM License { get; set; }

        public IActionResult OnGet(int id)
        {
            var all = LicenseData.GetAll();
            License = all.FirstOrDefault(x => x.Id == id);

            if (License == null)
                return RedirectToPage("/Licenses/Index");

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            LicenseData.Delete(id);
            return RedirectToPage("/Licenses/Index");
        }
    }
}
