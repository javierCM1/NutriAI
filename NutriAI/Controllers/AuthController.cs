
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Models.DTOs;
using NutriAI.Services;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    // POST /api/Auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistroRequest request)
    {
        // 1. Validación de Entrada
        if (!ModelState.IsValid)
        {
            // 400 Bad Request: Datos inválidos.
            return BadRequest(ModelState);
        }
        // Intentar registrar al usuario
        var usuario = await _authService.RegistrarAsync(request.Nombre, request.Email, request.Password);

        // 2. Manejo de Respuesta
        if (usuario == null)
        {
            return Conflict(new { message = "El email ya se encuentra registrado." });
        }

        return CreatedAtAction(nameof(Register), new { message = "Registro exitoso.", id = usuario.Id });
    }

    // POST /api/Auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuario = await _authService.LoginAsync(request.Email, request.Password);
        if (usuario == null)
            return Unauthorized(new { message = "Credenciales inválidas." });

        var token = _authService.GenerateJwtToken(usuario);

        // 1️⃣ Guardar JWT en cookie
        Response.Cookies.Append("jwt_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Expires = null
        });

        // 2️⃣ Guardar info de usuario en session
        HttpContext.Session.SetInt32("UserId", usuario.Id);
        HttpContext.Session.SetString("Rol", usuario.Rol);

        return Ok(new { message = "Login exitoso.", email = usuario.Email, rol = usuario.Rol });
    }


    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Limpiar session
        HttpContext.Session.Clear();

        // Eliminar cookie JWT
        Response.Cookies.Delete("jwt_token");

        return Ok(new { message = "Logout exitoso" });
    }

}