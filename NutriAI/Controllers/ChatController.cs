using Microsoft.AspNetCore.Mvc;
using NutriAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutriAI.Controllers
{
    public class ChatController : Controller
    {
        // Simulación de base de datos en memoria
        private static List<ChatSession> _sessions = new List<ChatSession>();
        private static int _sessionIdCounter = 1;
        private static int _messageIdCounter = 1;

        public IActionResult Index()
        {
            var model = new ChatViewModel
            {
                CurrentSessionId = "",
                ChatSessions = _sessions.OrderByDescending(s => s.LastMessageTime).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                string responseMessage = GenerateNutritionResponse(request.Message);
                string sessionId = request.SessionId;
                string chatTitle = GenerateChatTitle(request.Message);
                bool isNewSession = false;

                // Buscar o crear sesión
                ChatSession session;
                if (string.IsNullOrEmpty(sessionId))
                {
                    // Crear nueva sesión
                    session = new ChatSession
                    {
                        Id = _sessionIdCounter++,
                        Title = chatTitle,
                        CreatedAt = DateTime.Now,
                        LastMessageTime = DateTime.Now,
                        MessageCount = 0
                    };
                    _sessions.Add(session);
                    isNewSession = true;
                    sessionId = session.Id.ToString();
                }
                else
                {
                    // Usar sesión existente
                    int id = int.Parse(sessionId);
                    session = _sessions.FirstOrDefault(s => s.Id == id);
                    if (session == null)
                    {
                        // Si no existe, crear nueva
                        session = new ChatSession
                        {
                            Id = _sessionIdCounter++,
                            Title = chatTitle,
                            CreatedAt = DateTime.Now,
                            LastMessageTime = DateTime.Now,
                            MessageCount = 0
                        };
                        _sessions.Add(session);
                        isNewSession = true;
                        sessionId = session.Id.ToString();
                    }
                    else
                    {
                        session.LastMessageTime = DateTime.Now;
                    }
                }

                // Agregar mensaje del usuario
                var userMessage = new ChatMessage
                {
                    Id = _messageIdCounter++,
                    SessionId = session.Id,
                    Message = request.Message,
                    IsUserMessage = true,
                    Timestamp = DateTime.Now
                };
                session.Messages.Add(userMessage);
                session.MessageCount++;

                // Agregar respuesta de la IA
                var aiMessage = new ChatMessage
                {
                    Id = _messageIdCounter++,
                    SessionId = session.Id,
                    Message = responseMessage,
                    IsUserMessage = false,
                    Timestamp = DateTime.Now
                };
                session.Messages.Add(aiMessage);
                session.MessageCount++;

                return Json(new SendMessageResponse
                {
                    Success = true,
                    Message = responseMessage,
                    SessionId = sessionId,
                    ChatTitle = session.Title,
                    UpdateHistory = isNewSession
                });
            }
            catch (Exception ex)
            {
                return Json(new SendMessageResponse
                {
                    Success = false,
                    Message = "Error: " + ex.Message
                });
            }
        }

        private string GenerateNutritionResponse(string userMessage)
        {
            // Tu lógica de respuesta aquí (la misma que antes)
            userMessage = userMessage.ToLower();

            if (userMessage.Contains("proteína") || userMessage.Contains("proteínas"))
            {
                return "Las proteínas son esenciales para construir y reparar tejidos...";
            }
            // ... resto de tu lógica
            else
            {
                return "Como nutricionista IA, puedo ayudarte con temas de nutrición...";
            }
        }

        private string GenerateChatTitle(string firstMessage)
        {
            if (firstMessage.Length > 30)
            {
                return firstMessage.Substring(0, 30) + "...";
            }
            return firstMessage;
        }

        public JsonResult GetChatHistory(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                return Json(session.Messages.OrderBy(m => m.Timestamp).ToList());
            }
            return Json(new List<ChatMessage>());
        }
    }
}