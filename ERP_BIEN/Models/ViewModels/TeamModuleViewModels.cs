namespace ERP_BIEN.Models.ViewModels
{
    public class TeamModuleIndexViewModel
    {
        public int SelectedTeamId { get; set; }
        public List<TeamItemVm> ManagedTeams { get; set; } = new();
        public List<UserItemVm> Members { get; set; } = new();
        public List<UserItemVm> Candidates { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? InfoMessage { get; set; }
    }

    public class TeamItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class UserItemVm
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string DomainUser { get; set; } = "";
    }
}