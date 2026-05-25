
using complist_BACK;
using complist_BACK.Entities;
using complist_BACK.RequestHandlers;
using complist_BACK.RequestHandlers.MailService;
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


app.MapPost("/changeOrder/{pageName}", async (
    ApplicationContext db,
    string pageName,
    JsonElement data) =>
{
    /* =========================
       PHONES
    ========================= */

    if (pageName == "phones")
    {

        if (
            data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("depId", out var depIdProp)
        )
        {
            int depId = depIdProp.GetInt32();

            var sections = await db.Sections
                .Where(s => s.DepartmentId == depId)
                .ToListAsync();

            var sectionMap =
                sections.ToDictionary(s => s.Id);

            var items =
                data.GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
                int id =
                    item.GetProperty("id").GetInt32();

                int priority =
                    item.GetProperty("priority").GetInt32();

                if (
                    sectionMap.TryGetValue(
                        id,
                        out var section
                    )
                )
                {
                    section.PhonesPagePriority = priority;
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok();
        }

        /* =========================
           DEPARTMENTS REORDER
           payload:
           [
             { "id": 1, "priority": 10 }
           ]
        ========================= */

        var departments = await db.Departments
            .ToListAsync();

        var departmentMap =
            departments.ToDictionary(d => d.Id);

        foreach (var item in data.EnumerateArray())
        {
            int id =
                item.GetProperty("id").GetInt32();

            int priority =
                item.GetProperty("priority").GetInt32();

            if (
                departmentMap.TryGetValue(
                    id,
                    out var department
                )
            )
            {
                department.PhonesPagePriority =
                    priority;
            }
        }

        await db.SaveChangesAsync();

        return Results.Ok();
    }

    /* =========================
       DEFAULT
    ========================= */

    var mails = await db.Mails
        .ToListAsync();

    var mailMap =
        mails.ToDictionary(m => m.Id);

    foreach (var item in data.EnumerateArray())
    {
        int id =
            item.GetProperty("id").GetInt32();

        int priority =
            item.GetProperty("priority").GetInt32();

        if (
            mailMap.TryGetValue(
                id,
                out var mail
            )
        )
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
