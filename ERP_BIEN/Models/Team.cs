namespace ERP_BIEN.Models
{
    public class Team
    {
        public Team()
        {
            Members = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        // Project Manager (User)
        public int? ProjectManagerId { get; set; }
        public virtual User ProjectManager { get; set; }

        // Members (Users)
        public virtual ICollection<User> Members { get; set; }
    }
}
