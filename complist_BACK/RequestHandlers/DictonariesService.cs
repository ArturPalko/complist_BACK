using Microsoft.EntityFrameworkCore;
namespace complist_BACK.RequestHandlers.DictionariesService
{
    public static class DictionariesService
    {
        public static async Task<IResult> Handle(ApplicationContext db)
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
                            name = u.Name
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

                    sections = d.Sections
                        .OrderBy(s => s.PhonesPagePriority)
                        .Select(s => new
                        {
                            sectionId = s.Id,
                            sectionName = s.Name,
                            sectionPriority = s.PhonesPagePriority,
                            departmentId = s.DepartmentId
                        })
                        .ToList()
                })
                .ToListAsync();

            return Results.Ok(new
            {
                phonesResult,
                positions,
                userTypes,
                departments
            });
        }
    }
}
