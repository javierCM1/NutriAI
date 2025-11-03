
using Microsoft.AspNetCore.Mvc;
using NutriAI.Services;
using NutriAI.Models.DTOs;

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
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuario = await _authService.LoginAsync(request.Email, request.Password);

        if (usuario == null)
        {
            // 401 Unauthorized: Credenciales incorrectas.
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        // Generar el token JWT
        var token = _authService.GenerateJwtToken(usuario);

        // Devolver el token en la respuesta
        // 200 OK: Login exitoso.
        return Ok(new { message = "Login exitoso.", token = token, email = usuario.Email, rol = usuario.Rol });
    }
}