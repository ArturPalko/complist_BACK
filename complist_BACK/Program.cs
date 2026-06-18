
using complist_BACK;
using complist_BACK.Entities;
using complist_BACK.RequestHandlers;
using complist_BACK.RequestHandlers.MailService;
using complist_BACK.RequestHandlers.PositionService;
using complist_BACK.RequestHandlers.UserTypeService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.VisualBasic;
using System;
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

string? connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(connection));
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


/*app.MapGet("/dictionaries", async (ApplicationContext db) =>
{
    var positions = await db.Positions
        .OrderBy(p => p.Priority)
        .Select(p => new
        {
            id = p.Id,
            positionName = p.Name,
            priority = p.Priority
        })
        .ToListAsync();

    var userTypes = await db.UserTypes
        .OrderBy(t => t.Priority)
        .Select(t => new
        {
            id = t.Id,
            userType = t.Name,
            priority = t.Priority
        })
        .ToListAsync();

    return Results.Ok(new
    {
        positions,
        userTypes
    });
});
*/
app.MapGet("/dictionaries", async (ApplicationContext db) =>
{
    var positions = await db.Positions
        .OrderBy(p => p.Priority)
        .Select(p => new
        {
            id = p.Id,
            positionName = p.Name,
            priority = p.Priority
        })
        .ToListAsync();

    var userTypes = await db.UserTypes
        .OrderBy(t => t.Priority)
        .Select(t => new
        {
            id = t.Id,
            userType = t.Name,
            priority = t.Priority
        })
        .ToListAsync();

    var departments = await db.Departments
        .Include(d => d.Sections)
        .OrderBy(d => d.PhonesPagePriority)
        .Select(d => new
        {
            id = d.Id,
            name = d.Name,
            priority = d.PhonesPagePriority,

            sections = d.Sections
                .OrderBy(s => s.PhonesPagePriority)
                .Select(s => new
                {
                    sectionId = s.Id,
                    sectionName = s.Name,
                    sectionPriority = s.PhonesPagePriority,
                    departmentId = s.Department.Id
                })
                .ToList()
        })
        .ToListAsync();

    return Results.Ok(new
    {
        positions,
        userTypes,
        departments
    });
});

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
       PHONES LOGIC
    ========================= */
    if (pageName == "phones")
    {
        var mode = data.TryGetProperty("mode", out var modeProp)
            ? modeProp.GetString()
            : null;

        var items = data.GetProperty("items");

        // =========================
        // SECTIONS
        // =========================
        if (mode == "sections")
        {
            int depId = data.GetProperty("depId").GetInt32();

            var map = (await db.Sections
                .Where(s => s.DepartmentId == depId)
                .ToListAsync())
                .ToDictionary(x => x.Id);

            foreach (var item in items.EnumerateArray())
            {
                int id = item.GetProperty("id").GetInt32();
                int priority = item.GetProperty("priority").GetInt32();

                if (map.TryGetValue(id, out var section))
                    section.PhonesPagePriority = priority;
            }

            await db.SaveChangesAsync();
            return Results.Ok();
        }

        // =========================
        // DEPARTMENTS
        // =========================
        if (mode == "departments")
        {
            var map = (await db.Departments.ToListAsync())
                .ToDictionary(x => x.Id);

            foreach (var item in items.EnumerateArray())
            {
                int id = item.GetProperty("id").GetInt32();
                int priority = item.GetProperty("priority").GetInt32();

                if (map.TryGetValue(id, out var d))
                    d.PhonesPagePriority = priority;
            }

            await db.SaveChangesAsync();
            return Results.Ok();
        }

        // =========================
        // POSITIONS
        // =========================
        if (mode == "positions")
        {
            var map = (await db.Positions.ToListAsync())
                .ToDictionary(x => x.Id);

            foreach (var item in items.EnumerateArray())
            {
                int id = item.GetProperty("id").GetInt32();
                int priority = item.GetProperty("priority").GetInt32();

                if (map.TryGetValue(id, out var p))
                    p.Priority = priority;
            }

            await db.SaveChangesAsync();
            return Results.Ok();
        }

        // =========================
        // USER TYPES
        // =========================
        if (mode == "userTypes")
        {
            var map = (await db.UserTypes.ToListAsync())
                .ToDictionary(x => x.Id);

            foreach (var item in items.EnumerateArray())
            {
                int id = item.GetProperty("id").GetInt32();
                int priority = item.GetProperty("priority").GetInt32();

                if (map.TryGetValue(id, out var t))
                    t.Priority = priority;
            }

            await db.SaveChangesAsync();
            return Results.Ok();
        }

        return Results.BadRequest("Invalid phones mode");
    }

    /* =========================
       NON-PHONES (GENERIC)
    ========================= */

    var itemsDefault = data.EnumerateArray();

    var mails = await db.Mails.ToListAsync();
    var mailMap = mails.ToDictionary(m => m.Id);

    foreach (var item in itemsDefault)
    {
        int id = item.GetProperty("id").GetInt32();
        int priority = item.GetProperty("priority").GetInt32();

        if (mailMap.TryGetValue(id, out var mail))
        {
            mail.Priority = priority;
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok();
});


app.MapPost("/api/positions", PositionsService.Create);

app.MapPost("/api/positions/delete", PositionsService.Delete);

app.MapPut("/api/positions/{id:int}", PositionsService.Update);

app.MapPost("/api/userTypes", UserTypesService.Create);

app.MapPost("/api/userTypes/delete", UserTypesService.Delete);

app.MapPut("/api/userTypes/{id:int}", UserTypesService.Update);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
