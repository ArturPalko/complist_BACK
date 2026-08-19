namespace complist_BACK.RequestHandlers.ChangeOrderService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    public static class ChangeOrderService
    {
        public static async Task<IResult> ChangeOrder(
            ApplicationContext db,
            string pageName,
            JsonElement data)
        {
            if (pageName == "sections")
            {
                if (!data.TryGetProperty("depId", out var depIdProperty))
                    return Results.BadRequest("depId is required for sections");

                if (!data.TryGetProperty("items", out var items))
                    return Results.BadRequest("items are required");

                var depId = depIdProperty.GetInt32();

                var map = (await db.Sections
                    .Where(s => s.DepartmentId == depId)
                    .ToListAsync())
                    .ToDictionary(x => x.Id);

                foreach (var item in items.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var priority = item.GetProperty("priority").GetInt32();

                    if (map.TryGetValue(id, out var section))
                        section.PhonesPagePriority = priority;
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            }

            if (pageName == "departments")
            {
                var items = data.ValueKind == JsonValueKind.Array
                    ? data
                    : data.GetProperty("items");

                var map = (await db.Departments
                    .ToListAsync())
                    .ToDictionary(x => x.Id);

                foreach (var item in items.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var priority = item.GetProperty("priority").GetInt32();

                    if (map.TryGetValue(id, out var department))
                        department.PhonesPagePriority = priority;
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            }

            if (pageName == "positions")
            {
                var items = data.ValueKind == JsonValueKind.Array
                    ? data
                    : data.GetProperty("items");

                var map = (await db.Positions
                    .ToListAsync())
                    .ToDictionary(x => x.Id);

                foreach (var item in items.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var priority = item.GetProperty("priority").GetInt32();

                    if (map.TryGetValue(id, out var position))
                        position.Priority = priority;
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            }

            if (pageName == "userType")
            {
                var items = data.ValueKind == JsonValueKind.Array
                    ? data
                    : data.GetProperty("items");

                var map = (await db.UserTypes
                    .ToListAsync())
                    .ToDictionary(x => x.Id);

                foreach (var item in items.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var priority = item.GetProperty("priority").GetInt32();

                    if (map.TryGetValue(id, out var userType))
                        userType.Priority = priority;
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            }

            if (pageName == "mails")
            {
                var items = data.ValueKind == JsonValueKind.Array
                    ? data
                    : data.GetProperty("items");

                var map = (await db.Mails
                    .ToListAsync())
                    .ToDictionary(x => x.Id);

                foreach (var item in items.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var priority = item.GetProperty("priority").GetInt32();

                    if (map.TryGetValue(id, out var mail))
                        mail.Priority = priority;
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            }

            return Results.BadRequest(
                $"Invalid page name: {pageName}"
            );
        }
    }
}