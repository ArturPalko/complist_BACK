namespace complist_BACK
{

    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Http;
    using complist_BACK.Entities;
    using Microsoft.VisualBasic;
    using System.Linq;

    public static class MailsService
    {
        public static async Task<IResult> GetMails(string mailType, ApplicationContext db)
        {
            var mailsData = await db.Mails
                .Where(m => m.MailType.Name == mailType)
                .Include(m => m.Department)
                .Include(m => m.Section)
                .Include(m => m.User)
                    .ThenInclude(u => u.Department)
                .Include(m => m.User)
                    .ThenInclude(u => u.Section)
                .Include(m => m.MailType)
                .ToListAsync();

            object mails;

            switch (mailType)
            {
                case "Gov-ua":
                     mails = await db.Mails
    .Where(m => m.MailType.Name == mailType)
    .Select(m => new
    {
        MailName = m.Name,
        DepartmentOrSection = m.Department != null ? m.Department.Name :
                              m.Section != null ? m.Section.Name :
                              m.User.Department != null ? m.User.Department.Name :
                              m.User.Section != null ? m.User.Section.Name : "",
        UserName = m.User != null ? m.User.Name : ""
    })
    .ToListAsync();

                    break;

                case "Lotus":
                    mails = mailsData.Select(m => new
                    {
                        Id = m.Id,
                        PreviousName = m.PreviousName,
                        Name = m.Name,
                        Owner = m.User?.Section?.Name
                              ?? m.User?.Name
                              ?? m.Department?.Name
                              ?? m.Section?.Name
                              ?? m.User?.Department?.Name
                              ?? "",
                    }).ToList();
                    break;

                default:
                    return Results.Json(new { Message = "Unknown mail type" });
            }

            return Results.Json(mails);
        }
        public static async Task<IResult> GetLotusPasswords(ApplicationContext db)
        {
            var passwordsData = await db.Mails.Include(m => m.MailType).ToListAsync(); ;
            var passwords = passwordsData.Where(m => m.MailType.Name == "Lotus").Select(m => new {id=m.Id, password = m.Password });
            return Results.Json(passwords);

        }
    }
}