using Microsoft.EntityFrameworkCore;
using TriggeredEmailer.Models;

namespace TriggeredEmailer.Data
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<SessionUserJoin> sessionUserJoins { get; set; }
    }
}
