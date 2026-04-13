using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class Child
    {
        public int Id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateOfBirth { get; set; }
        public bool Disability { get; set; }
    }
}
