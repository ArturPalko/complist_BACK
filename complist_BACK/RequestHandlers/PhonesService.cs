using complist_BACK.Entities;
using Microsoft.EntityFrameworkCore;

namespace complist_BACK.RequestHandlers
{
    public static class PhonesService
    {
        public static async Task<IResult> GetPhones(ApplicationContext db)
        {
            var phonesData = await db.Phones
                                     .Include(p => p.PhoneType)
                                     .Include(p => p.Users)
                                         .ThenInclude(u => u.Department)
                                     .Include(p => p.Users)
                                         .ThenInclude(u => u.Section)
                                     .Include(p => p.Users)
                                         .ThenInclude(u => u.Position)
                                     .Include(p => p.Users)
                                         .ThenInclude(u => u.UserType)
                                     .ToListAsync();

            var userPhones = phonesData
                .SelectMany(p => p.Users.Select(u => new
                {
                    User = new
                    {
                        Id = u.Id,
                        Name = u.Name,
                        UserTypeId = u.UserType?.Id,
                        UserTypePriority = u.UserType?.Id,
                        UserPosition = u.Position?.Name,
                        UserPositionPriority = u.Position?.Priority ?? int.MaxValue,
                        DepartmentId = u.Department?.Id ?? u.Section?.DepartmentId,
                        DepartmentName = u.Department?.Name ?? u.Section?.Department?.Name,
                        DepartmentPriority = u.Department != null
                            ? u.Department.PhonesPagePriority
                            : u.Section.Department.PhonesPagePriority,
                        SectionId = u.Section?.Id,
                        SectionName = u.Section?.Name
                    },
                    Phone = new { PhoneName = p.Number, PhoneType = p.PhoneType.Name }
                }))
                // групуємо по користувачу
                .GroupBy(x => x.User.Id)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = g.Select(u => u.User.Name).Distinct().FirstOrDefault(),
                    UserTypeId = g.Select(u => u.User.UserTypeId).Distinct().FirstOrDefault(),
                    UserTypePriority = g.Select(u => u.User.UserTypePriority).Min(),
                    UserPosition = g.Select(u => u.User.UserPosition).Distinct().FirstOrDefault(),
                    UserPositionPriority = g.Select(u => u.User.UserPositionPriority).Min(),
                    DepartmentId = g.Select(u => u.User.DepartmentId).Distinct().FirstOrDefault(),
                    DepartmentName = g.Select(u => u.User.DepartmentName).Distinct().FirstOrDefault(),
                    DepartmentPriority = g.Select(u => u.User.DepartmentPriority).Where(p => p.HasValue).Min(),
                    SectionId = g.Select(u => u.User.SectionId).Distinct().FirstOrDefault(),
                    SectionName = g.Select(u => u.User.SectionName).Distinct().FirstOrDefault(),
                    Phones = g.Select(x => x.Phone).Distinct().ToList()
                })
                .ToList();

            var groupedByDepartment = userPhones
                .GroupBy(u => u.DepartmentId)
                .Select(deptGroup => new
                {
                    DepartmentId = deptGroup.Key,
                    DepartmentName = deptGroup
                        .Select(u => u.DepartmentName)
                        .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? "Unknown",
                    DepartmentPriority = deptGroup
                        .Select(u => u.DepartmentPriority)
                        .Where(p => p.HasValue)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),
                    Users = deptGroup
                        .Where(u => u.SectionId == null)
                        .OrderBy(u => u.UserTypePriority)
                        .ThenBy(u => u.UserPositionPriority)
                        .ToList(),
                    Sections = deptGroup
                        .Where(u => u.SectionId != null)
                        .GroupBy(u => u.SectionId)
                        .Select(sectionGroup => new
                        {
                            SectionId = sectionGroup.Key,
                            SectionName = sectionGroup
                                .Select(u => u.SectionName)
                                .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? "Unknown",
                            Users = sectionGroup
                                .OrderBy(u => u.UserTypePriority)
                                .ThenBy(u => u.UserPositionPriority)
                                .ToList()
                        })
                        .ToList()
                })
                .OrderBy(d => d.DepartmentPriority)
                .ToList();

            return Results.Json(groupedByDepartment);
        }
    }
}