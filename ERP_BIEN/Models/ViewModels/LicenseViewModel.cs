using System;
using System.ComponentModel.DataAnnotations;

namespace ERP_BIEN.ViewModels
{
    public class LicenseViewModel
    {
        public int Id { get; set; }

        [Required] public string Code { get; set; }
        [Required] public string Producto { get; set; }
        [Required] public string Proveedor { get; set; }
        public string Price { get; set; }
        [DataType(DataType.Date)] public DateTime? Caducidad { get; set; }
        public bool Asignada { get; set; }
        public bool Disponible { get; set; }
        public int? UserId { get; set; }
    }
}
