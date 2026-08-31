namespace complist_BACK
{
    using complist_BACK.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    public class ApplicationContext : DbContext
    {
        public DbSet<Login> Logins { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<MailType> MailTypes { get; set; }
        public DbSet<PhoneType> PhoneTypes { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Mail> Mails { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<ResponsibleUser> ResponsibleUsers { get; set; }


        public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
        {


        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Sections)
                .WithOne(s => s.Department)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Mail>()
                .HasOne(m => m.User)
                .WithMany(u => u.Mails)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasIndex(u => u.Name)
            .IsUnique()
            .HasFilter("[Name] IS NOT NULL AND [Name] <> ''");


            // ==========================================
            // UNIQUE NAMES
            // ==========================================

            // Посади
            modelBuilder.Entity<Position>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // Департаменти
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // Секції
            modelBuilder.Entity<Section>()
                .HasIndex(s => s.Name)
                .IsUnique();

            // Номери телефонів
            modelBuilder.Entity<Phone>()
                .HasIndex(p => p.Number)
                .IsUnique();

        } 
    }
}
