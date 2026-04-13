using System.Collections.Generic;
using System.Linq;

namespace ERP_BIEN.Pages.Licenses
{
    public static class LicenseData
    {
        private static List<IndexModel.LicenseVM> _licenses = new()
        {
            new() { Id = 1, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Carlos Martín", Status = LicenseStatus.Disponible },
            new() { Id = 2, Product = "Adobe Photoshop", Key = "****-****-**PS", AssignedTo = "Angel Díaz", Status = LicenseStatus.Activo },
            new() { Id = 3, Product = "AutoCAD", Key = "****-****-**AC", AssignedTo = "Maria Belén", Status = LicenseStatus.Expirada },
            new() { Id = 4, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Marcos Gutierrez", Status = LicenseStatus.Activo },
            new() { Id = 5, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Carlos Martín", Status = LicenseStatus.Expirada },
            new() { Id = 6, Product = "Microsoft 365", Key = "****-****-**H2", AssignedTo = "Carlos Martín", Status = LicenseStatus.Activo }
        };

        public static List<IndexModel.LicenseVM> GetAll() => _licenses;

        public static void Delete(int id)
        {
            var item = _licenses.FirstOrDefault(x => x.Id == id);
            if (item != null)
                _licenses.Remove(item);
        }

        public static void Add(IndexModel.LicenseVM license)
        {
            _licenses.Add(license);
        }

        public static int NextId() => _licenses.Max(x => x.Id) + 1;
    }
}
