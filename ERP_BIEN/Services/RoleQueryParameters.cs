namespace ERP_BIEN.ViewModels
{
    public class RoleQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Search { get; set; }
    }
}
