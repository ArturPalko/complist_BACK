using complist_BACK.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace complist_BACK.RequestHandlers.PhonesCrudService
{
    public static class PhonesCrudService
    {
        // =========================
        // CREATE
        // =========================
        public static async Task<IResult> Create(
            ApplicationContext db,
            JsonElement data)
        {
            var phone = new Phone
            {
                Number = data.GetProperty("number").GetString(),
                PhoneTypeId = data.GetProperty("phoneTypeId").GetInt32()
            };

            db.Phones.Add(phone);
            await db.SaveChangesAsync();

            return Results.Ok(phone);
        }

        // =========================
        // UPDATE
        // =========================
        public static async Task<IResult> Update(
     ApplicationContext db,
     int id,
     JsonElement data)
        {
            var entity = await db.Phones.FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return Results.Ok();

            db.Phones.Update(entity); 

            if (data.TryGetProperty("name", out var numberProp))
                entity.Number = numberProp.GetString();

            if (data.TryGetProperty("phoneTypeId", out var typeProp))
                entity.PhoneTypeId = typeProp.GetInt32();

            var result = await db.SaveChangesAsync();

            Console.WriteLine($"Saved rows: {result}");

            return Results.Ok(entity);
        }
            // =========================
            // DELETE (bulk)
            // =========================
            public static async Task<IResult> Delete(
     ApplicationContext db,
     JsonElement data)
        {
            List<int> ids;

            if (data.ValueKind == JsonValueKind.Array)
            {
                ids = data.EnumerateArray()
                          .Select(x => x.GetInt32())
                          .ToList();
            }
            else if (data.ValueKind == JsonValueKind.Object &&
                     data.TryGetProperty("ids", out var idsProp))
            {
                ids = idsProp.EnumerateArray()
                             .Select(x => x.GetInt32())
                             .ToList();
            }
            else
            {
                return Results.Ok();
            }

            if (ids.Count == 0)
                return Results.Ok();

            var entities = await db.Phones
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            db.Phones.RemoveRange(entities);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                deleted = entities.Count,
                ids
            });
        }
    }
}