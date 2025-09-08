namespace complist_BACK
{

    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Http;
    using complist_BACK.Entities;

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
                case "GOV-UA":
                    mails = mailsData.Select(m => new
                    {
                        MailName = m.Name,
                        DepartmentOrSection = m.Department?.Name
                                             ?? m.Section?.Name
                                             ?? m.User?.Department?.Name
                                             ?? m.User?.Section?.Name
                                             ?? "",
                        UserName = m.User != null ? m.User.Name : ""
                    }).ToList();
                    break;

                case "Lotus":
                    mails = mailsData.Select(m => new
                    {
                        PreviousName = m.PreviousName,
                        Name = m.Name,
                        Owner = m.Department?.Name
                              ?? m.Section?.Name
                              ?? m.User?.Department?.Name
                              ?? m.User?.Section?.Name
                              ?? m.User?.Name
                              ?? "",
                    }).ToList();
                    break;

                default:
                    return Results.Json(new { Message = "Unknown mail type" });
            }

            return Results.Json(mails);
        }
    }
}