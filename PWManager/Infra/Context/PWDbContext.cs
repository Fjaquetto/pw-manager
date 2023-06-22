using Microsoft.EntityFrameworkCore;
using PWManager.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWManager.Infra.Context
{
    public class PWDbContext : DbContext
    {
        private string _dbPath;

        public PWDbContext(string dbPath)
        {
            this._dbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={_dbPath}");

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("User")
                .HasKey(u => u.Id);
        }
    }
}
