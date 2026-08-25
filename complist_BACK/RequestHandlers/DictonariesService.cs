namespace complist_BACK.RequestHandlers.DictionariesService
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;

    public static class DictionariesService
    {
        public static async Task<IResult> Get(ApplicationContext db)
        {
            var phonesResult = await db.PhoneTypes
                .Select(pt => new
                {
                    id = pt.Id,
                    name = pt.Name,
                    phones = pt.Phones.Select(p => new
                    {
                        id = p.Id,
                        number = p.Number,
                        users = p.Users.Select(u => new
                        {
                            id = u.Id,
                            name = !string.IsNullOrWhiteSpace(u.Name)
                                ? u.Name
                                : $"{u.Position.Name} {u.Department.Name}" +
                                  (u.Section != null ? $" / {u.Section.Name}" : "")
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            var positions = await db.Positions
                .OrderBy(p => p.Priority)
                .Select(p => new
                {
                    id = p.Id,
                    positionName = p.Name,
                    priority = p.Priority
                })
                .ToListAsync();

            var userTypes = await db.UserTypes
                .OrderBy(t => t.Priority)
                .Select(t => new
                {
                    id = t.Id,
                    userType = t.Name,
                    priority = t.Priority
                })
                .ToListAsync();

            var departments = await db.Departments
                .OrderBy(d => d.PhonesPagePriority)
                .Select(d => new
                {
                    departmentId = d.Id,
                    departmentName = d.Name,
                    priority = d.PhonesPagePriority,
                    presentedOnPhonesPage =
                    d.Users.Any(u =>
                        u.SectionId == null &&
                        u.Phones.Any())
                    ||
                    d.Sections.Any(s =>
                        s.Users.Any(u =>
                            u.Phones.Any())),

                    users = d.Users
                        .Where(u => u.SectionId == null)
                        .Select(u => new
                        {
                            id = u.Id,
                            name = u.Name,
                            positionId = u.PositionId,
                            userTypeId = u.UserTypeId,
                            positionName = u.Position != null ? u.Position.Name : null,
                            userType = u.UserType.Name
                        })
                        .ToList(),

                    sections = d.Sections
                        .OrderBy(s => s.PhonesPagePriority)
                        .Select(s => new
                        {
                            sectionId = s.Id,
                            sectionName = s.Name,
                            sectionPriority = s.PhonesPagePriority,
                            departmentId = s.DepartmentId,

                            users = s.Users
                                .Select(u => new
                                {
                                    id = u.Id,
                                    name = u.Name,
                                    positionId = u.PositionId,
                                    positionName = u.Position != null ? u.Position.Name : null,
                                    userTypeId = u.UserTypeId,
                                    userType = u.UserType.Name
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            var users = await db.Users
                .Select(u => new
                {
                    id = u.Id,
                    name = !string.IsNullOrWhiteSpace(u.Name)
                        ? u.Name
                        : $"{u.Position.Name} {u.Department.Name}" +
                          (u.Section != null ? $" / {u.Section.Name}" : "")
                })
                .ToListAsync();

            var sections = await db.Sections
         .OrderBy(s => s.PhonesPagePriority)
         .Select(s => new
         {
             id = s.Id,
             name = s.Name,
             departmentId = s.DepartmentId
         })
         .ToListAsync();

            var deps = await db.Departments
                .OrderBy(d => d.PhonesPagePriority)
                .Select(d => new
                {
                    id = d.Id,
                    name = d.Name
                })
                .ToListAsync();

            return Results.Ok(new
            {
                phonesResult,
                positions,
                userTypes,
                departments,
                users,
                sections,
                deps
            });
        }
    }
}