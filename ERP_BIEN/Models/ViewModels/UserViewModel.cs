using System.ComponentModel.DataAnnotations;

namespace ERP_BIEN.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        // ============================
        // NOMBRE
        // ============================
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
        public string Name { get; set; }


        // ============================
        // APELLIDOS
        // ============================
        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 80 caracteres.")]
        public string LastName { get; set; }


        // ============================
        // DOMAIN USER (CRÍTICO)
        // ============================
        [Required(ErrorMessage = "El usuario de dominio es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Debe tener entre 3 y 100 caracteres.")]
        [RegularExpression(@"^[A-Za-z0-9\.\-_\\@]+$", ErrorMessage = "Solo letras, números y . _ - \\ @")]
        public string DomainUser { get; set; }


        // ============================
        // TEAM
        // ============================
        public int? TeamId { get; set; }

        public string TeamName { get; set; }


        // ============================
        // ROLE (IMPORTANTE)
        // ============================
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public int? RoleId { get; set; }

        public string RoleName { get; set; } = "Sin Rol";


        // ============================
        // ESTADO
        // ============================
        public bool IsActive { get; set; }
    }
}
