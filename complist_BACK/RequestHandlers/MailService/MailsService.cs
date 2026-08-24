namespace complist_BACK.RequestHandlers.MailService
{
    using complist_BACK.Entities;
    using complist_BACK.RequestHandlers.MailService.helpers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    public static class MailsService
    {
        public static async Task<IResult> GetMails(
            string mailType,
            ApplicationContext db)
        {
            var mailsData =
                await GetMails_endpoint.GetData(mailType, db);

            object mails = mailType switch
            {
                "Gov-ua" => GetMails_endpoint.MapGovUa(mailsData),
                "Lotus" => GetMails_endpoint.MapLotus(mailsData),
                _ => Results.Json(new { Message = "Unknown mail type" })
            };

            return Results.Json(mails);
        }

        public static async Task<IResult> GetMailsPasswords(
            string mailType,
            ApplicationContext db,
            int? id)
        {
            var query = db.Mails
                .Include(m => m.MailType)
                .Where(m => m.MailType.Name == mailType);

            if (id.HasValue)
            {
                var password = await query
                    .Where(m => m.Id == id.Value)
                    .Select(m => new
                    {
                        id = m.Id,
                        password = m.Password
                    })
                    .FirstOrDefaultAsync();

                return Results.Json(password);
            }

            var passwords = await query
                .Select(m => new
                {
                    id = m.Id,
                    password = m.Password
                })
                .ToListAsync();

            return Results.Json(passwords);
        }

        public static async Task<IResult> AddMail(
            string mailType,
            ApplicationContext db,
            HttpRequest request)
        {
            var data =
                await request.ReadFromJsonAsync<
                    Dictionary<string, JsonElement>>();

            if (data == null)
                return Results.BadRequest("No data");

            int typeId = mailType switch
            {
                "Gov-ua" => 1,
                "Lotus" => 2,
                _ => 0
            };

            if (typeId == 0)
                return Results.BadRequest("Unknown mail type");

            if (!data.TryGetValue(
                "ownerType",
                out var ownerTypeElement))
            {
                return Results.BadRequest("ownerType is required");
            }

            var ownerType = ownerTypeElement.GetString();

            var mails = await db.Mails.ToListAsync();

            foreach (var m in mails)
            {
                m.Priority++;
            }

            string? ownerDisplayName = null;

            if (data.TryGetValue(
                "ownerDisplayName",
                out var ownerDisplayNameElement))
            {
                ownerDisplayName =
                    ownerDisplayNameElement.GetString();

                if (string.IsNullOrWhiteSpace(ownerDisplayName))
                {
                    ownerDisplayName = null;
                }
            }

            var newMail = new Mail
            {
                Name = data["mail"].GetString(),

                PreviousName =
                    mailType == "Lotus"
                    && data.TryGetValue(
                        "previousName",
                        out var previousName)
                        ? previousName.GetString()
                        : null,

                MailTypeId = typeId,
                Priority = 1,

                Password =
                    data.TryGetValue(
                        "password",
                        out var password)
                        ? password.GetString()
                        : null,

                OwnerDisplayName = ownerDisplayName
            };

            switch (ownerType)
            {
                case "department":
                    {
                        if (!data.TryGetValue(
                            "ownerId",
                            out var ownerIdElement))
                        {
                            return Results.BadRequest(
                                "ownerId is required");
                        }

                        var ownerId =
                            ownerIdElement.GetInt32();

                        var department =
                            await db.Departments.FindAsync(ownerId);

                        if (department == null)
                        {
                            return Results.BadRequest(
                                "Department not found");
                        }

                        newMail.DepartmentId = ownerId;

                        break;
                    }

                case "section":
                    {
                        if (!data.TryGetValue(
                            "ownerIds",
                            out var ownerIdsElement))
                        {
                            return Results.BadRequest(
                                "ownerIds is required");
                        }

                        if (ownerIdsElement.ValueKind !=
                            JsonValueKind.Array)
                        {
                            return Results.BadRequest(
                                "ownerIds must be an array");
                        }

                        var sectionIds =
                            ownerIdsElement
                                .EnumerateArray()
                                .Select(x => x.GetInt32())
                                .Distinct()
                                .ToList();

                        if (!sectionIds.Any())
                        {
                            return Results.BadRequest(
                                "At least one section is required");
                        }

                        var sections =
                            await db.Sections
                                .Where(s =>
                                    sectionIds.Contains(s.Id))
                                .ToListAsync();

                        if (sections.Count != sectionIds.Count)
                        {
                            return Results.BadRequest(
                                "One or more sections not found");
                        }

                        foreach (var section in sections)
                        {
                            newMail.Sections.Add(section);
                        }

                        if (sectionIds.Count <= 1)
                        {
                            newMail.OwnerDisplayName = null;
                        }

                        break;
                    }

                case "user":
                    {
                        if (!data.TryGetValue(
                            "ownerId",
                            out var ownerIdElement))
                        {
                            return Results.BadRequest(
                                "ownerId is required");
                        }

                        var ownerId =
                            ownerIdElement.GetInt32();

                        var user =
                            await db.Users.FindAsync(ownerId);

                        if (user == null)
                        {
                            return Results.BadRequest(
                                "User not found");
                        }

                        newMail.UserId = ownerId;
                        newMail.OwnerDisplayName = null;

                        break;
                    }

                case "none":
                    {
                        newMail.DepartmentId = null;
                        newMail.UserId = null;
                        newMail.Sections.Clear();

                        break;
                    }

                default:
                    return Results.BadRequest(
                        "Unknown owner type");
            }

            db.Mails.Add(newMail);

            await db.SaveChangesAsync();

            if (
                mailType == "Gov-ua"
                &&
                data.TryGetValue(
                    "responsibleUserIds",
                    out var responsibleUsers))
            {
                var ids =
                    responsibleUsers
                        .EnumerateArray()
                        .Select(x => x.GetInt32())
                        .Distinct();

                foreach (var userId in ids)
                {
                    db.ResponsibleUsers.Add(
                        new ResponsibleUser
                        {
                            MailId = newMail.Id,
                            UserId = userId
                        });
                }

                await db.SaveChangesAsync();
            }

            return Results.Ok();
        }

        public static async Task<IResult> DeleteMail(
            ApplicationContext db,
            HttpRequest request)
        {
            var ids =
                await request.ReadFromJsonAsync<List<int>>();

            if (ids == null || !ids.Any())
                return Results.BadRequest("No ids provided");

            var mailsToDelete =
                await db.Mails
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync();

            if (!mailsToDelete.Any())
                return Results.NotFound();

            db.Mails.RemoveRange(mailsToDelete);

            await db.SaveChangesAsync();

            var remainingMails =
                await db.Mails
                    .OrderBy(x => x.Priority)
                    .ToListAsync();

            for (int i = 0; i < remainingMails.Count; i++)
            {
                remainingMails[i].Priority = i + 1;
            }

            await db.SaveChangesAsync();

            return Results.Ok();
        }

        public static async Task<IResult> EditMail(
            string mailType,
            ApplicationContext db,
            int id,
            HttpRequest request)
        {
            var data =
                await request.ReadFromJsonAsync<
                    Dictionary<string, JsonElement>>();

            if (data == null)
                return Results.BadRequest("No data");

            var mail =
                await db.Mails
                    .Include(m => m.Sections)
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (mail == null)
                return Results.NotFound();

            var newName =
                data["mail"].GetString();

            var newPreviousName =
                data.TryGetValue(
                    "previousName",
                    out var previousName)
                    ? previousName.GetString()
                    : null;

            var autoUpdatePreviousName =
                data.TryGetValue(
                    "autoUpdatePreviousName",
                    out var autoUpdate)
                && autoUpdate.GetBoolean();

            if (mailType == "Lotus")
            {
                bool nameChanged =
                    !string.Equals(
                        mail.Name,
                        newName,
                        StringComparison.OrdinalIgnoreCase);

                if (nameChanged)
                {
                    if (autoUpdatePreviousName)
                    {
                        mail.PreviousName = mail.Name;
                    }
                    else
                    {
                        mail.PreviousName = newPreviousName;
                    }

                    mail.Name = newName;
                }
                else
                {
                    mail.PreviousName = newPreviousName;
                }
            }
            else
            {
                mail.Name = newName;
            }

            if (data.TryGetValue(
                "ownerDisplayName",
                out var ownerDisplayNameElement))
            {
                mail.OwnerDisplayName =
                    ownerDisplayNameElement.GetString();

                if (string.IsNullOrWhiteSpace(
                    mail.OwnerDisplayName))
                {
                    mail.OwnerDisplayName = null;
                }
            }
            else
            {
                mail.OwnerDisplayName = null;
            }

            mail.DepartmentId = null;
            mail.UserId = null;
            mail.Sections.Clear();

            var ownerType =
                data["ownerType"].GetString();

            switch (ownerType)
            {
                case "department":
                    {
                        if (!data.TryGetValue(
                            "ownerId",
                            out var ownerIdElement))
                        {
                            return Results.BadRequest(
                                "ownerId is required");
                        }

                        var ownerId =
                            ownerIdElement.GetInt32();

                        var department =
                            await db.Departments.FindAsync(ownerId);

                        if (department == null)
                        {
                            return Results.BadRequest(
                                "Department not found");
                        }

                        mail.DepartmentId = ownerId;
                        mail.OwnerDisplayName = null;

                        break;
                    }

                case "section":
                    {
                        if (!data.TryGetValue(
                            "ownerIds",
                            out var ownerIdsElement))
                        {
                            return Results.BadRequest(
                                "ownerIds is required");
                        }

                        if (ownerIdsElement.ValueKind !=
                            JsonValueKind.Array)
                        {
                            return Results.BadRequest(
                                "ownerIds must be an array");
                        }

                        var sectionIds =
                            ownerIdsElement
                                .EnumerateArray()
                                .Select(x => x.GetInt32())
                                .Distinct()
                                .ToList();

                        if (!sectionIds.Any())
                        {
                            return Results.BadRequest(
                                "At least one section is required");
                        }

                        var sections =
                            await db.Sections
                                .Where(s =>
                                    sectionIds.Contains(s.Id))
                                .ToListAsync();

                        if (sections.Count != sectionIds.Count)
                        {
                            return Results.BadRequest(
                                "One or more sections not found");
                        }

                        foreach (var section in sections)
                        {
                            mail.Sections.Add(section);
                        }

                        if (sectionIds.Count <= 1)
                        {
                            mail.OwnerDisplayName = null;
                        }

                        break;
                    }

                case "user":
                    {
                        if (!data.TryGetValue(
                            "ownerId",
                            out var ownerIdElement))
                        {
                            return Results.BadRequest(
                                "ownerId is required");
                        }

                        var ownerId =
                            ownerIdElement.GetInt32();

                        var user =
                            await db.Users.FindAsync(ownerId);

                        if (user == null)
                        {
                            return Results.BadRequest(
                                "User not found");
                        }

                        mail.UserId = ownerId;
                        mail.OwnerDisplayName = null;

                        break;
                    }

                case "none":
                    {
                        mail.DepartmentId = null;
                        mail.UserId = null;
                        mail.Sections.Clear();

                        break;
                    }

                default:
                    return Results.BadRequest(
                        "Unknown owner type");
            }

            if (data.TryGetValue(
                "password",
                out var password))
            {
                mail.Password =
                    password.GetString();
            }

            if (mailType == "Gov-ua")
            {
                var existing =
                    await db.ResponsibleUsers
                        .Where(x => x.MailId == mail.Id)
                        .ToListAsync();

                db.ResponsibleUsers.RemoveRange(existing);

                if (data.TryGetValue(
                    "responsibleUserIds",
                    out var responsibleUsers))
                {
                    var ids =
                        responsibleUsers
                            .EnumerateArray()
                            .Select(x => x.GetInt32())
                            .Distinct();

                    foreach (var userId in ids)
                    {
                        db.ResponsibleUsers.Add(
                            new ResponsibleUser
                            {
                                MailId = mail.Id,
                                UserId = userId
                            });
                    }
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok();
        }
    }
}