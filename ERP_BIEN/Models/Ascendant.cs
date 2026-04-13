using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class Ascendant
    {
        public int Id { get; set; }
        public bool Disability { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateOfBirth { get; set; }
    }
}
