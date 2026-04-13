using System.ComponentModel.DataAnnotations;

namespace ERP_BIEN.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }


        public ICollection<RolePermission> RolePermissions { get; set; }
                = new HashSet<RolePermission>();

    }
}
