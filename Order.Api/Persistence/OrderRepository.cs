using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Order.Api.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersContext _context;

        public OrderRepository(OrdersContext context)
        {
            _context = context;
        }

        public Models.Order GetOrder(Guid id)
        {
            return _context.Orders
                .Include("OrderDetails")
                .FirstOrDefault(c => c.OrderId == id);
        }

        public async Task<Models.Order> GetOrderAsync(Guid id)
        {
            return await _context.Orders
                .Include("OrderDetails")
                .FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task<IEnumerable<Models.Order>> GetOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public Task RegisterOrder(Models.Order order)
        {
            _context.Add(order);
            _context.SaveChanges();
            return Task.FromResult(true);
        }

        public void UpdateOrder(Models.Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}