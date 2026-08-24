using complist_BACK.Entities;
using Microsoft.EntityFrameworkCore;

namespace complist_BACK.RequestHandlers.MailService.helpers
{
    public class GetMails_endpoint
    {
        public static async Task<List<Mail>> GetData(
            string mailType,
            ApplicationContext db)
        {
            return await db.Mails
                .Where(m => m.MailType.Name == mailType)
                .Include(m => m.Department)
                .Include(m => m.Sections)
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

        public static object MapGovUa(List<Mail> mails)
        {
            return mails
                .Select(m => new
                {
                    m.Id,
                    m.Priority,
                    m.PreviousName,
                    Name = m.Name,

                    OwnerDisplayName = m.OwnerDisplayName,

                    Owner =
                        m.OwnerDisplayName
                        ?? m.Department?.Name
                        ?? (
                            m.Sections.Any()
                                ? string.Join(
                                    ", ",
                                    m.Sections.Select(s => s.Name)
                                )
                                : null
                        )
                        ?? m.User?.Department?.Name
                        ?? m.User?.Section?.Name
                        ?? "",

                    userName =
                        m.User != null
                            ? m.User.Name
                            : "",

                    OwnerId =
                        m.UserId
                        ?? m.DepartmentId
                        ?? 0,

                    OwnerIds =
                        m.Sections
                            .Select(s => s.Id)
                            .ToList(),

                    OwnerType =
                        m.User != null
                            ? "User"
                            : m.Department != null
                                ? "Department"
                                : m.Sections.Any()
                                    ? "Section"
                                    : !string.IsNullOrEmpty(m.OwnerDisplayName)
                                        ? "None"
                                        : "",

                    ResponsibleUsers =
                        m.ResponsibleUsers
                            .Select(x => new
                            {
                                id = x.UserId,
                                name = x.User.Name
                            })
                            .ToList(),

                   HasResponsible =
                        m.UserId.HasValue ||
                        m.ResponsibleUsers.Any(),

                    Sections =
                        m.Sections
                            .Select(s => new
                            {
                                id = s.Id,
                                name = s.Name,
                                departmentId = s.DepartmentId,
                                departmentName =
                                    s.Department?.Name ?? ""
                            })
                            .ToList(),

                    PasswordKnown =
                        !string.IsNullOrEmpty(m.Password),

                    DepSec = new
                    {
                        Department =
                            m.Department?.Name
                            ?? m.User?.Department?.Name
                            ?? m.Sections
                                .Select(s => s.Department?.Name)
                                .FirstOrDefault(
                                    x => !string.IsNullOrEmpty(x)
                                )
                            ?? m.User?.Section?.Department?.Name
                            ?? "",

                        Section =
                            string.Join(
                                ", ",
                                m.Sections.Select(s => s.Name)
                            )
                    }
                })
                .OrderBy(x => x.Priority)
                .ToList();
        }

        public static object MapLotus(List<Mail> mails)
        {
            return mails
                .Select(m => new
                {
                    m.Id,
                    m.Priority,
                    m.PreviousName,
                    Name = m.Name,

                    OwnerDisplayName = m.OwnerDisplayName,

                    Owner =
                        m.OwnerDisplayName
                        ?? m.User?.Name
                        ?? m.Department?.Name
                        ?? (
                            m.Sections.Any()
                                ? string.Join(", ", m.Sections.Select(s => s.Name))
                                : null
                        )
                        ?? m.User?.Department?.Name
                        ?? m.User?.Section?.Name
                        ?? "",

                    OwnerId =
                        m.UserId
                        ?? m.DepartmentId
                        ?? 0,

                    OwnerIds =
                        m.Sections
                            .Select(s => s.Id)
                            .ToList(),

                    OwnerType =
                        m.User != null
                            ? "User"
                            : m.Department != null
                                ? "Department"
                                : m.Sections.Any()
                                    ? "Section"
                                    : !string.IsNullOrEmpty(m.OwnerDisplayName)
                                        ? "None"
                                        : "",

                    Sections =
                        m.Sections
                            .Select(s => new
                            {
                                id = s.Id,
                                name = s.Name,
                                departmentId = s.DepartmentId,
                                departmentName = s.Department?.Name ?? ""
                            })
                            .ToList(),

                    ResponsibleUsers =
                        m.ResponsibleUsers
                            .Select(x => new
                            {
                                id = x.UserId,
                                name = x.User.Name
                            })
                            .ToList(),

                    PasswordKnown =
                        !string.IsNullOrEmpty(m.Password),

                    DepSec = new
                    {
                        Department =
                            m.Department?.Name
                            ?? m.User?.Department?.Name
                            ?? m.Sections
                                .Select(s => s.Department?.Name)
                                .FirstOrDefault(x => !string.IsNullOrEmpty(x))
                            ?? m.User?.Section?.Department?.Name
                            ?? "",

                        Section =
                            string.Join(
                                ", ",
                                m.Sections.Select(s => s.Name)
                            )
                    }
                })
                .OrderBy(x => x.Priority)
                .ToList();
        }
    }
}