
using complist_BACK;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowFrontend", policy =>
{
    policy.WithOrigins("http://localhost:3000") 
          .AllowAnyHeader()
          .AllowAnyMethod();
});
});

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
var app = builder.Build();
app.UseCors("AllowFrontend");



app.Map("/dovidniki", UserService.GetUsers);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
