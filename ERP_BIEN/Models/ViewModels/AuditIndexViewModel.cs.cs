using System.Collections.Generic;
using ERP_BIEN.Models;

namespace ERP_BIEN.ViewModels
{
    public class AuditIndexViewModel
    {
        public IEnumerable<AuditLog> Logs { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }

        public int TotalPages => (int)System.Math.Ceiling((double)Total / PageSize);
    }
}
