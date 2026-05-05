using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class License
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Price { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Producto { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Proveedor { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime? Caducidad { get; set; }

        public bool Asignada { get; set; }
        public bool Disponible { get; set; } = true;

        public int? UserId { get; set; }
        public virtual User? User { get; set; }
    }
}