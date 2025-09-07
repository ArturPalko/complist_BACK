namespace complist_BACK
{

    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Http;
    using complist_BACK.Entities;

    public static class UserService
    {
        public static async Task<IResult> GetUsers(ApplicationContext db)
        {


      var mailsData = await db.Mails
            .Where(m => m.MailType.Name == "Gov-ua")
            .Include(m => m.Department)
            .Include(m => m.Section)
            .Include(m => m.User)
                .ThenInclude(u => u.Department)
            .Include(m => m.MailType)
            .ToListAsync(); // Завантажили у пам’ять

            var mails = mailsData.Select(m => new
            {
                MailName = m.Name,
                DepartmentOrSection = m.Department?.Name
                                      ?? m.Section?.Name
                                      ?? m.User?.Department?.Name
                                      ?? m.User?.Section?.Name
                                      ?? "",
                UserName = m.User != null ? m.User.Name : ""
            }).ToList();

            return Results.Json(mails);




        }
    }

}
