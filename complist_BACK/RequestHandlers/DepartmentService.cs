namespace complist_BACK.RequestHandlers.DepartmentService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    public static class DepartmentsService
    {
        public static async Task<IResult> Create(
     ApplicationContext db,
     HttpRequest request)
        {
            var data =
                await request.ReadFromJsonAsync<
                    Dictionary<string, string>>();

            var name = data?["name"];

            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest("Name is required");

            var department = new Department
            {
                Name = name.Trim()
            };

            db.Departments.Add(department);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict(new
                {
                    message =
                        "Департамент з таким іменем уже існує."
                });
            }

            return Results.Ok(department);
        }

        public static async Task<IResult> Delete(
      ApplicationContext db,
      List<int> ids)
        {
            var departments = await db.Departments
                .Where(d => ids.Contains(d.Id))
                .ToListAsync();

            if (!departments.Any())
                return Results.NotFound();

            var departmentIds = departments
                .Select(d => d.Id)
                .ToList();

            // Користувачі департаментів,
            // включно з користувачами їхніх секцій
            var users = await db.Users
                .Where(u =>
                    u.DepartmentId.HasValue &&
                    departmentIds.Contains(u.DepartmentId.Value))
                .ToListAsync();

            var userIds = users
                .Select(u => u.Id)
                .ToList();

            // Персональні поштові скриньки користувачів
            var personalMails = await db.Mails
                .Where(m =>
                    m.UserId.HasValue &&
                    userIds.Contains(m.UserId.Value))
                .ToListAsync();

            db.Mails.RemoveRange(personalMails);

            // Поштові скриньки самого департаменту
            var departmentMails = await db.Mails
                .Where(m =>
                    m.DepartmentId.HasValue &&
                    departmentIds.Contains(m.DepartmentId.Value))
                .ToListAsync();

            db.Mails.RemoveRange(departmentMails);

            // Sections цього департаменту
            var sectionIds = await db.Sections
                .Where(s => departmentIds.Contains(s.DepartmentId))
                .Select(s => s.Id)
                .ToListAsync();

            // Поштові скриньки секцій
            var sectionMails = await db.Mails
                .Where(m =>
                    m.Sections.Any(s => sectionIds.Contains(s.Id)))
                .ToListAsync();

            db.Mails.RemoveRange(sectionMails);

            // Users
            db.Users.RemoveRange(users);

            // Sections видаляться через Cascade
            db.Departments.RemoveRange(departments);

            await db.SaveChangesAsync();

            return Results.Ok();
        }

        public static async Task<IResult> Update(
    ApplicationContext db,
    int id,
    JsonElement body)
        {
            var department =
                await db.Departments.FindAsync(id);

            if (department == null)
                return Results.NotFound();

            var newName =
                body.TryGetProperty(
                    "name",
                    out var nameProp)
                        ? nameProp.GetString()
                        : null;

            var newPriority =
                body.TryGetProperty(
                    "phonesPagePriority",
                    out var prProp)
                        ? prProp.GetInt32()
                        : (int?)null;

            if (!string.IsNullOrWhiteSpace(newName))
                department.Name = newName.Trim();

            if (newPriority.HasValue)
                department.PhonesPagePriority =
                    newPriority;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict(new
                {
                    message =
                        "Департамент з таким іменем уже існує."
                });
            }

            return Results.Ok(department);
        }
    }
}