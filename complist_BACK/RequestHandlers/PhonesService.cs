using complist_BACK.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
                     UserType = u.UserType?.Id,
                     UserPosition = u.Position?.Name,
                     UserPositionPriority = u.Position?.Priority,
                     DepartmentId = u.Department?.Id ?? u.Section?.DepartmentId,
                     DepartmentName = u.Department?.Name,
                     SectionId = u.Section?.Id,
                     SectionName = u.Section?.Name
                 },
                 Phone = new { PhoneName = p.Number, PhoneType = p.PhoneType.Name }
             }))
             .GroupBy(x => x.User) 
             .Select(g => new
             {
                 UserId = g.Key.Id,
                 UserName = g.Key.Name,
                 UserType = g.Key.UserType,
                 UserPosition = g.Key.UserPosition,
                 UserPositionPriority = g.Key.UserPositionPriority,
                 DepartmentId = g.Key.DepartmentId,
                 DepartmentName = g.Key.DepartmentName,
                 SectionId = g.Key.SectionId,
                 SectionName = g.Key.SectionName,
                 Phones = g.Select(x => x.Phone).ToList()
             })
             .ToList();


            var groupedByDepartment = userPhones
                .GroupBy(u => u.DepartmentId)
                .Select(deptGroup => new
                {
                    DepartmentId = deptGroup.Key,
                    DepartmentName = deptGroup.First().DepartmentName,
                    Users = deptGroup
                        .Where(u => u.SectionId == null)
                        .OrderBy(u => u.UserType)
                        .ThenBy(u => u.UserPositionPriority)
                        .ToList(),
                    Sections = deptGroup
                        .Where(u => u.SectionId != null)
                        .GroupBy(u => u.SectionId)
                        .Select(sectionGroup => new
                        {
                            SectionId = sectionGroup.Key,
                            SectionName = sectionGroup.First().SectionName,
                            Users = sectionGroup
                                .OrderBy(u => u.UserType)
                                .ThenBy(u => u.UserPositionPriority)
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            return Results.Json(groupedByDepartment);
        }
    }
    }






