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
        public PWDbContext() : this(new DbContextOptionsBuilder<PWDbContext>().UseSqlite($"Data Source={System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\pwmanager.db"))}").Options)
        {
        }

        public PWDbContext(DbContextOptions<PWDbContext> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("User")
                .HasKey(u => u.Id);
        }
    }
}
