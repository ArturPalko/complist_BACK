namespace complist_BACK.RequestHandlers.MailService
{
    using complist_BACK.RequestHandlers.MailService.helpers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

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
    }
}
