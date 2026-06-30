using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace complist_BACK.RequestHandlers.ChangeOrder
{
    public static class ChangeOrder
    {
        public static async Task<IResult> Handle(
            string pageName,
            JsonElement data,
            ApplicationContext db)
        {
            if (pageName == "phones")
            {
                var mode = data.TryGetProperty("mode", out var m)
                    ? m.GetString()
                    : null;

                var items = data.GetProperty("items");

                if (mode == "section")
                {
                    int depId = data.GetProperty("depId").GetInt32();

                    var map = (await db.Sections
                        .Where(x => x.DepartmentId == depId)
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

                if (mode == "department")
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

                if (mode == "position")
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

                if (mode == "userType")
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

            // =========================
            // DEFAULT (MAILS)
            // =========================

            var mailMap = (await db.Mails.ToListAsync())
                .ToDictionary(x => x.Id);

            foreach (var item in data.EnumerateArray())
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
        }
    }
}
