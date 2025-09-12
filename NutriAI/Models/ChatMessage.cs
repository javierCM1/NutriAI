using System;

namespace NutriAI.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Message { get; set; }
        public bool IsUserMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}