namespace complist_BACK.RequestHandlers.PositionService
{
    using complist_BACK.Entities;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Text.Json;

    public static class PositionsService
    {
        public static async Task<IResult> Create(ApplicationContext db, HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, string>>();
            var name = data?["name"];

            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest("Name is required");

            await db.Positions.ExecuteUpdateAsync(s =>
                s.SetProperty(p => p.Priority, p => p.Priority + 1));

            var position = new Position
            {
                Name = name,
                Priority = 1
            };

            db.Positions.Add(position);
            await db.SaveChangesAsync();
            return Results.Ok(position);
        }

        public static async Task<IResult> Delete(ApplicationContext db, List<int> ids)
        {
            var items = await db.Positions
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            db.Positions.RemoveRange(items);

            await db.SaveChangesAsync();

            return Results.Ok();
        }

        public static async Task<IResult> Update(ApplicationContext db, int id, JsonElement body)
        {
            var position = await db.Positions.FindAsync(id);

            if (position == null)
                return Results.NotFound();

            var newName = body.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()
                : null;

            var newPriority = body.TryGetProperty("priority", out var priorityProp)
                ? priorityProp.GetInt32()
                : (int?)null;

            if (!string.IsNullOrWhiteSpace(newName))
                position.Name = newName;

            if (newPriority.HasValue && newPriority != position.Priority)
            {
                int old = position.Priority ?? 0;
                int target = newPriority.Value;

                if (target < old)
                {
                    var affected = await db.Positions
                        .Where(x => x.Priority >= target && x.Priority < old)
                        .ToListAsync();

                    foreach (var item in affected)
                        item.Priority++;
                }
                else
                {
                    var affected = await db.Positions
                        .Where(x => x.Priority <= target && x.Priority > old)
                        .ToListAsync();

                    foreach (var item in affected)
                        item.Priority--;
                }

                position.Priority = target;
            }

            await db.SaveChangesAsync();

            return Results.Ok(position);
        }
    }
}