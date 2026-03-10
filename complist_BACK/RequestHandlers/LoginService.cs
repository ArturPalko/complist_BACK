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
            // 1️⃣ Знайти користувача
            var user = await db.Set<Login>()
                .FirstOrDefaultAsync(x => x.LoginName == request.LoginName);

            if (user == null)
                return Results.BadRequest("User not found");

            // 2️⃣ Перевірити пароль (ПОКИ без хешування)
            if (user.Password != request.Password)
                return Results.BadRequest("Invalid password");

            // 3️⃣ Створити claims
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

            // 4️⃣ Авторизація через cookie
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

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

