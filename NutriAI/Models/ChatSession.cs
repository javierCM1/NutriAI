using System;
using System.Collections.Generic;

namespace NutriAI.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int MessageCount { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}