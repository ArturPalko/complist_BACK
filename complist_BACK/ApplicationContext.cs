namespace complist_BACK
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationContext:DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<MailType> MailTypes { get; set; }
        public DbSet<PhoneType> PhoneTypes { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Mail> Mails { get; set; }  

        public ApplicationContext(DbContextOptions<ApplicationContext> options) 
        :base(options)
        {
  
        }
    }
}
