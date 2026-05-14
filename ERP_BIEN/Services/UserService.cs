using System;
using System.Collections.Generic;
using System.Linq;
using ERP_BIEN.Data;
using ERP_BIEN.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================
        // PAGINACIÓN + FILTROS (firma antigua)
        // ============================================
        public (List<User> Users, int TotalPages) GetPagedUsers(
            string searchName,
            string searchDomain,
            int? searchTeamId,
            int pageNumber,
            int pageSize = 10)
        {
            return GetPagedUsers(
                searchName,
                searchDomain,
                searchTeamId,
                pageNumber,
                searchRoleId: null,
                searchIsActive: null,
                pageSize: pageSize
            );
        }

        // ============================================
        // PAGINACIÓN + FILTROS (firma nueva)
        // ✅ Añade Rol + Estado
        // ============================================
        public (List<User> Users, int TotalPages) GetPagedUsers(
            string searchName,
            string searchDomain,
            int? searchTeamId,
            int pageNumber,
            int? searchRoleId,
            bool? searchIsActive,
            int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;

            var query = _context.Users
                .Include(u => u.Team)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var nameFilter = searchName.Trim().ToLower();
                query = query.Where(u =>
                    (u.Name != null && u.Name.ToLower().StartsWith(nameFilter)) ||
                    (u.LastName != null && u.LastName.ToLower().StartsWith(nameFilter))
                );
            }

            if (!string.IsNullOrWhiteSpace(searchDomain))
            {
                var domainFilter = searchDomain.Trim().ToLower();
                query = query.Where(u =>
                    u.DomainUser != null &&
                    u.DomainUser.ToLower().StartsWith(domainFilter)
                );
            }

            if (searchTeamId.HasValue)
            {
                query = query.Where(u => u.TeamId == searchTeamId.Value);
            }

            if (searchRoleId.HasValue)
            {
                query = query.Where(u =>
                    u.UserRoles != null &&
                    u.UserRoles.Any(ur => ur.RoleId == searchRoleId.Value)
                );
            }

            if (searchIsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == searchIsActive.Value);
            }

            int totalUsers = query.Count();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
                pageNumber = totalPages;

            var users = query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (users, totalPages);
        }

        // ============================================
        // OBTENER UN USUARIO (con Team + Roles)
        // ============================================
        public User GetUser(int id)
        {
            return _context.Users
                .Include(u => u.Team)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == id);
        }

        // ============================================
        // CREAR USUARIO ✅ (devuelve User)
        // ============================================
        public User CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        // ============================================
        // EDITAR USUARIO
        // ============================================
        public void UpdateUser(User updated)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == updated.Id);
            if (user == null)
                throw new Exception("Usuario no encontrado");

            user.Name = updated.Name;
            user.LastName = updated.LastName;
            user.DomainUser = updated.DomainUser;
            user.TeamId = updated.TeamId;
            user.IsActive = updated.IsActive;

            _context.SaveChanges();
        }

        // ============================================
        // ACTUALIZAR ROL DEL USUARIO (1 rol)
        // ============================================
        public void UpdateUserRole(int userId, int? roleId)
        {
            var user = _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            if (user.UserRoles != null && user.UserRoles.Any())
                _context.UserRoles.RemoveRange(user.UserRoles);

            if (roleId.HasValue)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId.Value
                });
            }

            _context.SaveChanges();
        }

        // ============================================
        // ✅ ELIMINAR USUARIO (CORREGIDO: evita DbUpdateConcurrencyException)
        // ============================================
        public void DeleteUser(int id)
        {
            // Cargamos el usuario con todo lo que tú estabas borrando manualmente
            var user = _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Devices)
                .Include(u => u.Licenses)
                .Include(u => u.PersonalInfo)
                .Include(u => u.CompanyInfo)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return;

            // Guardamos IDs para limpiar después (si quieres evitar huérfanos)
            var personalInfoId = user.PersonalInfoId;
            var companyInfoId = user.CompanyInfoId;

            // 1) Romper relaciones que puedan dar FK
            if (user.UserRoles != null && user.UserRoles.Any())
                _context.UserRoles.RemoveRange(user.UserRoles);

            if (user.Devices != null && user.Devices.Any())
                _context.Devices.RemoveRange(user.Devices);

            if (user.Licenses != null && user.Licenses.Any())
                _context.Licenses.RemoveRange(user.Licenses);

            // 2) CLAVE:
            // NO borres PersonalInfo / CompanyInfo ANTES del User
            // porque en tu modelo hay cascada desde esas tablas hacia User.
            // (Borrar el principal puede borrar el User y luego EF intenta borrarlo otra vez => Concurrency) 

            _context.Users.Remove(user);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si por cascada u otro proceso ya no existe, el resultado final (usuario borrado) ya se cumple
                return;
            }

            // 3) Limpieza opcional de tablas 1:1 (después de borrar el User ya no hay cascada peligrosa)
            // Solo si existen y quieres que no queden registros huérfanos
            if (personalInfoId.HasValue)
            {
                var pi = _context.PersonalInformation.FirstOrDefault(x => x.Id == personalInfoId.Value);
                if (pi != null) _context.PersonalInformation.Remove(pi);
            }

            if (companyInfoId.HasValue)
            {
                var ci = _context.CompanyInformation.FirstOrDefault(x => x.Id == companyInfoId.Value);
                if (ci != null) _context.CompanyInformation.Remove(ci);
            }

            _context.SaveChanges();
        }

        // ============================================
        // TOGGLE ACTIVO/INACTIVO
        // ============================================
        public bool ToggleUserActive(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            _context.SaveChanges();
            return user.IsActive;
        }

        // ============================================
        // LISTA DE EQUIPOS
        // ============================================
        public List<SelectListItem> GetTeams()
        {
            return _context.Teams
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToList();
        }

        // ============================================
        // LISTA DE ROLES
        // ============================================
        public List<SelectListItem> GetRoles()
        {
            return _context.Roles
                .OrderBy(r => r.Code)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Code
                })
                .ToList();
        }

        // ============================================
        // DUPLICADOS DomainUser
        // ============================================
        public bool DomainUserExists(string domainUser, int? excludeUserId = null)
        {
            domainUser = (domainUser ?? "").Trim();
            if (string.IsNullOrEmpty(domainUser))
                return false;

            return _context.Users.Any(u =>
                u.DomainUser == domainUser &&
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value)
            );
        }
    }
}