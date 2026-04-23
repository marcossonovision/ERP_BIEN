namespace WebCoreMVC.Services
{
    using ERP_BIEN.Data;
    using ERP_BIEN.Models;
    using ERP_BIEN.Services;
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
            // ROLES
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
            // PERMISSIONS → permission
            // =========================
            foreach (var ur in user.UserRoles ?? Enumerable.Empty<UserRole>())
            {
                var role = ur.Role;
                if (role == null) continue;

                foreach (var rp in role.RolePermissions ?? Enumerable.Empty<RolePermission>())
                {
                    if (rp.Permission != null &&
                        !string.IsNullOrWhiteSpace(rp.Permission.Code))
                    {
                        if (!identity.HasClaim("permission", rp.Permission.Code))
                        {
                            identity.AddClaim(new Claim("permission", rp.Permission.Code));
                        }
                    }
                }
            }

            // =========================
            // MODULES → module
            // =========================
            var modules = RoleAccessMatrix.GetModulesForRoles(roles);
            foreach (var module in modules)
            {
                if (!identity.HasClaim("module", module))
                    identity.AddClaim(new Claim("module", module));
            }

            identity.AddClaim(new Claim(TransformedMarker, "1"));
            return principal;
        }
    }
}