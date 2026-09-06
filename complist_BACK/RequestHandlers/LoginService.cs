namespace complist_BACK.RequestHandlers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.EntityFrameworkCore;
    using complist_BACK.Entities;
    using Microsoft.AspNetCore.Identity.Data;
    using System;

    public static class LoginService
    {
        public static async Task<IResult> Login(
            Login request,
            ApplicationContext db,
            HttpContext httpContext)
        {
            var user = await db.Set<Login>()
                .FirstOrDefaultAsync(x => x.LoginName == request.LoginName);

            if (user == null)
                return Results.BadRequest("User not found");

            if (user.Password != request.Password)
                return Results.BadRequest("Invalid password");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.LoginName),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddYears(10)
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            return Results.Ok(new
            {
                success = true,
                message = "Logged in successfully",
                userName = user.LoginName
            });
        }

        public static async Task<IResult> LogOut(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { message = "Logged out successfully" });
        }

        public static IResult ChechAuthorization(HttpContext context)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return Results.Unauthorized();

            var authData = new
            {
                success = true,
                message = "Logged in successfully",
                userName = context.User.Identity?.Name
            };

            return Results.Ok(authData);
        }
    }
}