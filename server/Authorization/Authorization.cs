using System.Text.Json;
using server.Classes;
using server.Enums;

namespace server.Authorization;

public static class Authorization
{
    public static RouteHandlerBuilder RoleAuthorization(this RouteHandlerBuilder builder, params Role[] roles)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var userJson = httpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userJson))
            {
                return Results.Unauthorized();
            }

            var user = JsonSerializer.Deserialize<User>(userJson);
            if (!roles.Contains(user.Role))
            {
                return Results.Json(new { message = "Access denied." }, statusCode: 403);
            }

            return await next(context);
        });
    }
}