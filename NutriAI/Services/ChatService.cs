using Entidad.Context;
using Entidad.Models;
using Microsoft.EntityFrameworkCore;
using NutriAI.Models.DTOs;

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

        public async Task<ChatSession> CreateNewSessionAsync(int usuarioId)
        {
            var session = new ChatSession
            {
                UsuarioId = usuarioId,
                Title = "Nueva Conversacion",
                MessageCount = 0,
                CreatedAt = DateTime.Now
            };


            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<List<ChatMessageDto>> GetMessagesBySessionAsync(int sessionId, int userId)
        {
            return await _context.ChatMessages
                .Where(m => m.SessionId == sessionId && m.Session.UsuarioId == userId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Message = m.Message,
                    IsUserMessage = m.IsUserMessage,
                    CreatedAt = m.Timestamp
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteSessionAsync(int sessionId, int userId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.ChatMessages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UsuarioId == userId);

            if (session == null)
                return false;

            _context.ChatMessages.RemoveRange(session.ChatMessages);
            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ChatSession?> GetSessionByIdAsync(int sessionId, int userId)
        {
            return await _context.ChatSessions
                .Include(s => s.Usuario)
                .ThenInclude(u => u.UserInfo)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UsuarioId == userId);
        }


        public async Task<List<ChatSession>> GetAllSessionsByUserAsync(int userId)
        {
            return await _context.ChatSessions
                .Where(s => s.UsuarioId == userId)
                .OrderByDescending(s => s.LastMessageTime)
                .ToListAsync();
        }



    }


}
