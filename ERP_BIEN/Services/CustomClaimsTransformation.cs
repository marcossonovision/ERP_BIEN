using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebCoreMVC.Services
{
    public class CustomClaimsTransformation : IClaimsTransformation
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CustomClaimsTransformation> _logger;

        private const string TransformedMarker = "rbac_transformed";

        public CustomClaimsTransformation(AppDbContext db, ILogger<CustomClaimsTransformation> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
                return principal;

            // Evita duplicar claims en cada request
            if (identity.HasClaim(c => c.Type == TransformedMarker))
                return principal;

            var rawUser =
                principal.FindFirstValue(ClaimTypes.Name) ??
                principal.FindFirstValue(ClaimTypes.Upn) ??
                principal.FindFirstValue("preferred_username") ??
                principal.Identity?.Name;

            if (string.IsNullOrWhiteSpace(rawUser))
            {
                identity.AddClaim(new Claim(TransformedMarker, "1"));
                return principal;
            }

            static string Normalize(string input)
            {
                var s = input.Trim();

                // DOMINIO\usuario -> usuario
                if (s.Contains("\\"))
                    s = s.Split('\\')[1].Trim();

                // usuario@dominio -> usuario
                if (s.Contains("@"))
                    s = s.Split('@')[0].Trim();

                return s;
            }

            var normalized = Normalize(rawUser);

            // Probamos contra ambas variantes (con dominio y sin dominio)
            var keys = new[] { rawUser.Trim(), normalized }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u =>
                    u.DomainUser != null &&
                    keys.Contains(u.DomainUser));

            if (user == null)
            {
                _logger.LogWarning(
                    "RBAC: Usuario no encontrado. rawUser={RawUser} normalized={Normalized}",
                    rawUser, normalized);

                identity.AddClaim(new Claim(TransformedMarker, "1"));
                return principal;
            }

            // =========================
            // ROLES -> ClaimTypes.Role
            // =========================
            var roles = user.UserRoles?
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role.Code)
                .Where(rc => !string.IsNullOrWhiteSpace(rc))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();

            foreach (var roleCode in roles)
            {
                if (!identity.HasClaim(identity.RoleClaimType, roleCode))
                    identity.AddClaim(new Claim(identity.RoleClaimType, roleCode));
            }

            // =========================
            // PERMISSIONS -> "permission"
            // =========================
            var permissions = user.UserRoles?
                .Where(ur => ur.Role != null)
                .SelectMany(ur => ur.Role.RolePermissions ?? Enumerable.Empty<RolePermission>())
                .Where(rp => rp.Permission != null && !string.IsNullOrWhiteSpace(rp.Permission.Code))
                .Select(rp => rp.Permission.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();

            foreach (var p in permissions)
            {
                if (!identity.HasClaim("permission", p))
                    identity.AddClaim(new Claim("permission", p));
            }

            // =========================
            // MODULES -> "module"
            // (según matriz RoleAccessMatrix)
            // =========================
            var modules = RoleAccessMatrix.GetModulesForRoles(roles);
            foreach (var module in modules)
            {
                if (!identity.HasClaim("module", module))
                    identity.AddClaim(new Claim("module", module));
            }

            // Marca transformado
            identity.AddClaim(new Claim(TransformedMarker, "1"));
            return principal;
        }
    }
}