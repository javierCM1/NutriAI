namespace NutriAI.Models
{
    public class SendMessageResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
        public string ChatTitle { get; set; }
        public bool UpdateHistory { get; set; }
    }
}