namespace WebCoreMVC.Services
{
    using ERP_BIEN.Data;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;


    public class CustomClaimsTransformation : IClaimsTransformation
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CustomClaimsTransformation> _logger;

        // Para evitar consultas repetidas en la misma request, marcamos cuando ya hemos transformado.
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

            var domainUser = rawUser.Trim();

            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.DomainUser == domainUser);

            if (user == null)
            {
                _logger.LogWarning("RBAC: No se encontró usuario en BD para DomainUser={DomainUser}", domainUser);
                identity.AddClaim(new Claim(TransformedMarker, "1"));
                return principal;
            }

            // ROLES
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
                {
                    identity.AddClaim(new Claim(identity.RoleClaimType, roleCode));
                }
            }

            // PERMISOS
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
                {
                    identity.AddClaim(new Claim(PermissionClaimType, permissionCode));
                }
            }

            identity.AddClaim(new Claim(TransformedMarker, "1"));
            return principal;
        }


    }
}
