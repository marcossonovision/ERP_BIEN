using System;

namespace ERP_BIEN.Models
{
    public class DeviceHistory
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
