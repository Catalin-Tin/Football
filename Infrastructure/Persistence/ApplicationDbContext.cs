using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Football.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Match> Matches { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>().HasKey(c => new { c.City, c.Date });
            base.OnModelCreating(modelBuilder);
        }
    }
}
