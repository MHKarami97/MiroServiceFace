using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Api.Persistence
{
    public interface IOrderRepository
    {
        Task<Models.Order> GetOrderAsync(Guid id);
        Task<IEnumerable<Models.Order>> GetOrdersAsync();
        Task RegisterOrder(Models.Order order);

        Models.Order GetOrder(Guid id);
        void UpdateOrder(Models.Order order);
    }
}