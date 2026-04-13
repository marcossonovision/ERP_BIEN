using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_BIEN.Models
{
    public class License
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Price { get; set; }
        public string Producto { get; set; }
        public string Proveedor { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? Caducidad { get; set; }
        public bool Asignada { get; set; }
        public bool Disponible { get; set; }

        public License()
        {
            Code = Price = Producto = Proveedor = string.Empty;
        }

        public int? UserId { get; set; }     // FK
        public virtual User User { get; set; }



    }
}
