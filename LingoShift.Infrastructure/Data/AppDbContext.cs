using Microsoft.EntityFrameworkCore;
using LingoShift.Domain.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LingoShift.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setting>().HasKey(s => s.Key);
        }

        public void MigrateDatabase()
        {
            if (Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator databaseCreator)
            {
                if (!databaseCreator.Exists())
                {
                    databaseCreator.Create();
                    databaseCreator.CreateTables();
                }
            }
        }
    }
}