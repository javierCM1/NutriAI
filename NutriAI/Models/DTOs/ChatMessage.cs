namespace NutriAI.Models.DTOs
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool IsUserMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
