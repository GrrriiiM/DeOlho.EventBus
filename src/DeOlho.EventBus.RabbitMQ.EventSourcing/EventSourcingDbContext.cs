using Microsoft.EntityFrameworkCore;

namespace DeOlho.EventBus.RabbitMQ.EventSourcing
{
    public class EventSourcingDbContext : DbContext
    {
        public EventSourcingDbContext(DbContextOptions<EventSourcingDbContext> options)
            :base(options)
        {
            
        }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder.UseMySql("teste");
        // }

        public DbSet<EventLog> EventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventLog>();
        }
    }
}