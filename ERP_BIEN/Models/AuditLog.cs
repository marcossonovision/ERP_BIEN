using System;

namespace ERP_BIEN.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; } // Create, Edit, Delete, Assign, Remove...
        public string Entity { get; set; } // License, Device, Employee...
        public int? EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } // JSON opcional
    }
}
