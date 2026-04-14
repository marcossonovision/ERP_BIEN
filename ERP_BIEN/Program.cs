using ERP_BIEN.Data;
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

// 3. Razor Pages
builder.Services.AddRazorPages();

#region RECOGER LOS PERMISOS DEL USUARIO
// Registrar el transformador
builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
#endregion

//builder.Services.AddAuthentication("Windows").AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

//#region CREACION DE DATOS
//DataSeeder.RellenarDatos(app.Services.CreateScope());
//#endregion

// 4. Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 5. Archivos estáticos
app.UseStaticFiles();

// 6. Routing
app.UseRouting();

// 7. Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 8. Redirección automática al Dashboard
app.MapGet("/", context =>
{
    context.Response.Redirect("/Dashboard");
    return Task.CompletedTask;
});

// 9. Razor Pages
app.MapRazorPages();

app.Run();
