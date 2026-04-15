namespace ERP_BIEN.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string LastName { get; set; }

        public string DomainUser { get; set; }

        public int? TeamId { get; set; }
        public string TeamName { get; set; }
    }
}
