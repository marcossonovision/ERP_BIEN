namespace ERP_BIEN.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? DomainUser { get; set; }

        public int? TeamId { get; set; }
        public string? TeamName { get; set; }

        public string RolPrincipal { get; set; } = "Sin Rol";

        public string? CompanyEmail { get; set; }
        public string? Department { get; set; }
    }
}
