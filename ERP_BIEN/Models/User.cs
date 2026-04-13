using ERP_BIEN.Common.Enums;

namespace ERP_BIEN.Models
{
    public class User
    {
        public User()
        {
            Devices = new HashSet<Device>();
            Licenses = new HashSet<License>();

            // 🔐 NUEVO (RBAC)
            UserRoles = new HashSet<UserRole>();
            ManagedTeams = new HashSet<Team>();
        }

        // =========================
        // CAMPOS EXISTENTES
        // =========================
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }

        public string DomainUser { get; set; }



        // =========================
        // RELACIONES EXISTENTES
        // =========================
        public virtual ICollection<Device> Devices { get; set; }
        public virtual ICollection<License> Licenses { get; set; }

        // 1:1 (como ya lo tienes)
        public PersonalInformation PersonalInfo { get; set; }
        public int? PersonalInfoId { get; set; }
        public CompanyInformation CompanyInfo { get; set; }

        public int? CompanyInfoId { get; set; }

        // =========================
        // 🔐 NUEVO MODELO RBAC
        // =========================
        public virtual ICollection<UserRole> UserRoles { get; set; }


        // Team membership
        public int? TeamId { get; set; }
        public virtual Team Team { get; set; }

        // Teams where this user is Project Manager
        public virtual ICollection<Team> ManagedTeams { get; set; }


    }

}