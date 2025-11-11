using Entidad.Models;
using NutriAI.Models.DTOs;

namespace NutriAI.Services
{
    public interface IChatService
    {
        // Obtiene o crea una sesión de chat para el usuario dado
        Task<ChatSession> GetOrCreateCurrentSessionAsync(int userId);

        // guarda el mensaje de el usuario y la IA en la base de datos
        Task<ChatMessage> AddMessageAsync(int sessionId, string message, bool isUserMessage, int userId);

        // Actualiza la información del usuario en la base de datos
        Task UpdateUserInfoAsync(int userId, UserInfo userInfoData);

        Task<Usuario> GetUsuarioWithUserInfoAsync(int userId);

        // Obtiene todos los mensajes de una sesión de chat específica
        Task<List<ChatMessage>> GetSessionMessagesAsync(int sessionId);


        Task<ChatSession> CreateNewSessionAsync(int usuarioId);


        Task<List<ChatMessageDto>> GetMessagesBySessionAsync(int sessionId, int userId);

        Task<bool> DeleteSessionAsync(int sessionId, int userId);

        Task<ChatSession?> GetSessionByIdAsync(int sessionId, int userId);

        Task<List<ChatSession>> GetAllSessionsByUserAsync(int userId);







    }
}
