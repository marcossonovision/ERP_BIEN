using System.ComponentModel.DataAnnotations;

namespace ERP_BIEN.Models
{
    public class Phone : Device
    {
        public int phonenumber { get; set; }
        public int Extension { get; set; }

        [MaxLength(50)]
        public string SIM { get; set; }
        public int IMEI { get; set; }
        public int PIN { get; set; }
        public int PUK { get; set; }


    }
}
