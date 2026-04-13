using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ERP_BIEN.Pages.Licenses
{
    public class CreateModel : PageModel
    {
        public IActionResult OnPost(string Product, string Key, string AssignedTo, LicenseStatus Status)
        {
            LicenseData.Add(new IndexModel.LicenseVM
            {
                Id = LicenseData.NextId(),
                Product = Product,
                Key = Key,
                AssignedTo = AssignedTo,
                Status = Status
            });

            return RedirectToPage("/Licenses/Index");
        }
    }
}
