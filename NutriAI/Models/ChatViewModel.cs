using System.Collections.Generic;
using Entidad.Models;
namespace NutriAI.Models
{
    public class ChatViewModel
    {
        public string CurrentSessionId { get; set; }
        public List<ChatSession> ChatSessions { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}