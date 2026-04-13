using ERP_BIEN.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class Computer : Device
    {
        public bool isClient { get; set; }

        public int? HardwareId { get; set; }
        [ForeignKey(nameof(HardwareId))]
        public virtual Hardware Hardware { get; set; }

        public int? SoftwareId { get; set; }
        [ForeignKey(nameof(SoftwareId))]
        public virtual Software Software { get; set; }

        public ComputerType ComputerType { get; set; }
    }
}
