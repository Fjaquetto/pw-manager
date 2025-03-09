using Microsoft.EntityFrameworkCore;
using PWManager.Domain.Model;

namespace PWManager.Infra.Context.SQLite
{
    public class PWDbContext : DbContext
    {
        private string _dbPath;

        public PWDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={_dbPath}");

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("User")
                .HasKey(u => u.Id);
                
            modelBuilder.Entity<User>()
                .Property(u => u.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
