using Microsoft.AspNetCore.Mvc;
using Entidad.Models;
using NutriAI.Models;
using NutriAIServicio;
using System.Text.Json;
using System.Threading.Tasks;

namespace NutriAI.Controllers
{
    public class ChatController : Controller
    {
        private static List<ChatSession> _sessions = new List<ChatSession>();
        private readonly OllamaService _ollamaService;

        public ChatController(OllamaService ollamaService)
        {
            _ollamaService = ollamaService;
        }

        public IActionResult Index()
        {
            var model = new ChatViewModel
            {
                CurrentSessionId = "",
                ChatSessions = _sessions,
                UserInfo = null
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarUserInfo(UserInfo userInfo)
        {
            if (!ModelState.IsValid)
                return Json(new { mensaje = "Error: datos inválidos" });

            HttpContext.Session.SetString("UserInfo", JsonSerializer.Serialize(userInfo));

            try
            {
                var respuesta = await _ollamaService.GetNutritionResponseAsync(
                    userInfo.Edad ?? 0,
                    (double)(userInfo.Peso ?? 0),
                    (double)(userInfo.Altura ?? 0),
                    userInfo.PreferenciaAlimenticia ?? "",
                    "Hola, acabo de registrar mis datos. ¿Podrías darme una recomendación nutricional general para mi perfil?"
                 );

                return Json(new { mensaje = respuesta });
            }
            catch
            {
                return Json(new { mensaje = "Error al comunicarse con el servicio de IA." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensaje([FromForm] string mensaje)
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfoJson))
                return Json(new { respuesta = "Primero completa tus datos en el formulario." });

            var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson);

            try
            {
                var respuesta = await _ollamaService.GetNutritionResponseAsync(
                userInfo.Edad ?? 0,
                (double)(userInfo.Peso ?? 0),
                (double)(userInfo.Altura ?? 0),
                userInfo.PreferenciaAlimenticia ?? "",
                mensaje
                );

                return Json(new { respuesta = respuesta });
            }
            catch
            {
                return Json(new { respuesta = "Error al comunicarse con el servicio de IA." });
            }
        }
    }
}