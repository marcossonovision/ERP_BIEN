namespace WebCoreMVC.Services
{
    using ERP_BIEN.Data;
    using ERP_BIEN.Services;          // RoleAccessMatrix si lo tienes aquí, si no, usa el de abajo (Paso 3)
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

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

            // ✅ Normalizar: dominio\user  |  user@dominio  |  user
            static string Normalize(string input)
            {
                var s = input.Trim();

                if (s.Contains("\\"))
                    s = s.Split('\\')[1].Trim();

                if (s.Contains("@"))
                    s = s.Split('@')[0].Trim();

                return s;
            }

            var normalized = Normalize(rawUser);

            // Probamos varias claves por si en BD guardaste distinto formato
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
                .FirstOrDefaultAsync(u => u.DomainUser != null && keys.Contains(u.DomainUser));

            if (user == null)
            {
                _logger.LogWarning("RBAC: No se encontró usuario en BD. rawUser={RawUser} normalized={Normalized}", rawUser, normalized);
                identity.AddClaim(new Claim(TransformedMarker, "1"));
                return principal;
            }

            // ===== ROLES =====
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

            // ===== PERMISOS =====
            const string PermissionClaimType = "permission";

            var permissions = user.UserRoles?
                .Where(ur => ur.Role != null)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission.Code)
                .Where(pc => !string.IsNullOrWhiteSpace(pc))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();

            foreach (var permissionCode in permissions)
            {
                if (!identity.HasClaim(PermissionClaimType, permissionCode))
                    identity.AddClaim(new Claim(PermissionClaimType, permissionCode));
            }

            // ===== MÓDULOS (module) =====
            const string ModuleClaimType = "module";

            var modules = RoleAccessMatrix.GetModulesForRoles(roles);
            foreach (var module in modules)
            {
                if (!identity.HasClaim(ModuleClaimType, module))
                    identity.AddClaim(new Claim(ModuleClaimType, module));
            }

            identity.AddClaim(new Claim(TransformedMarker, "1"));
            return principal;
        }
    }
}