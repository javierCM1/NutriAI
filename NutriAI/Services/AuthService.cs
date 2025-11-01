using Entidad.Context;
using Entidad.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace NutriAI.Services
{
    public class AuthService : IAuthService
    {
        private readonly NutriAIContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public AuthService(NutriAIContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
    }
}
