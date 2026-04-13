using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Device> Devices { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public StatusDevice? StatusFilter { get; set; }

        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = 1;
            TotalPages = 1;

            Devices = new List<Device>
    {
        new Computer {
            Id = 1,
            Hostname = "PC-001",
            Model = "HP ZBook 15 G6",
            SN = "11DSA551",
            Status = StatusDevice.BuenEstado,
            User = new User { Name = "Carlos", LastName = "Martín" }
        },
        new Screen {
            Id = 2,
            Hostname = "MN-003",
            Model = "Dell UltraSharp 27\"",
            SN = "23HGF892",
            Status = StatusDevice.BuenEstado,
            User = new User { Name = "Ana", LastName = "López" }
        },
        new Ubikey {
            Id = 3,
            Hostname = "UBK-001",
            Model = "YubiKey 5 NFC",
            SN = "LNV-00239",
            Status = StatusDevice.BuenEstado,
            User = null
        },
        new Computer {
            Id = 4,
            Hostname = "PC-002",
            Model = "Lenovo T14",
            SN = "AA5521X",
            Status = StatusDevice.BuenEstado,
            User = new User { Name = "Miguel", LastName = "Angel" }
        },
        new Computer {
            Id = 5,
            Hostname = "PC-003",
            Model = "—",
            SN = "—",
            Status = StatusDevice.Defectuoso,
            User = null
        },
        new Screen {
            Id = 6,
            Hostname = "MN-004",
            Model = "Dell UltraSharp 27\"",
            SN = "—",
            Status = StatusDevice.Defectuoso,
            User = null
        }
    };
        }


        public string GetDeviceType(Device d)
        {
            return d.GetType().Name;
        }
    }
}
