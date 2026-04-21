using ERP_BIEN.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public abstract class Device
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Hostname { get; set; }

        [MaxLength(100)]
        public string SN { get; set; }

        [MaxLength(100)]
        public string Model { get; set; }

        public int NumberOfDevice { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ManufacturingDate { get; set; }
        public StatusDevice Status { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? UseDate { get; set; }


        public int? UserId { get; set; }     // FK
        public virtual User User { get; set; }


        [NotMapped]
        public string DeviceTypeKey =>
            this switch
            {
                Computer => "computer",
                Screen => "screen",
                Phone => "phone",
                Ubikey => "ubikey",
                DockStation => "dock",
                _ => "default"
            };


    }
}