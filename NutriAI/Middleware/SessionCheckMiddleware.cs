namespace NutriAI.MiddleWare
{
    public class SessionCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/Chat") || path.StartsWithSegments("/api/Chat"))
            {
                if (context.Session.GetInt32("UserId") == null)
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
