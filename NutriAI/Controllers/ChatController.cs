using Entidad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Models;
using NutriAI.Services;
using NutriAIServicio;
using System.Security.Claims;

namespace NutriAI.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly OllamaService _ollamaService;
        private readonly IChatService _chatService;

        public ChatController(OllamaService ollamaService, IChatService chatService)
        {
            _ollamaService = ollamaService;
            _chatService = chatService;
        }

        [Authorize(Roles = "Usuario,Admin")]
        public IActionResult Index()
        {
            // Solo devuelve la vista con un modelo vacío; la sesión real se carga vía JS
            var model = new ChatViewModel
            {
                CurrentSessionId = "",
                ChatSessions = new List<ChatSession>(),
                UserInfo = null,
                ChatMessages = new List<ChatMessage>()
            };
            return View(model);
        }

        [Authorize(Roles = "Usuario,Admin")]
        public async Task<IActionResult> GetInitialData()
        {
            try
            {
                int userId = GetUserIDFromToken();

                // 🔹 Obtener usuario completo con UserInfo
                var usuario = await _chatService.GetUsuarioWithUserInfoAsync(userId);

                // 🔹 Crear UserInfo si no existe
                if (usuario.UserInfo == null)
                {
                    var newUserInfo = new UserInfo
                    {
                        UsuarioId = userId,
                        Edad = null,
                        Altura = null,
                        Peso = null,
                        PreferenciaAlimenticia = ""
                    };
                    await _chatService.UpdateUserInfoAsync(userId, newUserInfo);
                    usuario = await _chatService.GetUsuarioWithUserInfoAsync(userId); // recargar
                }

                // 🔹 Obtener todas las sesiones
                var allSessions = await _chatService.GetAllSessionsByUserAsync(userId);

                // 🔹 Obtener sesión actual o crear nueva
                var currentSession = allSessions.LastOrDefault()
                                     ?? await _chatService.GetOrCreateCurrentSessionAsync(userId);

                // 🔹 Obtener mensajes de la sesión actual
                var messages = await _chatService.GetSessionMessagesAsync(currentSession.Id);

                return Json(new
                {
                    currentSessionId = currentSession.Id,
                    chatSessions = allSessions,
                    userInfo = usuario.UserInfo,
                    chatMessages = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Usuario,Admin")]
        [HttpPost]
        public async Task<IActionResult> GuardarUserInfo(UserInfo userInfo)
        {
            if (!ModelState.IsValid)
                return Json(new { mensaje = "Error: datos inválidos" });

            try
            {
                int userId = GetUserIDFromToken();
                userInfo.UsuarioId = userId;

                await _chatService.UpdateUserInfoAsync(userId, userInfo);

                var respuesta = await _ollamaService.GetNutritionResponseAsync(
                    userInfo.sexo ?? "No especificado",
                    userInfo.Edad ?? 0,
                    (double)(userInfo.Peso ?? 0),
                    (double)(userInfo.Altura ?? 0),
                    userInfo.PreferenciaAlimenticia ?? "",
                    userInfo.Objetivo ?? "No especificado",
                    "Hola, acabo de registrar mis datos. ¿Podrías darme una recomendación nutricional general para mi perfil?"
                );

                return Json(new { mensaje = respuesta });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { mensaje = "ERROR DE BD: " + errorMessage });
            }
        }

        [Authorize(Roles = "Usuario,Admin")]
        [HttpPost]
        public async Task<IActionResult> EnviarMensaje([FromForm] string mensaje, [FromForm] int sessionId)
        {
            try
            {
                int userId = GetUserIDFromToken();

                var session = await _chatService.GetSessionByIdAsync(sessionId, userId);
                if (session == null)
                    return Json(new { respuesta = "Error: sesión no encontrada o no pertenece al usuario." });

                var userInfo = session.Usuario?.UserInfo;
                if (userInfo == null)
                    return Json(new { respuesta = "Error: completa tu perfil antes de enviar mensajes." });

                await _chatService.AddMessageAsync(session.Id, mensaje, true, userId);

                var respuestaIA = await _ollamaService.GetNutritionResponseAsync(
                    userInfo.sexo ?? "No especificado",
                    userInfo.Edad ?? 0,
                    (double)(userInfo.Peso ?? 0),
                    (double)(userInfo.Altura ?? 0),
                    userInfo.PreferenciaAlimenticia ?? "",
                    userInfo.Objetivo ?? "No especificado",
                    mensaje
                );

                await _chatService.AddMessageAsync(session.Id, respuestaIA, false, userId);

                return Json(new { respuesta = respuestaIA });
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error al comunicarse con la IA: " + ex.Message });
            }
        }

        [Authorize(Roles = "Usuario,Admin")]
        [HttpPost]
        public async Task<IActionResult> NuevaSesion()
        {
            try
            {
                int userId = GetUserIDFromToken();
                var session = await _chatService.CreateNewSessionAsync(userId);
                return Json(new
                {
                    success = true,
                    sessionId = session.Id,
                    title = session.Title,
                    messageCount = session.MessageCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Usuario,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            try
            {
                int userId = GetUserIDFromToken();
                var messages = await _chatService.GetMessagesBySessionAsync(sessionId, userId);
                return Json(new { success = true, messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Usuario,Admin")]
        [HttpDelete]
        public async Task<IActionResult> BorrarSesion(int id)
        {
            try
            {
                int userId = GetUserIDFromToken();
                var result = await _chatService.DeleteSessionAsync(id, userId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetUserIDFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            throw new InvalidOperationException("Claim de ID de usuario no válido.");
        }
    }
}