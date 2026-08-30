namespace complist_BACK.RequestHandlers.UsersService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    using static System.Net.Mime.MediaTypeNames;

    public static class UsersService
    {
        // CREATE
        public static async Task<IResult> Create(
            ApplicationContext db,
            HttpRequest request)
        {
            var data =
                await request.ReadFromJsonAsync<
                    Dictionary<string, JsonElement>>();

            var newUser = new User
            {
                Name = data["name"].GetString(),
                PositionId = data["positionId"].GetInt32(),
                UserTypeId = data["userTypeId"].GetInt32(),
                DepartmentId = data["departmentId"].GetInt32(),
                SectionId =
                    data["sectionId"].ValueKind ==
                    JsonValueKind.Null
                        ? null
                        : data["sectionId"].GetInt32(),
            };

            db.Users.Add(newUser);

            await db.SaveChangesAsync();

            return Results.Ok();
        }

        // DELETE
        public static async Task<IResult> Delete(
            ApplicationContext db,
            int[] ids)
        {
            var users = await db.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            if (!users.Any())
                return Results.NotFound();

            var userIds = users
                .Select(u => u.Id)
                .ToList();

            var personalMails = await db.Mails
                .Where(m =>
                    m.UserId.HasValue &&
                    userIds.Contains(m.UserId.Value))
                .ToListAsync();

            db.Mails.RemoveRange(personalMails);
            db.Users.RemoveRange(users);

            await db.SaveChangesAsync();

            return Results.Ok();
        }

        // UPDATE
        public static async Task<IResult> Update(
            ApplicationContext db,
            int id,
            HttpRequest request)
        {
            var data =
                await request.ReadFromJsonAsync<
                    Dictionary<string, JsonElement>>();

            var user = await db.Users.FindAsync(id);

            if (user == null)
                return Results.NotFound();

            user.Name =
                data["name"].GetString();

            user.PositionId =
                data["positionId"].GetInt32();

            user.UserTypeId =
                data["userTypeId"].GetInt32();

            user.DepartmentId =
                data["departmentId"].GetInt32();

            user.SectionId =
                data["sectionId"].ValueKind ==
                JsonValueKind.Null
                    ? null
                    : data["sectionId"].GetInt32();

            await db.SaveChangesAsync();

            return Results.Ok(user);
        }



public static async Task<IResult> Transfer(
    ApplicationContext db,
    HttpRequest request)
        {
            var data =
                await request.ReadFromJsonAsync<
                    Dictionary<string, JsonElement>>();

            var userIds = data["userIds"]
                .EnumerateArray()
                .Select(x => x.GetInt32())
                .ToList();

            var departmentId =
                data["departmentId"].GetInt32();

            var transferType =
                data["transferType"].GetString();

            int? sectionId =
                data["sectionId"].ValueKind ==
                JsonValueKind.Null
                    ? null
                    : data["sectionId"].GetInt32();

            var keepResponsibleForMails =
                data["keepResponsibleForMails"]
                    .GetBoolean();

            var keepPhonesByPosition =
                data["keepPhonesByPosition"]
                    .GetBoolean();

            var transferPhones =
                data["transferPhones"]
                    .GetBoolean();

            var users = await db.Users
                .Include(u => u.Phones)
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                // =========================
                // Старі дані
                // =========================

                var oldDepartmentId =
                    user.DepartmentId;

                var oldSectionId =
                    user.SectionId;

                var oldPositionId =
                    user.PositionId;

                var oldUserTypeId =
                    user.UserTypeId;

                var phones =
                    user.Phones?.ToList()
                    ?? new List<Phone>();

                // =========================
                // Зберегти телефони
                // у старому підрозділі
                // =========================

                if (keepPhonesByPosition && phones.Any())
                {
                    var technicalUser = new User
                    {
                        Name = null,

                        PositionId = oldPositionId,
                        UserTypeId = oldUserTypeId,

                        DepartmentId = oldDepartmentId,
                        SectionId = oldSectionId
                    };

                    db.Users.Add(technicalUser);

                    foreach (var phone in phones)
                    {
                        phone.Users ??= new List<User>();

                        phone.Users.Add(technicalUser);
                    }
                }

                // =========================
                // Якщо телефони НЕ переносимо
                // і НЕ залишаємо за посадою
                // =========================

                if (!transferPhones &&
                    !keepPhonesByPosition)
                {
                    foreach (var phone in phones)
                    {
                        phone.Users?.Remove(user);
                    }
                }

                // =========================
                // Перевести користувача
                // =========================

                user.DepartmentId =
                    departmentId;

                user.SectionId =
                    transferType == "section"
                        ? sectionId
                        : null;
            }

            // =========================
            // Responsible users
            // =========================

            if (!keepResponsibleForMails)
            {
                var responsibleUsers =
                    await db.ResponsibleUsers
                        .Where(r =>
                            userIds.Contains(r.UserId))
                        .ToListAsync();

                db.ResponsibleUsers
                    .RemoveRange(responsibleUsers);
            }

            // =========================
            // Save
            // =========================

            await db.SaveChangesAsync();

            return Results.Ok();
        }


        // CHANGE STATUS
        public static async Task<IResult> ChangeStatus(
           ApplicationContext db,
           HttpRequest request,
           int[] ids)
        {
  

            var users = await db.Users
                .Where(user => ids.Contains(user.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsActive = !user.IsActive;
            }

            await db.SaveChangesAsync();

            return Results.Ok();
        }
    }
}




