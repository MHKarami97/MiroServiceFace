using System;
using Order.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Polly;

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

        public void MigrateDb()
        {
            Policy
                .Handle<Exception>()
                .WaitAndRetry(10, r => TimeSpan.FromSeconds(10))
                .Execute(() => Database.Migrate());
        }

        public DbSet<Models.Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}