namespace complist_BACK
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
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
                .Include(m => m.ResponsibleUsers)
                    .ThenInclude(mo => mo.User)
                .Include(m => m.MailType)
                .ToListAsync();

            object mails = mailType switch
            {
                "Gov-ua" => mailsData.Select(m => new
                {
                    MailName = m.Name,
                    DepartmentOrSection = m.Department?.Name
                                          ?? m.Section?.Name
                                          ?? m.User?.Department?.Name
                                          ?? m.User?.Section?.Name
                                          ?? "",
                    Id = m.Id,
                    userName = m.User!=null ? m.User.Name : "",
                    ResponsibleUser = m.ResponsibleUsers.Select(mo => mo.User.Name).FirstOrDefault() ?? "",
                    OwnerType = m.User != null ? "User"
                                : m.Department != null ? "Department"
                                : m.Section != null ? "Section" : "",
                    PasswordKnown = !string.IsNullOrEmpty(m.Password)
                }).ToList(),

                "Lotus" => mailsData.Select(m => new
                {
                    Id = m.Id,
                    PreviousName = m.PreviousName,
                    Name = m.Name,
                    Owner = m.User?.Name
                          ?? m.Department?.Name
                          ?? m.Section?.Name
                          ?? m.User?.Department?.Name
                          ?? m.User?.Section?.Name
                          ?? "",
                    OwnerType = m.User != null ? "User"
                                : m.Department != null ? "Department"
                                : m.Section != null ? "Section" : "",
                    PasswordKnown = !string.IsNullOrEmpty(m.Password)
                }).ToList(),

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
