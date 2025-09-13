using ASP_421.Data.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_421.Middleware
{
    public class AuthSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.ContainsKey("logout"))
            {
                context.Session.Remove("SignIn");
                context.Response.Redirect(context.Request.Path);
                return;
            }

            if (context.Session.Keys.Contains("SignIn"))
            {
                UserAccess userAccess =
                    JsonSerializer.Deserialize<UserAccess>(
                        context.Session.GetString("SignIn")!)!;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userAccess.User.Name),
                    new Claim(ClaimTypes.Email, userAccess.User.Email),
                    new Claim("Id", userAccess.User.Id.ToString()),
                };

                // Добавляем сведения о дате рождения, если она есть
                if (userAccess.User.Birthdate.HasValue)
                {
                    claims.Add(new Claim(ClaimTypes.DateOfBirth, userAccess.User.Birthdate.Value.ToString("yyyy-MM-dd")));
                }

                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        claims,
                        nameof(AuthSessionMiddleware)
                    )
                );
            }
            await _next(context);
        }
    }

    public static class AuthSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthSession(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthSessionMiddleware>();
        }
    }

}
