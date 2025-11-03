using Entidad.Context;
using Entidad.Models;
using Microsoft.EntityFrameworkCore;

namespace NutriAI.Services
{
    public class ChatService : IChatService
    {
        private readonly NutriAIContext _context;

        public ChatService(NutriAIContext context)
        {
            _context = context;
        }

        public async Task UpdateUserInfoAsync(int userId, UserInfo userInfoData)
        {
            // 1. Intentar encontrar el perfil existente
            var existingProfile = await _context.UserInfos
                                                .FirstOrDefaultAsync(ui => ui.UsuarioId == userId);

            if (existingProfile == null)
            {
                // SCENARIO 1: CREAR NUEVO PERFIL
                var newProfile = new UserInfo
                {
                    UsuarioId = userId, // Vinculación clave
                    Edad = userInfoData.Edad,
                    Peso = userInfoData.Peso,
                    Altura = userInfoData.Altura,
                    PreferenciaAlimenticia = userInfoData.PreferenciaAlimenticia
                };

                _context.UserInfos.Add(newProfile);
            }
            else
            {
                // SCENARIO 2: ACTUALIZAR PERFIL EXISTENTE
                existingProfile.Edad = userInfoData.Edad;
                existingProfile.Peso = userInfoData.Peso;
                existingProfile.Altura = userInfoData.Altura;
                existingProfile.PreferenciaAlimenticia = userInfoData.PreferenciaAlimenticia;

                // No es estrictamente necesario, pero garantiza que EF Core marque como modificado
                _context.UserInfos.Update(existingProfile);
            }

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();
        }

        public async Task<ChatSession> GetOrCreateCurrentSessionAsync(int userId)
        {
            // Busca la última sesión activa del usuario.
            var session = await _context.ChatSessions
                                        .Include(s => s.Usuario)
                                        .ThenInclude(u => u.UserInfo)
                                        .Where(s => s.UsuarioId == userId)
                                        .OrderByDescending(s => s.LastMessageTime)
                                        .FirstOrDefaultAsync();

            // Si no hay sesión, crea una nueva.
            if (session == null)
            {
                var user = await _context.Usuarios.Include(u => u.UserInfo)
                                                  .FirstOrDefaultAsync(u => u.Id == userId);

                session = new ChatSession
                {
                    UsuarioId = userId,
                    Title = "Nueva Conversación",
                    CreatedAt = DateTime.Now,
                    MessageCount = 0
                };
                _context.ChatSessions.Add(session);
                await _context.SaveChangesAsync();
            }

            return session;
        }

        public async Task<ChatMessage> AddMessageAsync(int sessionId, string message, bool isUserMessage)
        {
            var chatMessage = new ChatMessage
            {
                SessionId = sessionId,
                Message = message,
                IsUserMessage = isUserMessage,
                Timestamp = DateTime.Now
            };
            // Agrega el mensaje a la base de datos.
            _context.ChatMessages.Add(chatMessage);

            // Actualiza el conteo de mensajes y la última hora del mensaje en la sesión.
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session != null)
            {
                // Incrementa el conteo de mensajes y actualiza la última hora del mensaje.
                session.MessageCount += 1;
                session.LastMessageTime = DateTime.Now;
            }

            // Guarda los cambios en la base de datos.
            await _context.SaveChangesAsync();
            return chatMessage;
        }

        public async Task<List<ChatMessage>> GetSessionMessagesAsync(int sessionId)
        {
            return await _context.ChatMessages
                                 .Where(m => m.SessionId == sessionId)
                                 .OrderBy(m => m.Timestamp)
                                 .ToListAsync();
        }
    }


}
