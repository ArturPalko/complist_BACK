
using complist_BACK;
using complist_BACK.Entities;
using complist_BACK.RequestHandlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.VisualBasic;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowFrontend", policy =>
{
    policy.WithOrigins("http://localhost:3000") 
          .AllowAnyHeader()
          .AllowAnyMethod()
         .AllowCredentials(); // <- критичн
});
});

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

// Cookie аутентифікація
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";        // куди редіректити якщо не аутентифіковано
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

}); // якщо будеш використовувати [Authorize]
var app = builder.Build();
app.UseCors("AllowFrontend");
app.UseAuthentication(); // Обов’язково перед UseAuthorization та MapPost
app.UseAuthorization();



app.Map("/mails/{mailType}", async (string mailType, ApplicationContext db) =>
{
    return await MailsService.GetMails(mailType, db);
});

app.Map("/phones/", async (ApplicationContext db) =>
{
    return await PhonesService.GetPhones(db);
});

app.Map("/mails/{mailType}/passwords", async (string mailType, ApplicationContext db) =>
{
    return await MailsService.GetMailsPasswords(mailType, db);
})
.RequireAuthorization("AdminOnly"); // ?? тільки для admin


app.MapGet("/checkAuth", (HttpContext context) =>
{
    return LoginService.ChechAuthorization(context);
})
.RequireAuthorization();

app.MapPost("/login",  (
    Login request,
    ApplicationContext db,
    HttpContext httpContext) =>
{
    return  LoginService.Login(request, db, httpContext);
});

app.MapPost("/logout",  (HttpContext context) =>
{
    return  LoginService.LogOut(context);
});


app.MapPost("/changeOrder/{pageName}", async (ApplicationContext db, string pageName, JsonElement data) =>
{
  

    var mails = await db.Mails
        .ToListAsync();

    foreach (var item in data.EnumerateArray())
    {
        int id = item.GetProperty("id").GetInt32();
        int priority = item.GetProperty("priority").GetInt32();

        var mail = mails.FirstOrDefault(m => m.Id == id);

        if (mail != null)
        {
            mail.Priority = priority;
        }
    }

    await db.SaveChangesAsync();

    return Results.Ok();
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
