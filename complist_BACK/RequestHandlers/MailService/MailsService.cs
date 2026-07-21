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

        public static async Task<IResult> GetMailsPasswords(string mailType, ApplicationContext db)
        {
            var passwordsData = await db.Mails
                                        .Include(m => m.MailType)
                                        .ToListAsync();

            var passwords = passwordsData
                                .Where(m => m.MailType.Name == mailType)
                                .Select(m => new { id = m.Id, password = m.Password });

            return Results.Json(passwords);
        }

        public static async Task<IResult> AddMail(
       string mailType,
       ApplicationContext db,
       HttpRequest request)
        {
            var data = await request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

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

            var ownerType = data["ownerType"].GetString();
            var ownerId = data["ownerId"].GetInt32();

            // Зсуваємо всі пріоритети вниз
            var mails = await db.Mails.ToListAsync();

            foreach (var mail in mails)
            {
                mail.Priority++;
            }

            var newMail = new Mail
            {
                Name = data["mail"].GetString(),
                MailTypeId = typeId,
                Priority = 1
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
                    return Results.BadRequest("Unknown owner type");
            }

            db.Mails.Add(newMail);

            await db.SaveChangesAsync();

            return Results.Json(newMail);
        }
    }
}
