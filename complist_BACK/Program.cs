using complist_BACK;
using complist_BACK.Entities;
using complist_BACK.RequestHandlers;
using complist_BACK.RequestHandlers.MailService;
using complist_BACK.RequestHandlers.PositionService;
using complist_BACK.RequestHandlers.UserTypeService;
using complist_BACK.RequestHandlers.DepartmentService;
using complist_BACK.RequestHandlers.UsersService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.VisualBasic;
using System;
using System.Text.Json;
using complist_BACK.RequestHandlers.SectionService;



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
    var phonesResult = await db.PhoneTypes
        .Select(pt => new
        {
            id = pt.Id,
            name = pt.Name,
            phones = pt.Phones.Select(p => new
            {
                id = p.Id,
                number = p.Number,
                users = p.Users.Select(u => new
                {
                    id = u.Id,
                    name = !string.IsNullOrWhiteSpace(u.Name)
            ? u.Name
            : $"{u.Position.Name} {u.Department.Name}" +
              (u.Section != null ? $" / {u.Section.Name}" : "")
                }).ToList()
            }).ToList()
        })
        .ToListAsync();

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
        .OrderBy(d => d.PhonesPagePriority)
        .Select(d => new
        {
            departmentId = d.Id,
            departmentName = d.Name,
            priority = d.PhonesPagePriority,

            // Користувачі департаменту (без секції)
            users = d.Users
                .Where(u => u.SectionId == null)
                .Select(u => new
                {
                    id = u.Id,
                    name = u.Name,
                    positionId = u.PositionId,
                    userTypeId = u.UserTypeId,
                    positionName = u.Position != null ? u.Position.Name : null,
                    userType = u.UserType.Name
                })
                .ToList(),

            sections = d.Sections
                .OrderBy(s => s.PhonesPagePriority)
                .Select(s => new
                {
                    sectionId = s.Id,
                    sectionName = s.Name,
                    sectionPriority = s.PhonesPagePriority,
                    departmentId = s.DepartmentId,

                    users = s.Users
                        .Select(u => new
                        {
                            id = u.Id,
                            name = u.Name,
                            positionId = u.PositionId,
                            positionName = u.Position != null ? u.Position.Name : null,
                            userTypeId = u.UserTypeId,
                            userType = u.UserType.Name
                        })
                        .ToList()
                })
                .ToList()
        })
        .ToListAsync();
    //var users = await db.Users.Where(u=> u.UserTypeId ==1 && !string.IsNullOrEmpty(u.Name))
    /*  var users = await db.Users
      .Select(u => new
      {
          id = u.Id,
          name = u.Name,
      })
      .ToListAsync();*/

    var users = await db.Users
    .Select(u => new
    {
        id = u.Id,
        name = !string.IsNullOrWhiteSpace(u.Name)
            ? u.Name
            : $"{u.Position.Name} {u.Department.Name}" +
              (u.Section != null ? $" / {u.Section.Name}" : "")
    })
    .ToListAsync();
    var sections = await db.Sections
     .OrderBy(s => s.PhonesPagePriority)
     .Select(s => new
     {
         id = s.Id,
         name = s.Name,
     })
     .ToListAsync();

    var deps = await db.Departments
 .OrderBy(s => s.PhonesPagePriority)
 .Select(s => new
 {
     id = s.Id,
     name = s.Name,
 })
 .ToListAsync();

    return Results.Ok(new
    {
         phonesResult,
         positions,
         userTypes,
         departments,
         users,
        sections,
        deps
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

app.MapGet(
    "/mails/{mailType}/passwords/{id?}",
    MailsService.GetMailsPasswords
)
.RequireAuthorization("AdminOnly");


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

app.MapPost("/logout", (HttpContext context) =>
{
    return LoginService.LogOut(context);
});


app.MapPost("/changeOrder/{pageName}", async (
    ApplicationContext db,
    string pageName,
    JsonElement data) =>
{
    var items = data.EnumerateArray();

    if (pageName == "positions")
    {
        var map = (await db.Positions.ToListAsync())
            .ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            var id = item.GetProperty("id").GetInt32();
            var priority = item.GetProperty("priority").GetInt32();

            if (map.TryGetValue(id, out var position))
                position.Priority = priority;
        }
    }
    else if (pageName == "userTypes")
    {
        var map = (await db.UserTypes.ToListAsync())
            .ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            var id = item.GetProperty("id").GetInt32();
            var priority = item.GetProperty("priority").GetInt32();

            if (map.TryGetValue(id, out var userType))
                userType.Priority = priority;
        }
    }
    else if (pageName == "departments")
    {
        var map = (await db.Departments.ToListAsync())
            .ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            var id = item.GetProperty("id").GetInt32();
            var priority = item.GetProperty("priority").GetInt32();

            if (map.TryGetValue(id, out var department))
                department.PhonesPagePriority = priority;
        }
    }
    else if (pageName == "sections")
    {
        var map = (await db.Sections.ToListAsync())
            .ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            var id = item.GetProperty("id").GetInt32();
            var priority = item.GetProperty("priority").GetInt32();

            if (map.TryGetValue(id, out var section))
                section.PhonesPagePriority = priority;
        }
    }
    else
    {
        return Results.BadRequest($"Invalid page: {pageName}");
    }

    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapPost("/api/sections", SectionsService.Create);
app.MapPost("/api/sections/delete", SectionsService.Delete);
app.MapPut("/api/sections/{id:int}", SectionsService.Update);


app.MapPost("/api/departments", DepartmentsService.Create);

app.MapPost("/api/departments/delete", DepartmentsService.Delete);

app.MapPut("/api/departments/{id:int}", DepartmentsService.Update);
app.MapPost("/api/positions", PositionsService.Create);

app.MapPost("/api/positions/delete", PositionsService.Delete);

app.MapPut("/api/positions/{id:int}", PositionsService.Update);

app.MapPost("/api/userTypes", UserTypesService.Create);

app.MapPost("/api/userTypes/delete", UserTypesService.Delete);

app.MapPut("/api/userTypes/{id:int}", UserTypesService.Update);





app.MapPost("/api/users", UsersService.Create);
app.MapPut("/api/users/{id:int}", UsersService.Update);
app.MapPost("/api/users/delete", UsersService.Delete);

app.MapPost("/api/phones", PhonesService.Create);
app.MapPut("/api/phones/{id:int}", PhonesService.Edit);
app.MapPost("/api/phones/delete", PhonesService.Delete);

app.MapPost("/api/mails/{mailType}", MailsService.AddMail);
app.MapPut("/api/mails/{mailType}/{id:int}", MailsService.EditMail);
app.MapPost("/api/mails/delete", MailsService.DeleteMail);

app.UseAuthorization();

app.MapControllers();

app.Run();
