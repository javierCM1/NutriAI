using Entidad.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using NutriAI.Services;
using NutriAIServicio;
using Entidad.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;

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

// =========================================================================
// CONFIGURACIÓN JWT 
// =========================================================================

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 1. RESTAURAR LA VALIDACIÓN (Esto faltaba en tu código)
        var key = builder.Configuration["JwtSettings:SecurityKey"] ?? throw new ArgumentNullException("La clave JWT no está configurada.");
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

        // 2. MOVER LA LÓGICA DE REDIRECCIÓN AL LUGAR CORRECTO
        options.Events = new JwtBearerEvents
        {
            // 'context' SÍ existe dentro de OnChallenge
            OnChallenge = context =>
            {
                // Si la solicitud es una API (fetch), devolvemos 401
                if (context.Request.Path.StartsWithSegments("/Chat/GetInitialData") ||
                    context.Request.Path.StartsWithSegments("/Chat/GuardarUserInfo") ||
                    context.Request.Path.StartsWithSegments("/Chat/EnviarMensaje") ||
                    context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.HandleResponse();
                    return Task.CompletedTask;
                }

                // Si es una navegación de navegador (a Index), redirigimos
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();