using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using TriggeredEmailer.Models;

namespace TriggeredEmailer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var builder = modelBuilder.Entity<BillingAmount>();
            builder.Property(p => p.TotalPay).HasColumnType("decimal(18,2)");
            builder.Property(p => p.TotalHours).HasColumnType("decimal(18,2)");
            
        }

        public DbSet<SessionUserJoin> sessionUserJoins { get; set; }
        public DbSet<vwSession> vwSessions { get; set; }
        public DbSet<Staff> Staffs { get; set; }

        /// <summary>
        /// Not mapped table
        /// </summary>
        [NotMapped]
        public DbSet<BillingAmount> BillingAmounts { get; set; }
        [NotMapped]
        public DbSet<SessionStudent> SessionStudents { get; set; }
    }
}
