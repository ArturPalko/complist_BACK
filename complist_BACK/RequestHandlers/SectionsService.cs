namespace complist_BACK.RequestHandlers.SectionService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    public static class SectionsService
    {
        public static async Task<IResult> Create(ApplicationContext db, HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, string>>();

            var name = data?["name"];
            var departmentIdStr = data?["departmentId"];

            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest("Name is required");

            if (!int.TryParse(departmentIdStr, out int departmentId))
                return Results.BadRequest("DepartmentId is required");

            var departmentExists = await db.Departments.AnyAsync(d => d.Id == departmentId);
            if (!departmentExists)
                return Results.NotFound("Department not found");

            await db.Sections.ExecuteUpdateAsync(s =>
                s.SetProperty(x => x.PhonesPagePriority, x => x.PhonesPagePriority + 1));

            var section = new Section
            {
                Name = name,
                DepartmentId = departmentId,
                PhonesPagePriority = 1
            };

            db.Sections.Add(section);
            await db.SaveChangesAsync();

            return Results.Ok(section);
        }

        public static async Task<IResult> Delete(ApplicationContext db, List<int> ids)
        {
            var items = await db.Sections
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (!items.Any())
                return Results.NotFound();

            db.Sections.RemoveRange(items);
            await db.SaveChangesAsync();

            return Results.Ok();
        }

        public static async Task<IResult> Update(ApplicationContext db, int id, JsonElement body)
        {
            var section = await db.Sections.FindAsync(id);

            if (section == null)
                return Results.NotFound();

            var newName = body.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()
                : null;

            var newAddress = body.TryGetProperty("address", out var addrProp)
                ? addrProp.GetString()
                : null;

            var newDepartmentId = body.TryGetProperty("departmentId", out var depProp)
                ? depProp.GetInt32()
                : (int?)null;

            var newPriority = body.TryGetProperty("phonesPagePriority", out var prProp)
                ? prProp.GetInt32()
                : (int?)null;

            if (!string.IsNullOrWhiteSpace(newName))
                section.Name = newName;

            if (!string.IsNullOrWhiteSpace(newAddress))
                section.Address = newAddress;

            if (newDepartmentId.HasValue)
            {
                var exists = await db.Departments.AnyAsync(d => d.Id == newDepartmentId);
                if (!exists)
                    return Results.NotFound("Department not found");

                section.DepartmentId = newDepartmentId.Value;
            }

            if (newPriority.HasValue)
                section.PhonesPagePriority = newPriority;

            await db.SaveChangesAsync();

            return Results.Ok(section);
        }
    }
}