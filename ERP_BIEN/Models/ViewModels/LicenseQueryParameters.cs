namespace ERP_BIEN.ViewModels
{
    public class LicenseQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Search { get; set; }
        public string SearchProveedor { get; set; }
        public string SearchProducto { get; set; }
        public string SearchAsignada { get; set; }
    }
}
