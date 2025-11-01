using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NutriAIServicio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Obtener la cadena de conexión del archivo de configuración
var connectionString = builder.Configuration.GetConnectionString("NutriAIConnection");

// Registrar el DbContext con la inyección de dependencias
builder.Services.AddDbContext<Entidad.Context.NutriAIContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpClient<OllamaService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}");

app.Run();