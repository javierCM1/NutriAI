using Entidad.Context;
using Entidad.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NutriAI.MiddleWare;
using NutriAI.Services;
using NutriAIServicio;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Configuración de la Base de Datos
var connectionString = builder.Configuration.GetConnectionString("NutriAIConnection");
builder.Services.AddDbContext<Entidad.Context.NutriAIContext>(options =>
    options.UseSqlServer(connectionString));

// Servicios de Autenticación y App
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<OllamaService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // duración de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // No ponemos options.Cookie.Expires -> así se elimina al cerrar el navegador
});


// =========================================================================
// CONFIGURACIÓN JWT 
// =========================================================================

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["JwtSettings:SecurityKey"]
                  ?? throw new ArgumentNullException("La clave JWT no está configurada.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };

        // 🔥 Leer el token automáticamente desde la cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwt_token"))
                {
                    context.Token = context.Request.Cookies["jwt_token"];
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var path = context.Request.Path;

                if (path.StartsWithSegments("/api/Auth/login", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWithSegments("/api/Auth/register", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.CompletedTask;
                }

                // 🚪 Si es vista web, redirigir al login
                context.Response.Redirect("/Account/Login");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
// =========================================================================

var app = builder.Build();

// Configuración de Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();


app.UseMiddleware<SessionCheckMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();