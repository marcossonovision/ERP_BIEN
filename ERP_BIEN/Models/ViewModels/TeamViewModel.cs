namespace ERP_BIEN.Models
{
    public class TeamViewModel
    {
        public string? Manager { get; set; }
        public List<TeamUserViewModel> Users { get; set; } = new();
    }

    public class TeamUserViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? TeamName { get; set; }
    }
}