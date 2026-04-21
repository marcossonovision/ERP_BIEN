using System.ComponentModel.DataAnnotations;

namespace ERP_BIEN.ViewModels
{
    public class RoleViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
