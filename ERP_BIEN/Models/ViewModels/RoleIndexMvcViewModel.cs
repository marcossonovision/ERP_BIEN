using ERP_BIEN.Models;

namespace ERP_BIEN.Models.ViewModels
{
    internal class RoleIndexMvcViewModel
    {
        public List<Role> Roles { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public string Search { get; set; }
    }
}