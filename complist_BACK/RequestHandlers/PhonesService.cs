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
                        .ThenInclude(s => s.Department)
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
                        UserType = u.UserType?.Name,
                        UserTypePriority = u.UserType?.Priority ?? int.MaxValue,

                        UserPositionId = u.PositionId,
                        UserPosition = u.Position?.Name,
                        UserPositionPriority = u.Position?.Priority ?? int.MaxValue,

                        DepartmentId = u.Department?.Id ?? u.Section?.DepartmentId,
                        DepartmentName = u.Department?.Name ?? u.Section?.Department?.Name,

                        DepartmentPriority = u.Department != null
                            ? u.Department.PhonesPagePriority
                            : u.Section!.Department.PhonesPagePriority,

                        SectionId = u.Section?.Id,
                        SectionName = u.Section?.Name,

                        SectionPriority = u.Section?.PhonesPagePriority ?? int.MaxValue
                    },

                    Phone = new
                    {
                        PhoneName = p.Number,
                        PhoneType = p.PhoneType.Name
                    }
                }))
                .GroupBy(x => x.User.Id)
                .Select(g => new
                {
                    UserId = g.Key,

                    UserName = g.Select(u => u.User.Name).FirstOrDefault(),

                    UserTypeId = g.Select(u => u.User.UserTypeId).FirstOrDefault(),
                    UserType = g.Select(u => u.User.UserType).FirstOrDefault(),

                    UserTypePriority = g.Select(u => u.User.UserTypePriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    UserPositionId = g.Select(u => u.User.UserPositionId).FirstOrDefault(),
                    UserPosition = g.Select(u => u.User.UserPosition).FirstOrDefault(),

                    UserPositionPriority = g.Select(u => u.User.UserPositionPriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    DepartmentId = g.Select(u => u.User.DepartmentId).FirstOrDefault(),
                    DepartmentName = g.Select(u => u.User.DepartmentName).FirstOrDefault(),

                    DepartmentPriority = g.Select(u => u.User.DepartmentPriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    SectionId = g.Select(u => u.User.SectionId).FirstOrDefault(),
                    SectionName = g.Select(u => u.User.SectionName).FirstOrDefault(),

                    SectionPriority = g.Select(u => u.User.SectionPriority)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min(),

                    Phones = g.Select(x => x.Phone)
                        .Distinct()
                        .ToList()
                })
                .ToList();

            // =========================
            // GROUP BY DEPARTMENT
            // =========================

            var groupedByDepartment = userPhones
                .GroupBy(u => u.DepartmentId)
                .Select(deptGroup => new
                {
                    DepartmentId = deptGroup.Key,

                    DepartmentName = deptGroup
                        .Select(u => u.DepartmentName)
                        .FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Unknown",

                    DepartmentPriority = deptGroup
                        .Select(u => u.DepartmentPriority)
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

                            DepartmentId = deptGroup.Key,

                            SectionName = sectionGroup
                                .Select(x => x.SectionName)
                                .FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Unknown",

                            SectionPriority = sectionGroup
                                .Select(x => x.SectionPriority)
                                .DefaultIfEmpty(int.MaxValue)
                                .Min(),

                            Users = sectionGroup
                                .OrderBy(u => u.UserTypePriority)
                                .ThenBy(u => u.UserPositionPriority)
                                .ToList()
                        })
                        .OrderBy(s => s.SectionPriority)
                        .ToList()
                })
                .OrderBy(d => d.DepartmentPriority)
                .ToList();

            return Results.Json(groupedByDepartment);
        }
    }
}