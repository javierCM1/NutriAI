using Entidad.Models;

namespace NutriAI.Services
{
    public interface IChatService
    {
        // Obtiene o crea una sesión de chat para el usuario dado
        Task<ChatSession> GetOrCreateCurrentSessionAsync(int userId);

        // guarda el mensaje de el usuario y la IA en la base de datos
        Task<ChatMessage> AddMessageAsync(int sessionId, string message, bool isUserMessage);

        // Actualiza la información del usuario en la base de datos
        Task UpdateUserInfoAsync(int userId, UserInfo userInfoData);

        // Obtiene todos los mensajes de una sesión de chat específica
        Task<List<ChatMessage>> GetSessionMessagesAsync(int sessionId);
    }
}
