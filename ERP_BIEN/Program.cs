using ERP_BIEN.Data;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using WebCoreMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar el DbContext con la cadena de conexión
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Activar SSO (Windows Authentication)
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// 3. ACTIVAR MVC
builder.Services.AddControllersWithViews();

// 4. Razor Pages (las mantenemos mientras migramos)
builder.Services.AddRazorPages();

// 👉 AÑADIDO: UserService para MVC
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DeviceService>();


//#region RECOGER LOS PERMISOS DEL USUARIO
//builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
//#endregion

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error"); // (lo dejo como lo tienes)
    app.UseHsts();
}

app.UseHttpsRedirection();

// Archivos estáticos
app.UseStaticFiles();

// Routing
app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();


// ✅ CAMBIO CLAVE: Dashboard como ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}"
);


app.MapGet("/", context =>
{
    context.Response.Redirect("/Dashboard");
    return Task.CompletedTask;
});


// Razor Pages (lo quitaremos cuando acabemos la migración)
app.MapRazorPages();

app.Run();