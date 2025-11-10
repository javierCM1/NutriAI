using Entidad.Context;
using Entidad.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace NutriAI.Services
{
    public class AuthService : IAuthService
    {
        private readonly NutriAIContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(NutriAIContext context, IPasswordHasher<Usuario> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<Usuario?> RegistrarAsync(string nombre, string email, string password)
        {
            // Verificar si el usuario ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
            {
                return null; // Usuario ya existe
            }
            // Crear nuevo usuario
            var usuario = new Usuario
            {
                Nombre = nombre,
                Email = email
            };

            // Hashear la contraseña
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, password);

            // Guardar en la base de datos
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }
        public async Task<Usuario?> LoginAsync(string email, string password)
        {
            // Buscar el usuario por email
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                return null; // Usuario no encontrado
            }
            // Verificar la contraseña
            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                return null; // Contraseña incorrecta
            }
            return usuario; // Login exitoso
        }

        public string GenerateJwtToken(Usuario usuario)
        {
            var claims = new List<Claim>
    {
        // 1. Identidad: ID y Email
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Email, usuario.Email),
        
        // 2. Rol del Usuario
        new Claim(ClaimTypes.Role, usuario.Rol)
    };

            // Obtener la clave secreta y la configuración de appsettings
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:SecurityKey"] ?? throw new InvalidOperationException("Clave JWT no configurada."))
            );

            // Credenciales de firma
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
