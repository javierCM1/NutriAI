using Entidad.Models;

namespace NutriAI.Services
{
    public interface IAuthService
    {
        Task<Usuario?> RegistrarAsync(string nombre, string email, string password);
        Task<Usuario?> LoginAsync(string email, string password);
    
        string GenerateJwtToken(Usuario usuario);
    }
}
