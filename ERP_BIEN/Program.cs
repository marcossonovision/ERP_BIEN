using ERP_BIEN.Data;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebCoreMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. DB CONTEXT
// =====================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================================================
// 2. WINDOWS AUTHENTICATION (SSO)
// =====================================================
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = NegotiateDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = NegotiateDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
    })
    .AddNegotiate();

// =====================================================
// 3. MVC + RAZOR
// =====================================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// =====================================================
// 4. APPLICATION SERVICES
// =====================================================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ERP_BIEN.Services.TeamService>();

// =====================================================
// 5. CLAIMS TRANSFORMATION (RBAC)
// =====================================================
builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();

// =====================================================
// 6. AUTHORIZATION POLICIES
// =====================================================
builder.Services.AddAuthorization(options =>
{
    // -------------------------------------------------
    // ✅ DASHBOARD (solo autenticación)
    // -------------------------------------------------
    options.AddPolicy("DASHBOARD", p =>
        p.RequireAuthenticatedUser());

    // -------------------------------------------------
    // ✅ MODULES (UI / MENÚ / NAVEGACIÓN)
    // -------------------------------------------------
    options.AddPolicy("USERS", p => p.RequireClaim("module", "USERS"));
    options.AddPolicy("ROLES", p => p.RequireClaim("module", "ROLES"));
    options.AddPolicy("EMPLOYEES", p => p.RequireClaim("module", "EMPLOYEES"));
    options.AddPolicy("DEVICES", p => p.RequireClaim("module", "DEVICES"));
    options.AddPolicy("LICENSES", p => p.RequireClaim("module", "LICENSES"));
    options.AddPolicy("TEAM", p => p.RequireClaim("module", "TEAM"));

    // -------------------------------------------------
    // ✅ PERMISSIONS (SEGURIDAD REAL)
    // -------------------------------------------------
    options.AddPolicy("USR_VIEW", p => p.RequireClaim("permission", "USR_VIEW"));
    options.AddPolicy("USR_CREATE", p => p.RequireClaim("permission", "USR_CREATE"));
    options.AddPolicy("USR_EDIT", p => p.RequireClaim("permission", "USR_EDIT"));
    options.AddPolicy("USR_DELETE", p => p.RequireClaim("permission", "USR_DELETE"));

    options.AddPolicy("DEV_VIEW", p => p.RequireClaim("permission", "DEV_VIEW"));
    options.AddPolicy("DEV_ASSIGN", p => p.RequireClaim("permission", "DEV_ASSIGN"));
    options.AddPolicy("DEV_EDIT", p => p.RequireClaim("permission", "DEV_EDIT"));
    options.AddPolicy("DEV_DELETE", p => p.RequireClaim("permission", "DEV_DELETE"));

    options.AddPolicy("LIC_VIEW", p => p.RequireClaim("permission", "LIC_VIEW"));
    options.AddPolicy("LIC_ASSIGN", p => p.RequireClaim("permission", "LIC_ASSIGN"));
    options.AddPolicy("LIC_EDIT", p => p.RequireClaim("permission", "LIC_EDIT"));
    options.AddPolicy("LIC_DELETE", p => p.RequireClaim("permission", "LIC_DELETE"));

    // -------------------------------------------------
    // ✅ WRITE (PERMISO GLOBAL DE ESCRITURA)
    // -------------------------------------------------
    options.AddPolicy("WRITE", p =>
        p.RequireClaim("permission", "WRITE"));
});

var app = builder.Build();

// =====================================================
// 7. HTTP PIPELINE
// =====================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =====================================================
// 8. ROUTES
// =====================================================

// /Licenses → LicenseController
app.MapControllerRoute(
    name: "licenses_plural",
    pattern: "Licenses/{action=Index}/{id?}",
    defaults: new { controller = "License" }
);

// /Roles → RoleController
app.MapControllerRoute(
    name: "roles_plural",
    pattern: "Roles/{action=Index}/{id?}",
    defaults: new { controller = "Role" }
);

// Ruta por defecto → Dashboard
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapRazorPages();

// =====================================================
// 9. DATA SEEDER
// =====================================================
using (var scope = app.Services.CreateScope())
{
    DataSeeder.RellenarDatos(scope);
}

app.Run();