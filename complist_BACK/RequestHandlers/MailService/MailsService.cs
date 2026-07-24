namespace complist_BACK.RequestHandlers.MailService
{
    using Azure.Core;
    using complist_BACK.Entities;
    using complist_BACK.RequestHandlers.MailService.helpers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Text.Json;


    public static class MailsService
    {
        public static async Task<IResult> GetMails(string mailType, ApplicationContext db)
        {
            var mailsData = await GetMails_endpoint.GetData(mailType, db);

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
                return Results.BadRequest(
                    "Unknown mail type");

            var ownerType =
                data["ownerType"].GetString();

            var ownerId =
                data["ownerId"].GetInt32();

            var mails =
                await db.Mails.ToListAsync();

            foreach (var m in mails)
            {
                m.Priority++;
            }

            var newMail = new Mail
            {
                Name = data["mail"].GetString(),
                MailTypeId = typeId,
                Priority = 1,

               

                Password =
                    data.TryGetValue(
                        "password",
                        out var password)
                    ? password.GetString()
                    : null
            };

            switch (ownerType)
            {
                case "department":
                    newMail.DepartmentId = ownerId;
                    break;

                case "section":
                    newMail.SectionId = ownerId;
                    break;

                case "user":
                    newMail.UserId = ownerId;
                    break;

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
                    out var responsibleUsers)
            )
            {
                var ids =
                    responsibleUsers
                        .EnumerateArray()
                        .Select(x => x.GetInt32());

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
            var ids = await request.ReadFromJsonAsync<List<int>>();

            if (ids == null || !ids.Any())
                return Results.BadRequest("No ids provided");

            var mailsToDelete = await db.Mails
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (!mailsToDelete.Any())
                return Results.NotFound();

            db.Mails.RemoveRange(mailsToDelete);

            await db.SaveChangesAsync();

            var remainingMails = await db.Mails
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
                    .FirstOrDefaultAsync(
                        x => x.Id == id);

            if (mail == null)
                return Results.NotFound();

            var newName =
                data["mail"].GetString();

            if (!string.Equals(
                mail.Name,
                newName,
                StringComparison.OrdinalIgnoreCase))
            {
                mail.PreviousName = mail.Name;
                mail.Name = newName;
            }

            mail.DepartmentId = null;
            mail.SectionId = null;
            mail.UserId = null;

            var ownerType =
                data["ownerType"].GetString();

            var ownerId =
                data["ownerId"].GetInt32();

            switch (ownerType)
            {
                case "department":
                    mail.DepartmentId = ownerId;
                    break;

                case "section":
                    mail.SectionId = ownerId;
                    break;

                case "user":
                    mail.UserId = ownerId;
                    break;

                default:
                    return Results.BadRequest(
                        "Unknown owner type");
            }

            

            if (
                data.TryGetValue(
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

                db.ResponsibleUsers
                    .RemoveRange(existing);

                if (
                    data.TryGetValue(
                        "responsibleUserIds",
                        out var responsibleUsers))
                {
                    var ids =
                        responsibleUsers
                            .EnumerateArray()
                            .Select(x => x.GetInt32());

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
