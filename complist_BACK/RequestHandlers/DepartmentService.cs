namespace complist_BACK.RequestHandlers.DepartmentService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    public static class DepartmentsService
    {
        public static async Task<IResult> Create(ApplicationContext db, HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, string>>();
            var name = data?["name"];

            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest("Name is required");

            var department = new Department
            {
                Name = name
            };

            db.Departments.Add(department);
            await db.SaveChangesAsync();

            return Results.Ok(department);
        }

        public static async Task<IResult> Delete(ApplicationContext db, List<int> ids)
        {
            var items = await db.Departments
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (!items.Any())
                return Results.NotFound();

            db.Departments.RemoveRange(items);
            await db.SaveChangesAsync();

            return Results.Ok();
        }

        public static async Task<IResult> Update(ApplicationContext db, int id, JsonElement body)
        {
            var department = await db.Departments.FindAsync(id);

            if (department == null)
                return Results.NotFound();

            var newName = body.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()
                : null;

            var newPriority = body.TryGetProperty("phonesPagePriority", out var prProp)
                ? prProp.GetInt32()
                : (int?)null;

            if (!string.IsNullOrWhiteSpace(newName))
                department.Name = newName;

            if (newPriority.HasValue)
                department.PhonesPagePriority = newPriority;

            await db.SaveChangesAsync();

            return Results.Ok(department);
        }
    }
}