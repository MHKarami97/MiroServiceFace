using Refit;
using System;
using Web.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Web.RestClients
{
    public interface IOrderManagementApi
    {
        [Get("/orders")]
        Task<List<OrderViewModel>> GetOrders();

        [Get("/orders/{orderId}")]
        Task<OrderViewModel> GetOrderById(Guid orderId);
    }
}