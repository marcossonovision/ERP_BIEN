using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ERP_BIEN.Pages.Licenses
{
    public class EditModel : PageModel
    {
        public IndexModel.LicenseVM License { get; set; }

        public IActionResult OnGet(int id)
        {
            License = LicenseData.GetAll().FirstOrDefault(x => x.Id == id);

            if (License == null)
                return RedirectToPage("/Licenses/Index");

            return Page();
        }

        public IActionResult OnPost(int Id, string Product, string Key, string AssignedTo, LicenseStatus Status)
        {
            var list = LicenseData.GetAll();
            var item = list.FirstOrDefault(x => x.Id == Id);

            if (item != null)
            {
                item.Product = Product;
                item.Key = Key;
                item.AssignedTo = AssignedTo;
                item.Status = Status;
            }

            return RedirectToPage("/Licenses/Index");
        }
    }
}
