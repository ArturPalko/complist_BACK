namespace complist_BACK.RequestHandlers.UsersService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    
    public static class UsersService
    {
        // CREATE
        public static async Task<IResult> Create(ApplicationContext db, HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

            var newUser = new User
            {
                Name = data["name"].GetString(),
                PositionId = data["positionId"].GetInt32(),
                UserTypeId = data["userTypeId"].GetInt32(),
                DepartmentId = data["departmentId"].GetInt32(),
                SectionId = data["sectionId"].ValueKind == JsonValueKind.Null
    ? null
    : data["sectionId"].GetInt32(),
            };

            db.Users.Add(newUser);
          
            await db.SaveChangesAsync();

            return Results.Ok();
        }

        public static async Task<IResult> Delete(ApplicationContext db, int[] ids)
        {
            var users = await db.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            if (!users.Any())
                return Results.NotFound();

            db.Users.RemoveRange(users);
            await db.SaveChangesAsync();

            return Results.Ok();
        }

        public static async Task<IResult> Update(
            ApplicationContext db,
            int id,
            HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

            var user = await db.Users.FindAsync(id);

            if (user == null)
                return Results.NotFound();

            user.Name = data["name"].GetString();
            user.PositionId = data["positionId"].GetInt32();
            user.UserTypeId = data["userTypeId"].GetInt32();
            user.DepartmentId = data["departmentId"].GetInt32();
            user.SectionId = data["sectionId"].ValueKind == JsonValueKind.Null
    ? null
    : data["sectionId"].GetInt32();

            await db.SaveChangesAsync();

            return Results.Ok(user);
        }
    }
}