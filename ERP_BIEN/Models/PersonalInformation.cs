using ERP_BIEN.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class PersonalInformation
    {
        public int Id { get; set; }   // NO debe ser nullable

        public int Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string DNI { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateOfBirth { get; set; }

        public string Studies { get; set; }
        public TypeOfStudies? TypeOfStudies { get; set; }
        public string Academy { get; set; }
        public FamilySituation? FamilySituation { get; set; }

        // ===== PRECEPTOR DETAILS (OPCIONAL) =====
        public int? PreceptorDetailsId { get; set; }
        public virtual PreceptorDetails PreceptorDetails { get; set; }

        // ===== RELACIONES =====
        public virtual ICollection<Child> Childs { get; set; }
        public virtual ICollection<Ascendant> Ascendants { get; set; }

        public PersonalInformation()
        {
            Childs = new List<Child>();
            Ascendants = new List<Ascendant>();
        }
    }
}
