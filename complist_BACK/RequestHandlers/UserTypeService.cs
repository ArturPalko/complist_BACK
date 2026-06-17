namespace complist_BACK.RequestHandlers.UserTypeService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    public static class UserTypesService
    {
        // CREATE
        public static async Task<IResult> Create(ApplicationContext db, HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, string>>();
            var name = data?["name"];

            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest("Name is required");

            await db.UserTypes.ExecuteUpdateAsync(s =>
                s.SetProperty(p => p.Priority, p => p.Priority + 1));

            var entity = new UserType
            {
                Name = name,
                Priority = 1
            };

            db.UserTypes.Add(entity);
            await db.SaveChangesAsync();

            return Results.Ok(entity);
        }

        // DELETE
        public static async Task<IResult> Delete(ApplicationContext db, List<int> ids)
        {
            var items = await db.UserTypes
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            db.UserTypes.RemoveRange(items);

            await db.SaveChangesAsync();

            return Results.Ok();
        }

        // UPDATE
        public static async Task<IResult> Update(ApplicationContext db, int id, JsonElement body)
        {
            var entity = await db.UserTypes.FindAsync(id);

            if (entity == null)
                return Results.NotFound();

            var newName = body.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()
                : null;

            var newPriority = body.TryGetProperty("priority", out var priorityProp)
                ? priorityProp.GetInt32()
                : (int?)null;

            if (!string.IsNullOrWhiteSpace(newName))
                entity.Name = newName;

            if (newPriority.HasValue && newPriority != entity.Priority)
            {
                int old = entity.Priority ?? 0;
                int target = newPriority.Value;

                if (target < old)
                {
                    var affected = await db.UserTypes
                        .Where(x => x.Priority >= target && x.Priority < old)
                        .ToListAsync();

                    foreach (var item in affected)
                        item.Priority++;
                }
                else
                {
                    var affected = await db.UserTypes
                        .Where(x => x.Priority <= target && x.Priority > old)
                        .ToListAsync();

                    foreach (var item in affected)
                        item.Priority--;
                }

                entity.Priority = target;
            }

            await db.SaveChangesAsync();

            return Results.Ok(entity);
        }
    }
}