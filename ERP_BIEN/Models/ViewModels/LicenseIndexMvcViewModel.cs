using System.Collections.Generic;
using ERP_BIEN.Models;

namespace ERP_BIEN.ViewModels
{
    public class LicenseIndexMvcViewModel
    {
        // =========================
        // DATOS PRINCIPALES
        // =========================
        public List<License> Licenses { get; set; } = new();

        public List<User> Users { get; set; } = new();

        // ✅ DASHBOARD
        public int TotalLicenses { get; set; }
        public int AssignedLicenses { get; set; }
        public int FreeLicenses { get; set; }
        public int UsagePercentage { get; set; }

        // ✅ TOP USUARIOS
        public List<string> TopUsers { get; set; } = new();

        // ✅ ALERTAS (caducidad)
        public List<string> ExpiringLicenses { get; set; } = new();

        // =========================
        // PAGINACIÓN
        // =========================
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        // =========================
        // FILTROS
        // =========================
        public string Search { get; set; }
        public string SearchProveedor { get; set; }
        public string SearchProducto { get; set; }
        public string SearchAsignada { get; set; }
    }
}