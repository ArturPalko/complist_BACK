using complist_BACK;
using complist_BACK.Entities;
using complist_BACK.RequestHandlers;
using complist_BACK.RequestHandlers.ChangeOrderService;
using complist_BACK.RequestHandlers.DepartmentService;
using complist_BACK.RequestHandlers.DictionariesService;
using complist_BACK.RequestHandlers.MailService;
using complist_BACK.RequestHandlers.PositionService;
using complist_BACK.RequestHandlers.SectionService;
using complist_BACK.RequestHandlers.UsersService;
using complist_BACK.RequestHandlers.UserTypeService;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

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
              .AllowCredentials();
    });
});

string? connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(connection));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();



app.Map("/mails/{mailType}", async (string mailType, ApplicationContext db) =>
    await MailsService.GetMails(mailType, db));

app.Map("/phones/", async (ApplicationContext db) =>
    await PhonesService.GetPhones(db));



app.MapGet("/checkAuth", (HttpContext context) =>
    LoginService.ChechAuthorization(context))
    .RequireAuthorization();

app.MapPost("/login", (
    Login request,
    ApplicationContext db,
    HttpContext httpContext) =>
    LoginService.Login(request, db, httpContext));

app.MapPost("/logout", (HttpContext context) =>
    LoginService.LogOut(context));



var privateApi = app.MapGroup("/api")
    .RequireAuthorization("AdminOnly");

privateApi.MapGet("/dictionaries", DictionariesService.Get);
privateApi.MapGet("/mails/{mailType}/passwords/{id?}", MailsService.GetMailsPasswords);

privateApi.MapPost("/changeOrder/{pageName}", ChangeOrderService.ChangeOrder);

privateApi.MapPost("/sections", SectionsService.Create);
privateApi.MapPost("/sections/delete", SectionsService.Delete);
privateApi.MapPut("/sections/{id:int}", SectionsService.Update);

privateApi.MapPost("/departments", DepartmentsService.Create);
privateApi.MapPost("/departments/delete", DepartmentsService.Delete);
privateApi.MapPut("/departments/{id:int}", DepartmentsService.Update);

privateApi.MapPost("/positions", PositionsService.Create);
privateApi.MapPost("/positions/delete", PositionsService.Delete);
privateApi.MapPut("/positions/{id:int}", PositionsService.Update);

privateApi.MapPost("/userTypes", UserTypesService.Create);
privateApi.MapPost("/userTypes/delete", UserTypesService.Delete);
privateApi.MapPut("/userTypes/{id:int}", UserTypesService.Update);

privateApi.MapPost("/users", UsersService.Create);
privateApi.MapPut("/users/{id:int}", UsersService.Update);
privateApi.MapPost("/users/delete", UsersService.Delete);


privateApi.MapPost("/phones", PhonesService.Create);
privateApi.MapPut("/phones/{id:int}", PhonesService.Edit);
privateApi.MapPost("/phones/delete", PhonesService.Delete);
privateApi.MapPut("/assignPhonesToUsers", PhonesService.Assign);

privateApi.MapPost("/mails/{mailType}", MailsService.AddMail);
privateApi.MapPut("/mails/{mailType}/{id:int}", MailsService.EditMail);
privateApi.MapPost("/mails/delete", MailsService.DeleteMail);

privateApi.MapPut("users/transfer", UsersService.Transfer);

app.MapControllers();

app.Run();