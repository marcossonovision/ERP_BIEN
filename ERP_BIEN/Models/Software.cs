using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class Software
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string SO { get; set; }

        public int LicenseId { get; set; }

        [ForeignKey(nameof(LicenseId))]
        public virtual License Licenses { get; set; }
    }
}