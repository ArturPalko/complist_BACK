using complist_BACK.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace complist_BACK.RequestHandlers
{
    public static class PhonesService
    {
        public static async Task<IResult> GetPhones(ApplicationContext db)
        {
            var phonesData = await db.Phones
                .Include(p => p.PhoneType)
                .Include(p => p.Users)
                    .ThenInclude(u => u.Department)
                .Include(p => p.Users)
                    .ThenInclude(u => u.Section)
                        .ThenInclude(s => s.Department)
                .Include(p => p.Users)
                    .ThenInclude(u => u.Position)
                .Include(p => p.Users)
                    .ThenInclude(u => u.UserType)
                .ToListAsync();

            var userPhones = phonesData
                .SelectMany(p => p.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    User = new
                    {
                        Id = u.Id,
                        Name = u.Name,

                        UserTypeId = u.UserType?.Id,
                        UserType = u.UserType?.Name,
                        UserTypePriority = u.UserType?.Priority ?? int.MaxValue,

                        UserPositionId = u.PositionId,
                        UserPosition = u.Position?.Name,
                        UserPositionPriority = u.Position?.Priority ?? int.MaxValue,

                        DepartmentId = u.Department?.Id ?? u.Section?.DepartmentId,
                        DepartmentName = u.Department?.Name ?? u.Section?.Department?.Name,

                        DepartmentPriority = u.Department != null
                            ? u.Department.PhonesPagePriority
                            : u.Section!.Department.PhonesPagePriority,

                        SectionId = u.Section?.Id,
                        SectionName = u.Section?.Name,

                        SectionPriority = u.Section?.PhonesPagePriority ?? int.MaxValue
                    },

                    Phone = new
                    {
                        PhoneName = p.Number,
                        PhoneType = p.PhoneType.Name
                    }
                }))
                .GroupBy(x => x.User.Id)
                .Select(g => new
                {
                    UserId = g.Key,

                    UserName = g.Select(u => u.User.Name).FirstOrDefault(),

                    UserTypeId = g.Select(u => u.User.UserTypeId).FirstOrDefault(),
                    UserType = g.Select(u => u.User.UserType).FirstOrDefault(),

                    UserTypePriority = g.Select(u => u.User.UserTypePriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    UserPositionId = g.Select(u => u.User.UserPositionId).FirstOrDefault(),
                    UserPosition = g.Select(u => u.User.UserPosition).FirstOrDefault(),

                    UserPositionPriority = g.Select(u => u.User.UserPositionPriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    DepartmentId = g.Select(u => u.User.DepartmentId).FirstOrDefault(),
                    DepartmentName = g.Select(u => u.User.DepartmentName).FirstOrDefault(),

                    DepartmentPriority = g.Select(u => u.User.DepartmentPriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    SectionId = g.Select(u => u.User.SectionId).FirstOrDefault(),
                    SectionName = g.Select(u => u.User.SectionName).FirstOrDefault(),

                    SectionPriority = g.Select(u => u.User.SectionPriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    Phones = g.Select(x => x.Phone)
                        .Distinct()
                        .ToList()
                })
                .ToList();

            // =========================
            // GROUP BY DEPARTMENT
            // =========================

            var groupedByDepartment = userPhones
                .GroupBy(u => u.DepartmentId)
                .Select(deptGroup => new
                {
                    DepartmentId = deptGroup.Key,

                    DepartmentName = deptGroup
                        .Select(u => u.DepartmentName)
                        .FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Unknown",

                    DepartmentPriority = deptGroup
                        .Select(u => u.DepartmentPriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    Users = deptGroup
                        .Where(u => u.SectionId == null)
                        .OrderBy(u => u.UserTypePriority)
                        .ThenBy(u => u.UserPositionPriority)
                        .ToList(),

                    Sections = deptGroup
                        .Where(u => u.SectionId != null)
                        .GroupBy(u => u.SectionId)
                        .Select(sectionGroup => new
                        {
                            SectionId = sectionGroup.Key,

                            DepartmentId = deptGroup.Key,

                      DepartmentName = deptGroup
                        .Select(u => u.DepartmentName)
                        .FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Unknown",

                            SectionName = sectionGroup
                                .Select(x => x.SectionName)
                                .FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Unknown",

                            SectionPriority = sectionGroup
                                .Select(x => x.SectionPriority)
                                .DefaultIfEmpty(int.MaxValue)
                                .Min(),

                            Users = sectionGroup
                                .OrderBy(u => u.UserTypePriority)
                                .ThenBy(u => u.UserPositionPriority)
                                .ToList()
                        })
                        .OrderBy(s => s.SectionPriority)
                        .ToList()
                })
                .OrderBy(d => d.DepartmentPriority)
                .ToList();

            return Results.Json(groupedByDepartment);
        }


        public static async Task<IResult> Create(
    ApplicationContext db,
    HttpRequest request)
{
    var data =
        await request.ReadFromJsonAsync<
            Dictionary<string, JsonElement>>();

    var name = data?["name"].GetString();
    var type = data["type"].GetInt32();

    var assignedUserIds = data?["assignedUsers"]
        .EnumerateArray()
        .Select(x => x.GetInt32())
        .ToList();

    var users = await db.Users
        .Where(u => assignedUserIds.Contains(u.Id))
        .ToListAsync();

    foreach (var user in users)
    {
        var oldPhones = await db.Phones
            .Include(p => p.Users)
            .Where(p =>
                p.PhoneTypeId == type &&
                p.Users.Any(u => u.Id == user.Id))
            .ToListAsync();

        foreach (var oldPhone in oldPhones)
        {
            oldPhone.Users.Remove(user);
        }
    }

    var phone = new Phone
    {
        Number = name,
        PhoneTypeId = type,
        Users = users
    };

    db.Phones.Add(phone);

    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new
        {
            message = "Телефон з таким номером уже існує."
        });
    }

    return Results.Ok();
}


        public static async Task<IResult> Edit(
    ApplicationContext db,
    int id,
    HttpRequest request)
        {
            var data =
                await request.ReadFromJsonAsync<
                    Dictionary<string, JsonElement>>();

            var number = data["name"].GetString();
            var phoneTypeId = data["type"].GetInt32();

            var assignedUserIds = data["assignedUsers"]
                .EnumerateArray()
                .Select(x => x.GetInt32())
                .ToList();

            var phone = await db.Phones
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phone == null)
                return Results.NotFound();

            var users = await db.Users
                .Where(u => assignedUserIds.Contains(u.Id))
                .ToListAsync();

            // Якщо користувач уже має інший телефон цього типу —
            // прибираємо старий зв'язок
            foreach (var user in users)
            {
                var oldPhones = await db.Phones
                    .Include(p => p.Users)
                    .Where(p =>
                        p.Id != phone.Id &&
                        p.PhoneTypeId == phoneTypeId &&
                        p.Users.Any(u => u.Id == user.Id))
                    .ToListAsync();

                foreach (var oldPhone in oldPhones)
                {
                    oldPhone.Users.Remove(user);
                }
            }

            // Оновлюємо телефон
            phone.Number = number;
            phone.PhoneTypeId = phoneTypeId;

            // Повністю оновлюємо користувачів
            phone.Users.Clear();

            foreach (var user in users)
            {
                phone.Users.Add(user);
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict(new
                {
                    message = "Телефон з таким номером уже існує."
                });
            }

            return Results.Ok(phone);
        }

        public static async Task<IResult> Delete(ApplicationContext db, List<int> ids)
        {
            var items = await db.Phones
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            db.Phones.RemoveRange(items);

            await db.SaveChangesAsync();

            return Results.Ok();
        }
        public static async Task<IResult> Assign(ApplicationContext db, HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

            var userId = data["userId"].GetInt32();

            int? transferId = null;

            if (data.TryGetValue("transferId", out var transferElement) &&
                transferElement.ValueKind != JsonValueKind.Null)
            {
                transferId = transferElement.GetInt32();
            }

            var user = await db.Users
                .Include(u => u.Phones)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Results.NotFound();

            if (transferId == null)
            {
                var phones = data["phones"];

                int? GetPhoneId(string type)
                {
                    var phone = phones.GetProperty(type);

                    if (phone.ValueKind == JsonValueKind.String &&
                        string.IsNullOrEmpty(phone.GetString()))
                    {
                        return null;
                    }

                    return phone.GetInt32();
                }

                var phoneIds = new[]
                {
            GetPhoneId("cisco"),
            GetPhoneId("internal"),
            GetPhoneId("landline")
        }
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToArray();

                var phonesToAssign = await db.Phones
                    .Where(p => phoneIds.Contains(p.Id))
                    .ToListAsync();

                user.Phones.Clear();

                foreach (var phone in phonesToAssign)
                {
                    user.Phones.Add(phone);
                }
            }
            else
            {
                var transferUser = await db.Users
                    .Include(u => u.Phones)
                    .FirstOrDefaultAsync(u => u.Id == transferId);

                if (transferUser == null)
                    return Results.NotFound();

                transferUser.Phones.Clear();

                foreach (var phone in user.Phones)
                {
                    transferUser.Phones.Add(phone);
                }

                user.Phones.Clear();
            }

            await db.SaveChangesAsync();

            return Results.Ok();
        }


    }
}

