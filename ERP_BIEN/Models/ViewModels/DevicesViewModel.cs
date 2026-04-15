using ERP_BIEN.Common.Enums;
using ERP_BIEN.Models;

namespace ERP_BIEN.Models.ViewModels
{
    public class DevicesViewModel
    {
        public List<Device> Devices { get; set; }
        public List<User> Users { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public string DeviceTypeFilter { get; set; }
        public StatusDevice? StatusFilter { get; set; }
        public int? UserIdFilter { get; set; }
        public string HostnameFilter { get; set; }
        public string ModelFilter { get; set; }
        public string SNFilter { get; set; }
        public DateTime? ManufacturingFrom { get; set; }
        public DateTime? ManufacturingTo { get; set; }
        public DateTime? UseFrom { get; set; }
        public DateTime? UseTo { get; set; }
    }
}
