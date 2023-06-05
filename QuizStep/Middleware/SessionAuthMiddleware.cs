using System.Security.Claims;
using QuizStep.Data;
using QuizStep.Data.Entity;

namespace QuizStep.Middleware;

public class SessionAuthMiddleware
{
    private readonly RequestDelegate _next;
       
    public SessionAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,   
        ILogger<SessionAuthMiddleware> logger,           
        DataContext dataContext) 
    {
        // дії Middleware
        String? userId = context.Session.GetString("authUserId");
        if(userId is not null)
        {
            try
            {
                User? user = dataContext.Users.Find(Guid.Parse(userId));
                if(user is not null)
                {
                    // зберігаємо у контексті запиту
                    context.Items.Add("authUser", user);
                    Claim[] claims = new Claim[]
                    {
                        new Claim(ClaimTypes.Sid, userId),
                        new Claim(ClaimTypes.Name, user.RealName),
                        new Claim(ClaimTypes.NameIdentifier, user.Login),
                        new Claim(ClaimTypes.UserData, user.Avatar ?? string.Empty),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
                        
                    var principal = new ClaimsPrincipal(
                        new ClaimsIdentity(claims,
                            nameof(SessionAuthMiddleware)));
            
                    context.User = principal;
                }
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex, "SessionAuthMiddleware");
            }
        }

        logger.LogInformation("SessionAuthMiddleware works");

        await _next(context);  
    }
}

public static class SessionAuthMiddlewareExtension
{
    public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SessionAuthMiddleware>();
    }
}