using System.ComponentModel.DataAnnotations;

namespace ERP_BIEN.Models
{
    public class Hardware
    {
        public int Id { get; set; }

        [MaxLength(100)] public string RAM { get; set; }
        [MaxLength(100)] public string HardDisk { get; set; }
        [MaxLength(100)] public string Processor { get; set; }
    }

}