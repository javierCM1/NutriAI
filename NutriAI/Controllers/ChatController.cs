using Entidad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Models;
using NutriAI.Services;
using NutriAIServicio;
using System.Security.Claims;

namespace NutriAI.Controllers
{
    public class ChatController : Controller
    {
        private readonly OllamaService _ollamaService;
        private readonly IChatService _chatService;

        // Inyección de dependencias de los servicios
        public ChatController(OllamaService ollamaService, IChatService chatService)
        {
            _ollamaService = ollamaService;
            _chatService = chatService;
        }

        public IActionResult Index()
        {

            var model = new ChatViewModel
            {
                CurrentSessionId = "",
                ChatSessions = new List<ChatSession>(),
                UserInfo = null,
                ChatMessages = new List<ChatMessage>()
            };

            return View(model);
        }

        // NutriAI/Controllers/ChatController.cs

        [Authorize(Roles = "Usuario, Admin")]
        public async Task<IActionResult> GetInitialData()
        {
            try
            {

                int userId = GetUserIDFromToken();

                var currentSession = await _chatService.GetOrCreateCurrentSessionAsync(userId);
                var messages = await _chatService.GetSessionMessagesAsync(currentSession.Id);

                // Devolvemos los datos como JSON
                return Json(new
                {
                    currentSessionId = currentSession.Id.ToString(),
                    chatSessions = new List<ChatSession> { currentSession },
                    userInfo = currentSession.Usuario?.UserInfo,
                    chatMessages = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // OBTENER USER ID DESDE EL TOKEN JWT
        private int GetUserIDFromToken()
        {
            // NO NECESITAS TRY/CATCH AQUÍ. El [Authorize] ya garantiza que el Claim existe.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convertimos el Claim a entero de forma segura.
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            // Si falla la conversión, lanzamos una excepción, pero el [Authorize]
            // ya debería haber atrapado el problema.
            throw new InvalidOperationException("Claim de ID de usuario no válido.");
        }

        // GUARDAR USER INFO (Actualiza perfil y llama a la IA)
        [Authorize(Roles = "Usuario, Admin")]
        [HttpPost]
        public async Task<IActionResult> GuardarUserInfo(UserInfo userInfo)
        {
            if (!ModelState.IsValid)
                return Json(new { mensaje = "Error: datos inválidos" });

            try
            {
                int userId = GetUserIDFromToken();

                // 1. GUARDAR/ACTUALIZAR UserInfo en la BD a través del servicio
                userInfo.UsuarioId = userId; // Vincula el perfil al usuario actual
                await _chatService.UpdateUserInfoAsync(userId, userInfo); // <--- LÍNEA DONDE FALLA LA BD

                // 2. Llamada inicial a la IA
                var respuesta = await _ollamaService.GetNutritionResponseAsync(
                    userInfo.Edad ?? 0,
                    (double)(userInfo.Peso ?? 0),
                    (double)(userInfo.Altura ?? 0),
                    userInfo.PreferenciaAlimenticia ?? "",
                    "Hola, acabo de registrar mis datos. ¿Podrías darme una recomendación nutricional general para mi perfil?"
                   );

                // 3. Devolver la respuesta de la IA al cliente
                return Json(new { mensaje = respuesta });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            // ESTE BLOQUE CAPTURA EL ERROR DE LA BASE DE DATOS Y LO EXPONE
            catch (Exception ex)
            {
                // Extrae el mensaje de error de la excepción interna (el error real de SQL Server o EF Core)
                string errorMessage = ex.InnerException?.Message ?? ex.Message;

                // Devolvemos el mensaje de error de la BD al cliente para el diagnóstico
                return Json(new { mensaje = "ERROR DE BD: " + errorMessage });
            }
        }

        // ENVIAR MENSAJE (Guarda en BD y llama a la IA)
        [Authorize(Roles = "Usuario, Admin")]
        [HttpPost]
        public async Task<IActionResult> EnviarMensaje([FromForm] string mensaje)
        {
            try
            {
                int userId = GetUserIDFromToken();

                // 1. Obtener sesión activa y UserInfo asociado
                var session = await _chatService.GetOrCreateCurrentSessionAsync(userId);
                var userInfo = session.Usuario?.UserInfo;

                if (userInfo == null)
                    return Json(new { respuesta = "Error: Datos de perfil no encontrados. Completa el formulario." });

                // 2. GUARDAR MENSAJE DEL USUARIO en la BD
                await _chatService.AddMessageAsync(session.Id, mensaje, true);

                // 3. Llamada al servicio de IA
                var respuestaIA = await _ollamaService.GetNutritionResponseAsync(
                    userInfo.Edad ?? 0,
                    (double)(userInfo.Peso ?? 0),
                    (double)(userInfo.Altura ?? 0),
                    userInfo.PreferenciaAlimenticia ?? "",
                    mensaje
                );

                // 4. GUARDAR RESPUESTA DE LA IA en la BD
                await _chatService.AddMessageAsync(session.Id, respuestaIA, false);

                return Json(new { respuesta = respuestaIA });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch
            {
                return Json(new { respuesta = "Error al comunicarse con el servicio de IA o al guardar el mensaje." });
            }
        }
    }
}