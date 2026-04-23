using ERP_BIEN.Data;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
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
builder.Services.AddAuthentication(options =>
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
// 4. APLICACIÓN / SERVICES
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
    // Dashboard: cualquier usuario autenticado
    options.AddPolicy("DASHBOARD", p =>
        p.RequireAuthenticatedUser());

    options.AddPolicy("USERS", p =>
        p.RequireClaim("module", "USERS"));

    options.AddPolicy("ROLES", p =>
        p.RequireClaim("module", "ROLES"));

    options.AddPolicy("EMPLOYEES", p =>
        p.RequireClaim("module", "EMPLOYEES"));

    options.AddPolicy("DEVICES", p =>
        p.RequireClaim("module", "DEVICES"));

    options.AddPolicy("LICENSES", p =>
        p.RequireClaim("module", "LICENSES"));

    options.AddPolicy("TEAM", p =>
        p.RequireClaim("module", "TEAM"));
});


var app = builder.Build();

// =====================================================
// 7. PIPELINE HTTP
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
// 8. RUTAS
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
// 🔥 9. DATA SEEDER (CLAVE)
// =====================================================
using (var scope = app.Services.CreateScope())
{
    DataSeeder.RellenarDatos(scope);
}

app.Run();