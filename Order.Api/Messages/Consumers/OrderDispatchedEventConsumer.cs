using System;
using MassTransit;
using Order.Api.Models;
using Order.Api.Persistence;
using System.Threading.Tasks;
using Messaging.InterfacesConstants.Events;
using Microsoft.AspNetCore.SignalR;
using Order.Api.Hubs;

namespace Order.Api.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderDispatchedEventConsumer(IOrderRepository orderRepository, IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
            _orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            var orderId = message.OrderId;

            UpdateDatabase(orderId);

            await _hubContext.Clients.All
                .SendAsync("UpdateOrders", "Dispatched", orderId);
        }

        private void UpdateDatabase(Guid orderId)
        {
            var order = _orderRepository.GetOrder(orderId);

            if (order != null)
            {
                order.Status = Status.Sent;
                _orderRepository.UpdateOrder(order);
            }
        }
    }
}