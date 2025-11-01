namespace NutriAI.Models.DTOs
{
    public class LoginRequest
    {
        public required string Email { get; set; } = null!;
        public required string Password { get; set; } = null!;
    }
    public class RegistroRequest : LoginRequest
    {
        public required string Nombre { get; set; }
    }
}
