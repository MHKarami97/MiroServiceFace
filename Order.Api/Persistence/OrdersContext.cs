using Order.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Order.Api.Persistence
{
    public class OrdersContext : DbContext
    {
        public OrdersContext(DbContextOptions<OrdersContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var converter = new EnumToStringConverter<Status>();
            builder
                .Entity<Models.Order>()
                .Property(p => p.Status)
                .HasConversion(converter);
        }

        public DbSet<Models.Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}