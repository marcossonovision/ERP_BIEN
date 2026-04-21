using System.Collections.Generic;
using ERP_BIEN.Models;

namespace ERP_BIEN.ViewModels
{
    public class LicenseIndexMvcViewModel
    {
        public List<License> Licenses { get; set; } = new List<License>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public string Search { get; set; }
        public string SearchProveedor { get; set; }
        public string SearchProducto { get; set; }
        public string SearchAsignada { get; set; }
    }
}
