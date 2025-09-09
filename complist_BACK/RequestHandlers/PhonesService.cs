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
                .ToListAsync();

             var grouped = phonesData
                                .SelectMany(p => p.Users, (phone, user) => new
                                {
                                    PhoneName = phone.Number,
                                    PhoneType = phone.PhoneType.Name,
                                    UserName = user.Name,
                                    Department = user.Department?.Name,
                                    Section = user.Section?.Name
                                })
                                .GroupBy(x => x.UserName)
                                .Select(userGroup => new
                                {
                                    UserName = userGroup.Key,
                                    Department = userGroup.First().Department,
                                    Section = userGroup.First().Section,
                                    Phones = userGroup.Select(p => new { p.PhoneName, p.PhoneType }).ToList()
                                })
                                .GroupBy(x => x.Department)
                                .Select(departmentGroup => new
                                {
                                    Department = departmentGroup.Key,
                                    Users = departmentGroup.ToList()
                                })
                                        .ToList();



            return Results.Json(grouped);
        }
    }
}






