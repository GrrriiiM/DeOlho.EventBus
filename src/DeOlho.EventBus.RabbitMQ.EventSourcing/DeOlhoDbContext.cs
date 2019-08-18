using Microsoft.EntityFrameworkCore;

namespace DeOlho.EventBus.RabbitMQ.EventSourcing
{
    public class DeOlhoDbContext : DbContext
    {
        public DeOlhoDbContext(DbContextOptions options)
            :base(options)
        {
            
        }

        public DbSet<EventSourcing> EventsSourcing { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventSourcing>();
        }
    }
}