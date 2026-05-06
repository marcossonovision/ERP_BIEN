using System;

namespace ERP_BIEN.Models
{
    public class LicenseHistory
    {
        public int Id { get; set; }

        public int LicenseId { get; set; }
        public virtual License License { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
