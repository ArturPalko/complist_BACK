using complist_BACK.Entities;
using Microsoft.EntityFrameworkCore;

namespace complist_BACK.RequestHandlers.MailService.helpers
{
    public class GetMails_endpoint
    {
        public static async Task<List<Mail>> GetData(string mailType, ApplicationContext db)
        {
            return await db.Mails
                .Where(m => m.MailType.Name == mailType)
                .Include(m => m.Department)
                .Include(m => m.Section)
                    .ThenInclude(s => s.Department)
                .Include(m => m.User)
                    .ThenInclude(u => u.Department)
                .Include(m => m.User)
                    .ThenInclude(u => u.Section)
                .Include(m => m.ResponsibleUsers)
                    .ThenInclude(r => r.User)
                .Include(m => m.MailType)
                .ToListAsync();
        }

        // =========================
        // GOV-UA DTO
        // =========================
        public static object MapGovUa(List<Mail> mails)
        {
            return mails.Select(m => new
            {
                MailName = m.Name,

                DepartmentOrSection =
                    m.Department?.Name
                    ?? m.Section?.Name
                    ?? m.User?.Department?.Name
                    ?? m.User?.Section?.Name
                    ?? "",

                m.Id,
                m.Priority,

                userName = m.User != null ? m.User.Name : "",

                ResponsibleUser =
                    m.ResponsibleUsers
                        .Select(x => x.User.Name)
                        .FirstOrDefault() ?? "",

                OwnerType =
                    m.User != null ? "User"
                    : m.Department != null ? "Department"
                    : m.Section != null ? "Section"
                    : "",

                PasswordKnown = !string.IsNullOrEmpty(m.Password),

                DepSec = new
                {
                    Department =
                        m.Department?.Name
                        ?? m.User?.Department?.Name
                        ?? m.Section?.Department?.Name
                        ?? m.User?.Section?.Department?.Name
                        ?? "",

                    Section =
                        m.Section?.Name
                        ?? m.User?.Section?.Name
                        ?? ""
                }
            })
            .OrderBy(x => x.Priority)
            .ToList();
        }

        // =========================
        // LOTUS DTO
        // =========================
        public static object MapLotus(List<Mail> mails)
        {
            return mails.Select(m => new
            {
                m.Id,
                m.Priority,
                m.PreviousName,
                m.Name,

                Owner =
                    m.User?.Name
                    ?? m.Department?.Name
                    ?? m.Section?.Name
                    ?? m.User?.Department?.Name
                    ?? m.User?.Section?.Name
                    ?? "",

                OwnerType =
                    m.User != null ? "User"
                    : m.Department != null ? "Department"
                    : m.Section != null ? "Section"
                    : "",

                PasswordKnown = !string.IsNullOrEmpty(m.Password),

                DepSec = new
                {
                    Department =
                        m.Department?.Name
                        ?? m.User?.Department?.Name
                        ?? m.Section?.Department?.Name
                        ?? "",

                    Section =
                        m.Section?.Name
                        ?? m.User?.Section?.Name
                        ?? ""
                }
            })
            .OrderBy(x => x.Priority)
            .ToList();
        }
    }
}