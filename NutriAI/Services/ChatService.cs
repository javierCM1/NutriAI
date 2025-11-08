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

        public async Task<Usuario> GetUsuarioWithUserInfoAsync(int userId)
        {
            return await _context.Usuarios
                .Include(u => u.UserInfo)
                .Include(u => u.ChatSessions)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Usuario no encontrado.");
        }

        public async Task UpdateUserInfoAsync(int userId, UserInfo userInfoData)
        {
            var existingProfile = await _context.UserInfos
                .FirstOrDefaultAsync(ui => ui.UsuarioId == userId);

            if (existingProfile == null)
            {
                var newProfile = new UserInfo
                {
                    UsuarioId = userId,
                    Edad = userInfoData.Edad,
                    Peso = userInfoData.Peso,
                    Altura = userInfoData.Altura,
                    PreferenciaAlimenticia = userInfoData.PreferenciaAlimenticia
                };
                _context.UserInfos.Add(newProfile);
            }
            else
            {
                existingProfile.Edad = userInfoData.Edad;
                existingProfile.Peso = userInfoData.Peso;
                existingProfile.Altura = userInfoData.Altura;
                existingProfile.PreferenciaAlimenticia = userInfoData.PreferenciaAlimenticia;

                _context.UserInfos.Update(existingProfile);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ChatSession> GetOrCreateCurrentSessionAsync(int userId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Usuario)
                .ThenInclude(u => u.UserInfo)
                .Where(s => s.UsuarioId == userId)
                .OrderByDescending(s => s.LastMessageTime)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.UserInfo)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (usuario.UserInfo == null)
                {
                    usuario.UserInfo = new UserInfo { UsuarioId = userId };
                    _context.UserInfos.Add(usuario.UserInfo);
                    await _context.SaveChangesAsync();
                }

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

        public async Task<ChatMessage> AddMessageAsync(int sessionId, string message, bool isUserMessage, int userId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UsuarioId == userId);

            if (session == null)
                throw new UnauthorizedAccessException("Sesión no pertenece al usuario.");

            var chatMessage = new ChatMessage
            {
                SessionId = sessionId,
                Message = message,
                IsUserMessage = isUserMessage,
                Timestamp = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            session.MessageCount += 1;
            session.LastMessageTime = DateTime.Now;

            await _context.SaveChangesAsync();
            return chatMessage;
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
                Title = "Nueva Conversación",
                CreatedAt = DateTime.Now,
                MessageCount = 0
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<List<ChatSession>> GetAllSessionsByUserAsync(int userId)
        {
            return await _context.ChatSessions
                .Where(s => s.UsuarioId == userId)
                .OrderByDescending(s => s.LastMessageTime)
                .ToListAsync();
        }

        public async Task<ChatSession?> GetSessionByIdAsync(int sessionId, int userId)
        {
            return await _context.ChatSessions
                .Include(s => s.Usuario)
                .ThenInclude(u => u.UserInfo)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UsuarioId == userId);
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
    }
}
